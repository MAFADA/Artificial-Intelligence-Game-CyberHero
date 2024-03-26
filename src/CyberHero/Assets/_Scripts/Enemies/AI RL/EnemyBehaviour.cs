using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class EnemyAgent : Agent
{
    public Transform player;
    public float detectionRadius = 10f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float skillDamage = 20f;
    public float skillCooldown = 5f;
    public float dodgeCooldown = 2f;
    public float patrolRange = 10f;
    public float patrolDuration = 10f;
    public float moveSpeed = 2f;
    public float rotationSpeed = 100f;

    private Rigidbody2D rb;
    private Vector2 workspace;
    private float attackTimer = 0f;
    private float skillTimer = 0f;
    private float dodgeTimer = 0f;
    private float patrolTimer = 0f;
    private Vector3 patrolTarget;
    private bool isFollowing = false;
    private bool isAttacking = false;
    private bool isSkilling = false;
    private bool isDodging = false;

    private int[,] QTable;
    private int currentState;
    private int nextState;
    private float reward;
    private float maxFutureQ;
    private float learningRate = 0.1f;
    private float discountFactor = 0.95f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        SetResetParameters();

        // Initialize Q-Table with zeros
        QTable = new int[16, 5];
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                QTable[i, j] = 0;
            }
        }
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
        int followPlayer = actions.DiscreteActions[0];
        int attackPlayer = actions.DiscreteActions[1];
        int useSkill = actions.DiscreteActions[2];
        int dodgePlayerAttack = actions.DiscreteActions[3];

        // Convert observations to state index
        int stateIndex = GetStateIndex();

        // Choose action based on Q-Table
        int actionIndex = ChooseAction(stateIndex);

        // Perform action
        /*PerformAction(horizontalMovement, followPlayer, attackPlayer, useSkill, dodgePlayerAttack);

        // Get next state, reward, and max future Q-Value
        nextState = GetStateIndex();
        reward = GetReward();
        maxFutureQ = GetMaxFutureQ(nextState);

        // Update Q-Table using Q-Learning algorithm
        float newQ = (1 - learningRate) * QTable[stateIndex, actionIndex] + learningRate * (reward + discountFactor * maxFutureQ);
        QTable[stateIndex, actionIndex] = (int)newQ;*/


        if (horizontalMovement == 1)
        {
            /* rb.AddForce(new Vector2(moveSpeed * 1 * Time.deltaTime, 0f));*/
            workspace.Set(moveSpeed, rb.velocity.y);
            rb.velocity = workspace;
        }
        else if (horizontalMovement == -1)
        {
            /*rb.AddForce(new Vector2(moveSpeed * -1 * Time.deltaTime, 0f));*/
            workspace.Set(-moveSpeed, rb.velocity.y);
            rb.velocity = workspace;
        }

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
            Vector2 moveDirection = (player.position - transform.position).normalized;
            rb.AddForce(moveDirection * moveSpeed);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                isFollowing = false;
                isAttacking = true;
                isSkilling = false;
                isDodging = false;
            }
        }

        if (attackPlayer == 1 && !isAttacking)
        {
            isAttacking = true;
            isFollowing = false;
            isSkilling = false;
            isDodging = false;

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.gameObject == player.gameObject)
                {
                    /*player.GetComponent<Health>().TakeDamage(attackDamage);*/
                    break;
                }
            }
        }

        if (useSkill == 1 && !isSkilling)
        {
            isSkilling = true;
            isFollowing = false;
            isAttacking = false;
            isDodging = false;

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.gameObject == player.gameObject)
                {
                    /*player.GetComponent<Health>().TakeDamage(skillDamage);*/
                    break;
                }
            }
        }

        if (dodgePlayerAttack == 1 && !isDodging)
        {
            isDodging = true;
            isFollowing = false;
            isAttacking = false;
            isSkilling = false;

            rb.AddForce(new Vector2(horizontalMovement * moveSpeed * 2f, rb.velocity.y));
        }

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
        base.Heuristic(actionsOut);

        var continuousActionsOut = actionsOut.ContinuousActions;

        // Horizontal movement
        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");

        var discreteActionsOut = actionsOut.DiscreteActions;

        // Follow player
        if (Input.GetKeyDown(KeyCode.F))
        {
            discreteActionsOut[0] = 1;
        }

        // Attack player
        if (Input.GetKeyDown(KeyCode.K))
        {
            discreteActionsOut[1] = 1;
        }

        // Use skill
        if (Input.GetKeyDown(KeyCode.P))
        {
            discreteActionsOut[2] = 1;
        }

        // Dodge player attack
        if (Input.GetKeyDown(KeyCode.O))
        {
            discreteActionsOut[3] = 1;
        }
    }

    private int GetStateIndex()
    {
        int horizontalDirection = (int)Mathf.Sign(player.position.x - transform.position.x);
        int isPlayerDetected = (Vector2.Distance(transform.position, player.position) <= detectionRadius) ? 1 : 0;
        int isPlayerInAttackRange = (Vector2.Distance(transform.position, player.position) <= attackRange) ? 1 : 0;
        /*int isPlayerAttacking = player.GetComponent<Player>().GetIsAttacking() ? 1 : 0;*/

        /*int stateIndex  = horizontalDirection * 4 + isPlayerDetected * 1 + isPlayerInAttackRange * 8 + isPlayerAttacking * 16;*/
        int stateIndex = 1;
        return stateIndex;
    }

    private int ChooseAction(int stateIndex)
    {
        int[] possibleActions = new int[5];
        int actionIndex = 0;

        if (stateIndex == 0 || stateIndex == 1 || stateIndex == 2 || stateIndex == 3)
        {
            possibleActions[actionIndex] = 0;
            actionIndex++;
        }

        if (stateIndex == 8 || stateIndex == 9 || stateIndex == 10 || stateIndex == 11)
        {
            possibleActions[actionIndex] = 1;
            actionIndex++;
        }

        if (stateIndex == 1 || stateIndex == 3 || stateIndex == 9 || stateIndex == 11)
        {
            possibleActions[actionIndex] = 2;
            actionIndex++;
        }

        if (stateIndex == 0 || stateIndex == 2 || stateIndex == 8 || stateIndex == 10)
        {
            possibleActions[actionIndex] = 3;
            actionIndex++;
        }

        if (stateIndex == 12 || stateIndex == 13 || stateIndex == 14 || stateIndex == 15)
        {
            possibleActions[actionIndex] = 4;
            actionIndex++;
        }

        int randomActionIndex = Random.Range(0, actionIndex);

        return possibleActions[randomActionIndex];
    }

    private void PerformAction(int horizontalMovement, int followPlayer, int attackPlayer, int useSkill, int dodgePlayerAttack)
    {
        if (horizontalMovement == 1)
        {
            rb.AddForce(new Vector2(moveSpeed, 0f));
        }
        else if (horizontalMovement == -1)
        {
            rb.AddForce(new Vector2(-moveSpeed, 0f));
        }

        if (followPlayer == 1 && !isFollowing)
        {
            isFollowing = true;
            isAttacking = false;
            isSkilling = false;
            isDodging = false;
        }

        if (isFollowing)
        {
            Vector2 moveDirection = (player.position - transform.position).normalized;
            rb.AddForce(moveDirection * moveSpeed);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                isFollowing = false;
                isAttacking = true;
                isSkilling = false;
                isDodging = false;
            }
        }

        if (attackPlayer == 1 && !isAttacking)
        {
            isAttacking = true;
            isFollowing = false;
            isSkilling = false;
            isDodging = false;

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.gameObject == player.gameObject)
                {
                    /*player.GetComponent<Health>().TakeDamage(attackDamage);*/
                    break;
                }
            }
        }

        if (useSkill == 1 && !isSkilling)
        {
            isSkilling = true;
            isFollowing = false;
            isAttacking = false;
            isDodging = false;

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.gameObject == player.gameObject)
                {
                    /*player.GetComponent<Health>().TakeDamage(skillDamage);*/
                    break;
                }
            }
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

      /*  if (isAttacking && player.GetComponent<PlayerHealth>().IsDead())
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
        }*/

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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}