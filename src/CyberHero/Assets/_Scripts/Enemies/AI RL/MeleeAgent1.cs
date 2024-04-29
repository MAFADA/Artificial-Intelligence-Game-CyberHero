using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MeleeEnemyAgent1 : Agent
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

    #endregion

    #region Agent Parameter
    private Vector2 workspace;
    private int facingDirection;
    public float moveSpeed = 2f;

    public float detectionRange = 3f;
    [SerializeField] private LayerMask whatIsTargetDetection;
    [SerializeField] private Transform targetToFollow;

    public GameObject attackPoint;
    public float attackDamage = 10f;
    public float attackRange = 3f;
    public float cooldownAttack = 5f;
    private float nextAttackTime;
    #endregion

    #region Agent Check Variables
    private bool isGrounded;
    private bool isWallDetected;
    private bool isPlayerDetected;
    private bool isFollowing;
    private bool isAttackingPlayer;
    private bool isUsingSkill;
    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        Core = GetComponentInChildren<Core>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Core.LogicUpdate();
    }

    #endregion

    #region MLAgents Functions

    public override void OnEpisodeBegin()
    {
        ResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);
        sensor.AddObservation(targetToFollow.position);
        sensor.AddObservation(isGrounded);
        sensor.AddObservation(isWallDetected);
        sensor.AddObservation(isFollowing);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // TODO: Make State Switch for Changing State of Agent

        int movementAction = actions.DiscreteActions[0];
        int attackAction = actions.DiscreteActions[1];

        #region Movement Action
        isGrounded = CollisionSenses.Ground;
        isWallDetected = CollisionSenses.WallFront;

        if (!isGrounded || isWallDetected)
        {
            anim.SetBool("Movement", false);
            Flip();
        }
        else if (movementAction == 1)
        {
            anim.SetBool("Movement", true);
            workspace.Set(movementAction * moveSpeed * facingDirection, rb.velocity.y);
            rb.velocity = workspace;
        }
        #endregion

        #region Detection & Following
        if (Physics2D.OverlapCircle(transform.position, detectionRange, whatIsTargetDetection) && movementAction == 0)
        {
            isPlayerDetected = true;
            isFollowing = true;

            Vector2 moveDirection = (targetToFollow.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, targetToFollow.position);

            if (isFollowing && distance < detectionRange)
            {
                workspace.Set(movementAction * moveDirection.x * moveSpeed, rb.velocity.y);
                rb.velocity = workspace;

                //TODO : Facing Towards Target
                if (moveDirection.x > 0 && facingDirection < 0)
                {
                    // If moving right but facing left, flip
                    Flip();
                }
                else if (moveDirection.x < 0 && facingDirection > 0)
                {
                    // If moving left but facing right, flip
                    Flip();
                }

            }
            AddReward(0.2f);
        }
        else
        {
            isPlayerDetected = false;
            isFollowing = false;
            AddReward(-0.1f);
        }

        if (Physics2D.Raycast(transform.position, Vector2.right * facingDirection, attackRange, whatIsTargetDetection))
        {

            rb.velocity = Vector2.zero;
            isAttackingPlayer = true;
            if (isAttackingPlayer && attackAction == 0)
            {
                if (Time.time >= nextAttackTime)
                {
                    anim.SetBool("Attack", true);
                    nextAttackTime = Time.time + cooldownAttack;
                }

            }
        }
        else
        {
            isAttackingPlayer = false;
        }


        if (targetToFollow.GetComponentInChildren<Combat>().IsDead())
        {
            SetReward(10f);
            EndEpisode();
        }
        #endregion

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {

    }

    #endregion

    public void MeleeAttack()
    {
        Collider2D[] detection = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRange, whatIsTargetDetection);

        foreach (Collider2D detectedObject in detection)
        {
            Debug.Log("Detected: "+detection.Length);
            detectedObject.GetComponentInChildren<Combat>().Damage(attackDamage);
            Debug.Log("Damage: "+attackDamage);
        }
    }

    public void EndAttackAnimation()
    {
        anim.SetBool("Attack", false);
    }

    private void Flip()
    {
        facingDirection *= -1;
        transform.Rotate(.0f, 180.0f, .0f);
    }

    private void ResetParameters()
    {
        facingDirection = 1;
        transform.position = new Vector3(-21.5f, -2f, 0f);
        transform.rotation = Quaternion.identity;
        isPlayerDetected = false;
        isFollowing = false;
        isAttackingPlayer = false;
        isUsingSkill = false;
        targetToFollow.gameObject.SetActive(true);
        targetToFollow.transform.GetComponentInChildren<Stats>().IncreaseHealth(100);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.transform.position, attackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackRange, transform.position.y, transform.position.z));
    }
}
