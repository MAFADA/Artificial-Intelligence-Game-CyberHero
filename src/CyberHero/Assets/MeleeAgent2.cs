using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class MeleeAgent2 : Agent
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

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 workspace;
    private int facingDirection = 1;
    #endregion

    #region Q-Learning Parameters
    [Header("QLearning Parameters")]
    private Dictionary<string, float> qTable = new Dictionary<string, float>();
    [SerializeField] private float learningRate = 0.1f;
    [SerializeField] private float discountFactor = 0.99f;
    [SerializeField] private float explorationRate = 0.1f;

    private string currentState;
    private string nextState;
    private string currentAction;
    private string[] actions = new string[] { "Patrol", "Follow", "Attack" };
    #endregion

    #region Enemy Parameter
    [Header("Movement & Detection")]
    public float moveSpeed = 2f;
    public Transform raycast;
    private RaycastHit2D hit;
    public float detectionRange = 3f;
    private float distanceToPlayer;
    public Transform player;
    [SerializeField] private LayerMask playerMask;
    public Transform[] patrolSpots;
    public float startWaitTime;
    private int randomSpot;
    public float stopDistance = 5;
    public float fleeDistance = 3;



    [Header("Attack parameters")]
    public float attackDamage = 10f;
    public float attackRange = 3f;
    public float cooldownAttack = 5f;
    private float intTimer;
    #endregion

    #region Agent Check Variables
    private bool isGrounded = true;
    private bool facingRight = true;
    private bool isWallDetected;
    private bool isPlayerDetected;
    private bool isFollowing;
    private bool isAttackingPlayer;
    private bool isWaiting;
    private bool isCoolingDown;
    #endregion

    private string GetState()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        string state = $"D:{distanceToPlayer:F1}_PD:{isPlayerDetected}_FL:{isFollowing}_W:{isWaiting}_G:{isGrounded}_W:{isWallDetected}_F:{facingRight}_AT:{isAttackingPlayer}";

        return state;
    }
    #region Unity Callbacks
    private void Awake()
    {
        Core = GetComponentInChildren<Core>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        intTimer = cooldownAttack;
        randomSpot = Random.Range(0, patrolSpots.Length);
    }

    private void Update()
    {
        Core.LogicUpdate();
        if (isPlayerDetected)
        {
            Vector2 rayDirection = facingRight ? Vector2.right : Vector2.left;
            hit = Physics2D.Raycast(raycast.position, rayDirection, detectionRange, playerMask);
            RaycastDebugger(rayDirection);
        }
    }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.gameObject.tag == "Player")
        {
            isPlayerDetected = true;
        }
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
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        sensor.AddObservation(distanceToPlayer);
        sensor.AddObservation(isGrounded);
        sensor.AddObservation(isWallDetected);
        sensor.AddObservation(isPlayerDetected);
        sensor.AddObservation(facingRight);
        sensor.AddObservation(isAttackingPlayer);
        sensor.AddObservation(isFollowing);
        sensor.AddObservation(isWaiting);
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
                reward = Patrol();
                break;
            case "Follow":
                reward = Follow();
                break;
            case "Attack":
                reward = Attack();
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
        if (!isWaiting)
        {
            anim.SetBool("Movement", true);
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
        return 0f;
    }

    private IEnumerator WaitAtSpot()
    {
        anim.SetBool("Movement", false);

        isWaiting = true;

        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(startWaitTime);

        randomSpot = Random.Range(0, patrolSpots.Length);
        isWaiting = false;
    }

    private float Follow()
    {
        if (hit.collider != null)
        {

            isFollowing = true;
            distanceToPlayer = Vector2.Distance(transform.position, player.position);
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            if (distanceToPlayer > stopDistance)
            {
                anim.SetBool("Movement", true);
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    rb.velocity = directionToPlayer * moveSpeed;
                    FlipDirection(directionToPlayer);
                }
                StopAttack();
                return 0.1f;
            }
            else if (distanceToPlayer < stopDistance && distanceToPlayer > fleeDistance)
            {
                anim.SetBool("Movement", false);
                rb.velocity = Vector2.zero;
                StopAttack();

                return 0.05f;
                /*float shootReward = ShootLaser();
                return 0.05f + shootReward;*/
            }
        }
        else if (hit.collider == null)
        {
            isFollowing = false;
            isPlayerDetected = false;
        }

        if (isPlayerDetected == false)
        {
            anim.SetBool("Movement", false);
            StopAttack();
        }

        return -0.2f;
    }

    private float Attack()
    {
        if (isCoolingDown)
        {
            Cooldown();
            anim.SetBool("Attack", false);
            return -.1f;
        }
        /*   cooldownAttack = intTimer;*/
        isAttackingPlayer = true;

        anim.SetBool("Movement", false);
        anim.SetBool("Attack", true);

        return 1f;

    }

    private void StopAttack()
    {
        isCoolingDown = false;
        isAttackingPlayer = false;
        anim.SetBool("Attack", false);
    }

    private void Cooldown()
    {
        cooldownAttack -= Time.deltaTime;

        if (cooldownAttack <= 0 && isCoolingDown && isAttackingPlayer)
        {
            isCoolingDown = false;
            cooldownAttack = intTimer;
        }
    }

    public void TriggerCooldown()
    {
        isCoolingDown = true;
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

    private void RaycastDebugger(Vector2 rayDirection)
    {
        if (distanceToPlayer > attackRange)
        {
            Debug.DrawRay(raycast.position, rayDirection * detectionRange, Color.red);
        }
        else if (attackRange > distanceToPlayer)
        {
            Debug.DrawRay(raycast.position, rayDirection * detectionRange, Color.green);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + fleeDistance, transform.position.y, transform.position.z));
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + stopDistance, transform.position.y, transform.position.z));
    }
    #endregion
}
