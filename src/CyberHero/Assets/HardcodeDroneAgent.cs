using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HardcodeDroneAgent : MonoBehaviour
{
    private List<float> rewardLog = new List<float>();
    private string rewardLogFilePath = "Assets/Logs/Hardcode Logs/RewardLog.txt";

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

    #region Enemy Parameters
    public Transform player;
    public LayerMask playerLayer;
    public float moveSpeed = 2.0f;
    public float detectionRange = 3f;
    public float laserRange = 5.0f;
    public GameObject laserPrefab;
    public Transform firepoint;
    private bool canShoot = true;
    private float shootCooldown = 2.0f;

    private bool isGrounded = true;
    private bool isWallDetected;
    private bool facingRight = true;
    private bool isPlayerDetected;
    #endregion


    void Start()
    {
        Core = GetComponentInChildren<Core>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Flip();
    }

    void Update()
    {
        Core.LogicUpdate();

        isGrounded = CollisionSenses.Ground;
        isWallDetected = CollisionSenses.WallFront;

        // Simple behavior: Move towards the player and shoot if within range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (Physics2D.Raycast(firepoint.position, Vector2.right * facingDirection, detectionRange, playerLayer) && canShoot)
        {
            isPlayerDetected = true;
            ShootLaser();
            LogReward(1.0f);
        }
        else
        {
            isPlayerDetected = false;
            if (player.position.x < transform.position.x)
            {
                MoveLeft();
                LogReward(-0.01f);
            }
            else
            {
                MoveRight();
                LogReward(-0.01f);
            }
        }

        CheckForFlip();
      
    }

    private void MoveLeft()
    {
        if (facingRight)
        {
            Flip();
        }
        workspace.Set(moveSpeed * facingDirection, rb.velocity.y);
        rb.velocity = workspace;
   
    }

    private void MoveRight()
    {
        if (!facingRight)
        {
            Flip();
        }
        workspace.Set(moveSpeed * facingDirection, rb.velocity.y);
        rb.velocity = workspace;
       
    }

    private void ShootLaser()
    {
        if (!canShoot) return;


        Instantiate(laserPrefab, firepoint.position, Quaternion.identity);
        canShoot = false;
        Invoke(nameof(ResetShoot), shootCooldown);
       
    }

    private void ResetShoot()
    {
        canShoot = true;
    }

    private void CheckForFlip()
    {
        isGrounded = CollisionSenses.Ground;
        isWallDetected = CollisionSenses.WallFront;
        if (!isGrounded || isWallDetected)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingDirection *= -1;
        facingRight = !facingRight;
        transform.Rotate(.0f, 180.0f, .0f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + detectionRange, transform.position.y, transform.position.z));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + laserRange, transform.position.y, transform.position.z));
    }

    private void LogReward(float reward)
    {
        rewardLog.Add(reward);
        using (StreamWriter writer = new StreamWriter(rewardLogFilePath, true))
        {
            writer.WriteLine(reward);
        }
        rewardLog.Clear();
    }
}
