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

    #region Components
    private Animator anim;
    private Rigidbody2D rb;
    private Vector2 workspace;
    private int facingDirection = 1;
    #endregion

    #region Q-Learning Parameters
    private Dictionary<string, float> qTable = new Dictionary<string, float>();
    private float learningRate = 0.1f;
    private float discountFactor = 0.99f;
    private float explorationRate = 0.1f;

    private string currentState;
    private string nextState;
    private string currentAction;
    private string[] actions = new string[] { "MoveLeft", "MoveRight", "ShootLaser" };
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

    #region Log Training
    private StreamWriter logWriter;
    private string logFilePath = "Assets/Logs/qLearningLog.txt";
    private string qTableFilePath = "Assets/Logs/qTable.txt";

    private List<string> actionLog = new List<string>();
    private string actionLogFilePath = "Assets/Logs/DroneActionLog.txt";

    private List<float> distanceLog = new List<float>();
    private string distanceLogFilePath = "Assets/Logs/DroneDistanceLog.txt";

    private List<string> qValueLog = new List<string>();
    private string qValueLogFilePath = "Assets/Logs/QValueLog.txt";

    private List<float> rewardLog = new List<float>();
    private string rewardLogFilePath = "Assets/Logs/RewardLog.txt";
    #endregion

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

    #region Unity Callbacks
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
    #endregion

    #region MLAgent Functions
    public override void OnEpisodeBegin()
    {
        // Reset drone position
        transform.position = new Vector3(Random.Range(-5f, 5f), transform.position.y, transform.position.z);

        // Reset Q-Table and state
        qTable.Clear();
        currentState = GetState();

        logWriter.WriteLine("New Episode Started");

        SaveActionLog();
        actionLog.Clear();

        SaveDistanceLog();
        distanceLog.Clear();

        SaveQValueLog();
        qValueLog.Clear();

        SaveRewardLog();
        rewardLog.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        sensor.AddObservation(distanceToPlayer);
        distanceLog.Add(distanceToPlayer);

        sensor.AddObservation(isGrounded);
        sensor.AddObservation(isWallDetected);
        sensor.AddObservation(facingRight);
        sensor.AddObservation(canShoot);


    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        currentAction = actions[actionBuffers.DiscreteActions[0]];
        actionLog.Add(currentAction);


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
       
        string qValueEntry = $"{currentState}_{currentAction}: {newQ}";
        qValueLog.Add(qValueEntry);

        currentState = nextState;

        AddReward(reward);

        rewardLog.Add(GetCumulativeReward());
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
    #endregion

    #region Other Functions
    private float MoveLeft()
    {
        CheckForFlip();
        if (facingRight)
        {
            Flip();
        }
        workspace.Set(moveSpeed * facingDirection, rb.velocity.y);
        rb.velocity = workspace;
      

        // Reward for moving closer to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer < detectionRange ? 0.1f : -0.1f;
    }

    private float MoveRight()
    {
        CheckForFlip();
        if (!facingRight)
        {
            Flip();
        }
        workspace.Set(moveSpeed * facingDirection, rb.velocity.y);
        rb.velocity = workspace;
     

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer < detectionRange ? 0.1f : -0.1f;
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

    #endregion

    #region LogFunctions

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

    private void SaveActionLog()
    {
        using (StreamWriter writer = new StreamWriter(actionLogFilePath, true))
        {
            foreach (var action in actionLog)
            {
                writer.WriteLine(action);
            }
        }
    }

    private void SaveDistanceLog()
    {
        using (StreamWriter writer = new StreamWriter(distanceLogFilePath, true))
        {
            foreach (var distance in distanceLog)
            {
                writer.WriteLine(distance);
            }
        }
    }

    private void SaveQValueLog()
    {
        using (StreamWriter writer = new StreamWriter(qValueLogFilePath, true))
        {
            foreach (var qValue in qValueLog)
            {
                writer.WriteLine(qValue);
            }
        }
    }

    private void SaveRewardLog()
    {
        using (StreamWriter writer = new StreamWriter(rewardLogFilePath, true))
        {
            foreach (var reward in rewardLog)
            {
                writer.WriteLine(reward);
            }
        }
    }
    #endregion
}
