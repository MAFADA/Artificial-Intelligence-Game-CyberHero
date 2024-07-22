using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class GroundDroneHardcodeTest : MonoBehaviour
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
    float distanceToPlayer;
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

    float rewardAction = 0f;

    private List<string> logEntries = new List<string>();
    private string logFilePath = "Assets/Logs/Hardcode Logs/GroundDroneTestLogs.csv";
    

    private void Awake()
    {
        Core = GetComponentInChildren<Core>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        randomSpot = Random.Range(0, patrolSpots.Length);
    }

    private void Start()
    {
        if (!File.Exists(logFilePath))
        {
            using (StreamWriter writer = new StreamWriter(logFilePath))
            {
                writer.WriteLine("Distance,Action,Reward");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Core.LogicUpdate();

        isGrounded = CollisionSenses.Ground;
        isWallDetected = CollisionSenses.WallFront;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        #region Patrol
        if (!isWaiting && !isPlayerDetected)
        {
            rewardAction = Patrol();
            LogEntry(distanceToPlayer, "Patrol", rewardAction);

        }
        else
        {
            rewardAction = 0;
            LogEntry(distanceToPlayer, "Patrol", rewardAction);
        }
        #endregion

        #region Follow
        Collider2D detection = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (detection != null && detection.CompareTag("Player"))
        {
           rewardAction = Follow();
            LogEntry(distanceToPlayer, "Follow", rewardAction);

        }
        else
        {
            isPlayerDetected = false;
            LogEntry(distanceToPlayer, "Follow", -.02f);
        }

        #endregion

        #region ShootLaser
        if (!canShoot) return;

        Collider2D laserDetection = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (laserDetection != null && laserDetection.CompareTag("Player"))
        {
            rewardAction=  ShootLaser();
            LogEntry(distanceToPlayer, "ShootLaser", rewardAction);
        }
        else
        {
            isPlayerDetected = false;
            anim.SetBool("Attack", false);

            LogEntry(distanceToPlayer, "ShootLaser", -0.05f);
        }
        #endregion


    }

    float ShootLaser()
    {

        anim.SetBool("Attack", true);

        isPlayerDetected = true;
        Instantiate(laserPrefab, firepoint.position, firepoint.rotation);
        canShoot = false;
        Invoke(nameof(ResetShoot), shootCooldown);

        return 2.0f;

    }

    float Follow()
    {
        isPlayerDetected = true;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        if (distanceToPlayer > stopDistance)
        {
            anim.SetBool("Movement", true);

            rb.velocity = directionToPlayer * moveSpeed;
            FlipDirection(directionToPlayer);
            return .1f;
        }
        else if (distanceToPlayer < stopDistance && distanceToPlayer > fleeDistance)
        {
            anim.SetBool("Movement", false);
            anim.SetBool("Idle", true);


            rb.velocity = Vector2.zero;
            return 0.05f;

        }
        else if (distanceToPlayer < fleeDistance)
        {
            anim.SetBool("Movement", true);

            rb.velocity = -directionToPlayer * moveSpeed;
            FlipDirection(-directionToPlayer);
            return -0.1f;
        }

        return 0f;
    }

    float Patrol()
    {
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
        float reward = distanceToSpot < .2f ? 0.1f : -0.1f;
        return reward;
    }

    private IEnumerator WaitAtSpot()
    {
        isWaiting = true;

        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(startWaitTime);

        randomSpot = Random.Range(0, patrolSpots.Length);
        isWaiting = false;
    }

    private void FlipDirection(Vector2 direction)
    {
        if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
        {
            facingRight = !facingRight;
            transform.Rotate(.0f, 180.0f, .0f);
        }
    }

    private void ResetShoot()
    {
        canShoot = true;
    }

    private void LogEntry(float distance, string action, float reward)
    {
        logEntries.Add($"{distance},{action},{reward}");
        SaveLogs();
    }

    private void SaveLogs()
    {
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            foreach (var entry in logEntries)
            {
                writer.WriteLine(entry);
            }
            logEntries.Clear();
        }
    }

}
