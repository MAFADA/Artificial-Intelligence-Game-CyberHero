using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class EnemyAgent : Agent
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

    #region Enemy Data
    public Transform player;
    public float detectionRadius = 10f;
    public float closeRange = 2f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float skillDamage = 20f;
    public float skillCooldown = 5f;
    public float dodgeCooldown = 2f;
    public float patrolRange = 10f;
    public float patrolDuration = 10f;
    public float moveSpeed = 2f;
    public float rotationSpeed = 100f;
    public int facingDirection;

    public Transform firePoint;
    public GameObject bulletPrefab;
    #endregion

    #region Components
    private Rigidbody2D rb;
    private Animator anim;
    #endregion

    private Vector2 workspace;

    #region Timer Variables
    private float attackTimer = 0f;
    private float skillTimer = 0f;
    private float dodgeTimer = 0f;
    private float patrolTimer = 0f;
    #endregion

    private Vector3 patrolTarget;

    #region Check Variables
    private bool isFollowing = false;
    private bool isAttacking = false;
    private bool isSkilling = false;
    private bool isDodging = false;
    private bool isGrounded;
    private bool isWallDetected;
    #endregion

    #region Q-Learning Variables
    private int[,] QTable;
    private int currentState;
    private int nextState;
    private float reward;
    private float maxFutureQ;
    private float learningRate = 0.1f;
    private float discountFactor = 0.95f;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        Core = GetComponentInChildren<Core>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        facingDirection = 1;
    }

    private void Update()
    {
        Core.LogicUpdate();
    }
    #endregion

    #region MLAgent Callbacks
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        //SetResetParameters();

        // Initialize Q-Table with zeros
        QTable = new int[12, 5];
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                QTable[i, j] = 0;
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(player.position);
        sensor.AddObservation(isFollowing);
        sensor.AddObservation(isAttacking);
        sensor.AddObservation(isSkilling);
        sensor.AddObservation(isDodging);
        sensor.AddObservation(dodgeTimer);
        sensor.AddObservation(attackTimer);
        sensor.AddObservation(skillTimer);
        sensor.AddObservation(patrolTimer);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int horizontalMovement = (int)actions.ContinuousActions[0];
        //int horizontalMovement = actions.DiscreteActions[4];
        int followPlayer = actions.DiscreteActions[0];
        int attackPlayer = actions.DiscreteActions[1];
        int useSkill = actions.DiscreteActions[2];
        int dodgePlayerAttack = actions.DiscreteActions[3];

        // Convert observations to state index
        int stateIndex = GetStateIndex();

        // Choose action based on Q-Table
        int actionIndex = ChooseAction(stateIndex);

        // Perform action
        /* PerformAction(horizontalMovement, followPlayer, attackPlayer, useSkill, dodgePlayerAttack);

        // Get next state, reward, and max future Q-Value
        nextState = GetStateIndex();
        reward = GetReward();
        maxFutureQ = GetMaxFutureQ(nextState);

        // Update Q-Table using Q-Learning algorithm
        float newQ = (1 - learningRate) * QTable[stateIndex, actionIndex] + learningRate * (reward + discountFactor * maxFutureQ);
        QTable[stateIndex, actionIndex] = (int)newQ;*/

        #region Test Action
        // TODO : Idle Anim, Idle time
        if (horizontalMovement == 1)
        {
            isGrounded = CollisionSenses.Ground;
            isWallDetected = CollisionSenses.WallFront;

            if (!isGrounded || isWallDetected)
            {
                anim.SetBool("movement", false);
                Flip();
            }
            else
            {
                anim.SetBool("movement", true);
                workspace.Set(moveSpeed * facingDirection, rb.velocity.y);
                rb.velocity = workspace;
            }

        }


        // TODO: IDLE Anim n Time
        if (followPlayer == 1 && !isFollowing)
        {
            isFollowing = true;
            isAttacking = false;
            isSkilling = false;
            isDodging = false;
        }

        if (isFollowing)
        {
            Debug.Log("FOLLOW PLAYER");
            Debug.Log(isFollowing);

            Vector2 moveDirection = (player.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance < detectionRadius && distance > closeRange)
            {
                workspace.Set(moveDirection.x * moveSpeed, rb.velocity.y);
                rb.velocity = workspace;

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else if (distance <= attackRange)
            {
                isFollowing = false;
                isAttacking = true;
                isSkilling = false;
                isDodging = false;
            }
        }

        // TODO ATTACK LOGIC, ANIM ATTACK, BULLET DAMAGE DONE
        if (attackPlayer == 1 && !isAttacking)
        {
            Debug.Log("Attacking Player");
            isAttacking = true;
            isFollowing = false;
            isSkilling = false;
            isDodging = false;

            Shoot();
        }

        // TODO: LASER SKILL
        if (useSkill == 1 && !isSkilling)
        {
            Debug.Log("Skill Used");

             isSkilling = true;
             isFollowing = false;
             isAttacking = false;
             isDodging = false;

            
        }

        if (dodgePlayerAttack == 1 && !isDodging)
        {
            isDodging = true;
            isFollowing = false;
            isAttacking = false;
            isSkilling = false;

            rb.AddForce(new Vector2(horizontalMovement * moveSpeed * 2f, rb.velocity.y));
        }
        #endregion

        // Reset timers andflags
        if (attackPlayer == 1)
        {
            attackTimer = skillCooldown;
        }

        if (useSkill == 1)
        {
            skillTimer = skillCooldown;
        }

        if (dodgePlayerAttack == 1)
        {
            dodgeTimer = dodgeCooldown;
        }

        if (patrolTimer > 0f)
        {
            patrolTimer -= Time.deltaTime;
        }
        else
        {
            patrolTarget = transform.position + new Vector3(Random.Range(-patrolRange, patrolRange), 0f, Random.Range(-patrolRange, patrolRange));
            patrolTimer = patrolDuration;
        }

        isFollowing = false;
        isAttacking = false;
        isSkilling = false;
        isDodging = false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {

        var continuousActionsOut = actionsOut.ContinuousActions;

        // Horizontal movement
        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");


        #region DiscreteActions
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Follow player
        if (Input.GetKeyDown(KeyCode.F))
        {
            discreteActionsOut[0] = 1;
            Debug.Log(discreteActionsOut[0]);
        }

        // Attack
        if (Input.GetKeyDown(KeyCode.Q))
        {
            discreteActionsOut[1] = 1;
        }

        // Skill
        if (Input.GetKeyDown(KeyCode.P))
        {
            discreteActionsOut[2] = 1;
        }

        // Dodge
        if (Input.GetKeyDown(KeyCode.O))
        {
            discreteActionsOut[3] = 1;
        }

        #endregion

    }

    #endregion

    #region Other Functions

    private void Shoot()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    private void Flip()
    {
        facingDirection *= -1;
        transform.Rotate(.0f, 180.0f, .0f);
    }

    private int GetStateIndex()
    {
        int horizontalDirection = (int)Mathf.Sign(player.position.x - transform.position.x);
        int isPlayerDetected = (Vector2.Distance(transform.position, player.position) <= detectionRadius) ? 1 : 0;
        int isPlayerInAttackRange = (Vector2.Distance(transform.position, player.position) <= attackRange) ? 1 : 0;
        int isPlayerAttacking = player.GetComponent<Player>().GetIsAttacking() ? 1 : 0;


        int stateIndex = horizontalDirection * 3 + isPlayerDetected * 1 + isPlayerInAttackRange * 4 + isPlayerAttacking * 8;

        return stateIndex;
    }

    private int ChooseAction(int stateIndex)
    {
        int[] possibleActions = new int[5];
        int actionIndex = 0;

        if (stateIndex == 0 || stateIndex == 1 || stateIndex == 2)
        {
            possibleActions[actionIndex] = 0;
            actionIndex++;
        }

        if (stateIndex == 4 || stateIndex == 5 || stateIndex == 6 || stateIndex == 7)
        {
            possibleActions[actionIndex] = 1;
            actionIndex++;
        }

        if (stateIndex == 1 || stateIndex == 2 || stateIndex == 5 || stateIndex == 6)
        {
            possibleActions[actionIndex] = 2;
            actionIndex++;
        }
        if (stateIndex == 0 || stateIndex == 2 || stateIndex == 4 || stateIndex == 6)
        {
            possibleActions[actionIndex] = 3;
            actionIndex++;
        }

        if (stateIndex == 8 || stateIndex == 9 || stateIndex == 10)
        {
            possibleActions[actionIndex] = 4;
            actionIndex++;
        }

        int randomActionIndex = Random.Range(0, actionIndex);

        return possibleActions[randomActionIndex];
    }

    private void PerformAction(int horizontalMovement, int followPlayer, int attackPlayer, int useSkill, int dodgePlayerAttack)
    {
        
        // TODO : Idle Anim, Idle time
        if (horizontalMovement == 1)
        {
            isGrounded = CollisionSenses.Ground;
            isWallDetected = CollisionSenses.WallFront;

            if (!isGrounded || isWallDetected)
            {
                anim.SetBool("movement", false);
                Flip();
            }
            else
            {
                anim.SetBool("movement", true);
                workspace.Set(moveSpeed * facingDirection, rb.velocity.y);
                rb.velocity = workspace;
            }

        }


        // FOLLOW PLAYER LOGIC DONE
        // TODO: IDLE Anim n Time
        if (followPlayer == 1 && !isFollowing)
        {
            isFollowing = true;
            isAttacking = false;
            isSkilling = false;
            isDodging = false;
        }

        if (isFollowing)
        {
            Debug.Log("FOLLOW PLAYER");
            Debug.Log(isFollowing);

            Vector2 moveDirection = (player.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance < detectionRadius && distance > closeRange)
            {
                workspace.Set(moveDirection.x * moveSpeed, rb.velocity.y);
                rb.velocity = workspace;

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else if (distance <= attackRange)
            {
                isFollowing = false;
                isAttacking = true;
                isSkilling = false;
                isDodging = false;
            }
        }

        // TODO ATTACK LOGIC, ANIM ATTACK, BULLET DAMAGE DONE
        if (attackPlayer == 1 && !isAttacking)
        {
            Debug.Log("Attacking Player");
            isAttacking = true;
            isFollowing = false;
            isSkilling = false;
            isDodging = false;

            Shoot();
        }

        if (useSkill == 1 && !isSkilling)
        {
            Debug.Log("Skill Used");

            /* isSkilling = true;
             isFollowing = false;
             isAttacking = false;
             isDodging = false;

             Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
             foreach (Collider2D hitCollider in hitColliders)
             {
                 if (hitCollider.gameObject == player.gameObject)
                 {
                     *//*player.GetComponent<Health>().TakeDamage(skillDamage);*//*
                     break;
                 }
             }*/
        }

        if (dodgePlayerAttack == 1 && !isDodging)
        {
            isDodging = true;
            isFollowing = false;
            isAttacking = false;
            isSkilling = false;

            rb.AddForce(new Vector2(horizontalMovement * moveSpeed * 2f, rb.velocity.y));
        }
    }

    private float GetReward()
    {
        float reward = 0f;

        if (isAttacking && player.GetComponent<PlayerHealth>().IsDead())
        {
            reward = 1f;
        }
        else if (isSkilling && player.GetComponent<PlayerHealth>().IsDead())
        {
            reward = 0.5f;
        }
        else if (isDodging && player.GetComponent<Player>().GetIsAttacking())
        {
            reward = 0.2f;
        }
        else if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            reward = -0.01f;
        }
        else if (Vector2.Distance(transform.position, player.position) <= detectionRadius)
        {
            reward = 0.01f;
        }
        else
        {
            reward = -0.05f;
        }

        return reward;
    }

    private float GetMaxFutureQ(int nextState)
    {
        float maxFutureQ = int.MinValue;

        for (int i = 0; i < 5; i++)
        {
            float q = QTable[nextState, i];
            if (q > maxFutureQ)
            {
                maxFutureQ = q;
            }
        }

        return maxFutureQ;
    }

    private void SetResetParameters()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = new Vector3(-21.5f, 0f, -1.5f);
        attackTimer = 0f;
        skillTimer = 0f;
        dodgeTimer = 0f;
        patrolTimer = 0f;
        isFollowing = false;
        isAttacking = false;
        isSkilling = false;
        isDodging = false;
    }
    #endregion

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}