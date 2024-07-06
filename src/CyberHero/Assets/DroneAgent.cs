using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class DroneAgent : Agent
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

    private Animator anim;
    private Rigidbody2D rb;
    private Vector2 workspace;
    private int facingDirection = 1;

    // Q-Learning parameters
    private Dictionary<string, float> qTable = new Dictionary<string, float>();
    private float learningRate = 0.1f;
    private float discountFactor = 0.99f;
    private float explorationRate = 0.1f;

    // Enemy drone parameters
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


    private string currentState;
    private string nextState;
    private string currentAction;

    private StreamWriter logWriter;
    private string logFilePath = "Assets/Logs/qLearningLog.txt";
    private string qTableFilePath = "Assets/Logs/qTable.txt";

    // Actions
    private string[] actions = new string[] { "MoveLeft", "MoveRight", "ShootLaser" };

    // State representation (for simplicity, we use a string)
    private string GetState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        string state = $"D:{distanceToPlayer:F1}_G:{isGrounded}_W:{isWallDetected}_F:{facingRight}";

        return state;
    }

    private void EnsureLogDirectoryExists()
    {
        string logDirectory = Path.GetDirectoryName(logFilePath);
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    private void Awake()
    {
        Core = GetComponentInChildren<Core>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        EnsureLogDirectoryExists();
        logWriter = new StreamWriter(logFilePath, false);
        Flip();
    }

    private void Update()
    {
        Core.LogicUpdate();
    }

    public override void OnEpisodeBegin()
    {
        // Reset drone position
        transform.position = new Vector3(Random.Range(-5f, 5f), transform.position.y, transform.position.z);

        // Reset Q-Table and state
        qTable.Clear();
        currentState = GetState();

        logWriter.WriteLine("New Episode Started");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Vector3.Distance(transform.position, player.position));
        sensor.AddObservation(isGrounded);
        sensor.AddObservation(isWallDetected);
        sensor.AddObservation(facingRight);
        sensor.AddObservation(canShoot);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        currentAction = actions[actionBuffers.DiscreteActions[0]];
        isGrounded = CollisionSenses.Ground;
        isWallDetected = CollisionSenses.WallFront;
        float reward = -0.01f; // Small negative reward to encourage quicker completion

        switch (currentAction)
        {
            case "MoveLeft":
                reward = MoveLeft();
                break;
            case "MoveRight":
                reward = MoveRight();
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

        logWriter.WriteLine($"State: {currentState}, Action: {currentAction}, Reward: {reward}, New Q-value: {newQ}");

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
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActions[0] = 1;
        }

        else if (Input.GetKey(KeyCode.F))
        {
            discreteActions[0] = 2;
        }
    }

    private float MoveLeft()
    {
        if (facingRight)
        {
            Flip();
        }
        workspace.Set(moveSpeed * facingDirection, rb.velocity.y);
        rb.velocity = workspace;
        CheckForFlip();

        // Reward for moving closer to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer < laserRange ? 0.1f : -0.1f;
    }

    private float MoveRight()
    {
        if (!facingRight)
        {
            Flip();
        }
        workspace.Set(moveSpeed * facingDirection, rb.velocity.y);
        rb.velocity = workspace;
        CheckForFlip();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer < laserRange ? 0.1f : -0.1f;
    }

    private float ShootLaser()
    {
        if (!canShoot) return -1.0f;

        if (Physics2D.Raycast(firepoint.position, Vector2.right * facingDirection, detectionRange, playerLayer))
        {
            isPlayerDetected = true;
            Instantiate(laserPrefab, firepoint.position, firepoint.rotation);
            canShoot = false;
            Invoke(nameof(ResetShoot), shootCooldown);
            return 1.0f;
        }
        else
        {
            isPlayerDetected = false;

            return -0.5f;
        }
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
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + detectionRange, transform.position.y, transform.position.z));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + laserRange, transform.position.y, transform.position.z));
    }

    void OnDestroy()
    {
        logWriter.Close();
        SaveQTable();
    }

    private void SaveQTable()
    {
        EnsureLogDirectoryExists();
        using (StreamWriter qTableWriter = new StreamWriter(qTableFilePath, false))
        {
            foreach (var entry in qTable)
            {
                qTableWriter.WriteLine($"{entry.Key}:{entry.Value}");
            }
        }
    }
}
