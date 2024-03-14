using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class EnemyBehaviour : Agent
{
    // STATE

    private Vector2 playerPosition;
    private Vector2 enemyPosition;
    private float playerHealth;
    private float enemyHealth;

    #region Check Variables
    private bool isGrounded;
    private bool isFacingWall;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    #endregion


    // ACTION
    public enum EnemyAction { MoveLeft, MoveRight, Attack, Skill, FollowPlayer, Jump }
    private Dictionary<EnemyAction, int> actionMapping = new Dictionary<EnemyAction, int>();


    // Reward
    private float reward;

    
    private Rigidbody2D rb;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    #region Unity MlAgents Function
    public override void Initialize()
    {
        base.Initialize();

        actionMapping.Add(EnemyAction.MoveLeft, 0);
        actionMapping.Add(EnemyAction.MoveRight, 1);
        actionMapping.Add(EnemyAction.Attack, 2);
        actionMapping.Add(EnemyAction.Skill, 3);
        actionMapping.Add(EnemyAction.FollowPlayer, 4);
        actionMapping.Add(EnemyAction.Jump, 5);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        sensor.AddObservation(playerPosition);
        sensor.AddObservation(enemyPosition);
        sensor.AddObservation(playerHealth);
        sensor.AddObservation(enemyHealth);
        sensor.AddObservation(isGrounded ? 1 : 0);
        sensor.AddObservation(isFacingWall ? 1 : 0);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);

        int actionIndex = actions.DiscreteActions[0];
        EnemyAction enemyAction = (EnemyAction)actionIndex;

        switch (enemyAction)
        {
            case EnemyAction.MoveLeft:
                Move(-1);
                break;
            case EnemyAction.MoveRight:
                Move(1);
                break;
            case EnemyAction.Attack:
                Attack();
                break;
            case EnemyAction.Skill:
                Skill();
                break;
            case EnemyAction.FollowPlayer:
                FollowPlayer();
                break;
            case EnemyAction.Jump:
                if (isGrounded && isFacingWall)
                {
                    Jump();
                }
                break;
        }

        UpdateState();
        UpdateReward();
        AddReward(reward);
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        base.Heuristic(actionsOut);
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 0; // MoveLeft
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 1; // MoveRight
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 5; // Jump
        }
        else if (Input.GetKey(KeyCode.F))
        {
            discreteActionsOut[0] = 4; // FollowPlayer
        }
    }

    #endregion

    #region QLearning Functions
    private void UpdateState()
    {
        playerPosition = playerTransform.position;
        enemyPosition = transform.position;

        if (Physics2D.OverlapCircle(groundCheck.position,groundCheckRadius,whatIsGround))
        {
            isGrounded = true;   
        }
        else
        {
            isGrounded = false;
        }

        if (Physics2D.Raycast(wallCheck.position,Vector2.right*1,wallCheckDistance,whatIsGround))
        {
            isFacingWall = true;
        }
        else
        {
            isFacingWall= false;
        }


    }

    private void UpdateReward()
    {
        if (playerHealth <= 0)
        {
            reward = 1.0f;
        }
        else
        {
            reward = -0.01f;
        }
    }

    #endregion

    #region Action Functions
    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void FollowPlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Vector2 movement = direction * movementSpeed;
        rb.velocity = movement;
    }

    private void Skill()
    {
        throw new NotImplementedException();
    }

    private void Attack()
    {
        throw new NotImplementedException();
    }

    private void Move(float facingDirection)
    {
        Vector2 movement = new Vector2(facingDirection * movementSpeed * Time.deltaTime, rb.velocity.y);
        rb.velocity = movement;
    }

    #endregion
}
