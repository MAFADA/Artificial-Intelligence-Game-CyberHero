using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
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
    public float detectionRange = 3f;
    private float distanceToPlayer;
    public Transform player;
    [SerializeField] private LayerMask playerMask;
    public Transform[] patrolSpots;
    public float startWaitTime;
    private int randomSpot;
    public float stopDistance = 5;



    [Header("Attack parameters")]
    public Transform attackHit;
    public Vector3 attackRange;
    public float attackDamage = 10f;
    public float meleeDistance = 10f;
    public float meleeCooldown = 3f;
    private bool canMeleeAttack = true;
    #endregion

    #region Agent Check Variables
    private bool isGrounded = true;
    private bool facingRight = true;
    private bool isWallDetected;
    private bool isPlayerDetected;
    private bool isFollowing;
    private bool isAttackingPlayer;
    private bool isWaiting;
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
                if (!isWaiting && !isPlayerDetected)
                {
                    reward = Patrol();
                    UpdateQValue(reward);
                }
                else
                {
                    reward = 0f;
                    UpdateQValue(reward);
                }
                break;
            case "Follow":
                Collider2D detection = Physics2D.OverlapCircle(transform.position, detectionRange, playerMask);

                if (detection != null && detection.CompareTag("Player"))
                {
                    isPlayerDetected = true;
                    reward = Follow();
                    UpdateQValue(reward);
                }
                else
                {
                    isPlayerDetected = false;
                    reward = -0.2f;
                    UpdateQValue(reward);
                }
                break;
            case "Attack":
                reward = MeleeAttack();
                UpdateQValue(reward);
                break;
        }
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
        anim.SetBool("Idle", false);
        anim.SetBool("Movement", true);
        Vector2 direction = (patrolSpots[randomSpot].position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
        FlipDirection(direction);

        if (Vector2.Distance(transform.position, patrolSpots[randomSpot].position) < .2f)
        {
            anim.SetBool("Movement", false);
            anim.SetBool("Idle", true);
            StartCoroutine(WaitAtSpot());
        }

        float distanceToSpot = Vector2.Distance(transform.position, patrolSpots[randomSpot].position);
        return distanceToSpot < .2f ? 0.1f : -0.1f;
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
        isFollowing = true;
        distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        if (distanceToPlayer > stopDistance)
        {
            anim.SetBool("Idle", false);
            anim.SetBool("Movement", true);


            rb.velocity = directionToPlayer * moveSpeed;
            FlipDirection(directionToPlayer);
            return 0.1f;
        }
        /*else if (distanceToPlayer < stopDistance && distanceToPlayer > meleeDistance)
        {
            anim.SetBool("Movement", false);
            anim.SetBool("Idle", true);


            rb.velocity = Vector2.zero;
            return 0.05f;
        }*/
        return 0f;
    }

    private float MeleeAttack()
    {
        if (canMeleeAttack && distanceToPlayer <= meleeDistance)
        {
            isAttackingPlayer = true;

            anim.SetBool("Melee", true);
            canMeleeAttack = false;

            StartCoroutine(MeleeAttackCooldownCoroutine());

            Invoke(nameof(StopMeleeAnimation), 1f);

            return 1f;
        }
        isAttackingPlayer = false;
        return -0.5f;
    }
    private void StopMeleeAnimation()
    {
        anim.SetBool("Melee", false);
    }
    public void SendAttack()
    {
        Collider2D[] detection = Physics2D.OverlapBoxAll(attackHit.transform.position, attackRange, playerMask);

        foreach (Collider2D detectedObject in detection)
        {
            if (detectedObject.CompareTag("Player"))
            {
                Combat playerCombatCores = detectedObject.GetComponentInChildren<Combat>();
                if (playerCombatCores != null)
                {
                    playerCombatCores.Damage(attackDamage);
                    Debug.Log("Damage: " + attackDamage);
                }
            }
        }
    }
    private IEnumerator MeleeAttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(meleeCooldown);
        canMeleeAttack = true;
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
    private void UpdateQValue(float reward)
    {
        nextState = GetState();
        float maxFutureQ = GetMaxQ(nextState);

        float currentQ = qTable.ContainsKey($"{currentState}_{currentAction}") ? qTable[$"{currentState}_{currentAction}"] : 0f;
        float newQ = currentQ + learningRate * (reward + discountFactor * maxFutureQ - currentQ);
        qTable[$"{currentState}_{currentAction}"] = newQ;

        currentState = nextState;

        AddReward(reward);
    }


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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeDistance);   
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackHit.position, attackRange);
    }
    #endregion
}
