using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class FlyDroneAgent : Agent
{
    #region Cores
    public Core Core { get; private set; }

    protected Movement Movement { get => movement ?? Core.GetCoreComponent(ref movement); }
    private Movement movement;

    private CollisionSenses CollisionSenses
    {
        get => collisionSenses ??= Core.GetCoreComponent(ref collisionSenses);
    }
    private CollisionSenses collisionSenses;
    #endregion

    #region Components
    private Animator anim;
    private Rigidbody2D rb;
    private Vector2 workspace;
    private int facingDirection = 1;
    #endregion


    #region Q-Learning Parameters

    private Dictionary<string, float> qTable = new Dictionary<string, float>();
    [SerializeField] private float learningRate = 0.1f;
    [SerializeField] private float discountFactor = 0.99f;
    [SerializeField] private float explorationRate = 0.1f;

    private string currentState;
    private string nextState;
    private string currentAction;
    private string[] actions = new string[] { "Patrol", "Follow", "ShootLaser" };
    #endregion

    #region Enemy Parameters
    [Header("Movement & Detection")]
    public Transform player;
    public LayerMask playerLayer;
    public float moveSpeed = 2.0f;
    public Transform[] patrolSpots;
    public float startWaitTime;
    private int randomSpot;
    public float stopDistance = 20f;
    public float fleeDistance = 10f;
    public float detectionRange = 3f;


    [Header("Attack parameters")]
    public GameObject laserPrefab;
    public Transform firepoint;
    public float laserRange = 5.0f;
    private bool canShoot = true;
    private float shootCooldown = 2.0f;

    private bool isGrounded = false;
    private bool isWallDetected;
    private bool facingRight = true;
    private bool isPlayerDetected;
    private bool isWaiting = false;
    #endregion

    private string GetState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        string state = $"D:{distanceToPlayer:F1}_PD:{isPlayerDetected}_G:{isGrounded}_W:{isWaiting}_W:{isWallDetected}_F:{facingRight}_S:{canShoot}";

        return state;
    }

    #region Unity Callbacks
    private void Awake()
    {
        Core = GetComponentInChildren<Core>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        randomSpot = Random.Range(0, patrolSpots.Length);
    }

    private void Update()
    {
        Core.LogicUpdate();
    }
    #endregion

    #region MLAgent Functions
    public override void OnEpisodeBegin()
    {
        transform.position = new Vector3(Random.Range(-5f, 5f), transform.position.y, transform.position.z);

        qTable.Clear();
        currentState = GetState();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        sensor.AddObservation(distanceToPlayer);
        sensor.AddObservation(isGrounded);
        sensor.AddObservation(isWallDetected);
        sensor.AddObservation(isPlayerDetected);
        sensor.AddObservation(isWaiting);
        sensor.AddObservation(facingRight);
        sensor.AddObservation(canShoot);

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        currentAction = actions[actionBuffers.DiscreteActions[0]];

        isGrounded = CollisionSenses.Ground;
        isWallDetected = CollisionSenses.WallFront;
        float reward = -0.01f;

        switch (currentAction)
        {
            case "Patrol":
                if (!isWaiting && !isPlayerDetected)
                {
                    reward = Patrol();
                }
                else
                {
                    reward = 0f;
                }
                break;
            case "Follow":
                Collider2D detection = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

                if (detection != null && detection.CompareTag("Player"))
                {
                    isPlayerDetected = true;
                    reward = Follow();
                }
                else
                {
                    isPlayerDetected = false;
                    reward = -0.2f;
                }
                break;
            case "ShootLaser":
                reward = ShootLaser();
                break;
        }

        nextState = GetState();
        float maxFutureQ = GetMaxQ(nextState);

        float currentQ = qTable.ContainsKey($"{currentState}_{currentAction}") ? qTable[$"{currentState}_{currentAction}"] : 0f;
        float newQ = currentQ + learningRate * (reward + discountFactor * maxFutureQ - currentQ);
        qTable[$"{currentState}_{currentAction}"] = newQ;

        currentState = nextState;

        AddReward(reward);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActions[0] = 0;
        }

        else if (Input.GetKey(KeyCode.F))
        {
            discreteActions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            discreteActions[0] = 2;
        }
    }
    #endregion

    #region Action Functions
    private float Patrol()
    {

        Vector2 direction = (patrolSpots[randomSpot].position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
        FlipDirection(direction);

        if (Vector2.Distance(transform.position, patrolSpots[randomSpot].position) < .2f)
        {
            StartCoroutine(WaitAtSpot());
        }

        float distanceToSpot = Vector2.Distance(transform.position, patrolSpots[randomSpot].position);
        return distanceToSpot < .2f ? 0.1f : -0.1f;

    }

    private IEnumerator WaitAtSpot()
    {
        isWaiting = true;

        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(startWaitTime);

        randomSpot = Random.Range(0, patrolSpots.Length);
        isWaiting = false;
    }

    private float Follow()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        if (distanceToPlayer > stopDistance)
        {
            rb.velocity = directionToPlayer * moveSpeed;
            FlipDirection(directionToPlayer);
            return 0.1f;
        }
        else if (distanceToPlayer < stopDistance && distanceToPlayer > fleeDistance)
        {
            rb.velocity = Vector2.zero;
            return 0.05f;
            /*float shootReward = ShootLaser();
            return 0.05f + shootReward;*/
        }
        else if (distanceToPlayer < fleeDistance)
        {
            rb.velocity = -directionToPlayer * moveSpeed;
            FlipDirection(-directionToPlayer);
            return -0.1f;
        }
        return 0f;
    }

    private float ShootLaser()
    {
        if (!canShoot) return -1.0f;

        Collider2D laserDetection = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (laserDetection != null && laserDetection.CompareTag("Player"))
        {
            isPlayerDetected = true;
            Instantiate(laserPrefab, firepoint.position, firepoint.rotation);
            canShoot = false;
            Invoke(nameof(ResetShoot), shootCooldown);
            return 2.0f;
        }
        else
        {
            isPlayerDetected = false;

            return -.05f;
        }
    }

    private void ResetShoot()
    {
        canShoot = true;
    }

    private void FlipDirection(Vector2 direction)
    {
        if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
        {
            facingRight = !facingRight;
            transform.Rotate(.0f, 180.0f, .0f);
        }
    }
    #endregion

    #region Other Functions
    private float GetMaxQ(string state)
    {
        float maxQ = float.MinValue;
        foreach (var action in actions)
        {
            string key = $"{state}_{action}";
            if (qTable.ContainsKey(key))
            {
                maxQ = Mathf.Max(maxQ, qTable[key]);
            }
            else
            {
                maxQ = Mathf.Max(maxQ, 0f);
            }
        }
        return maxQ;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, laserRange);
    }
    #endregion
}
