using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MidBossAgent1 : Agent
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
    Stats statsCore;
    float statusHealth;
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
    private string[] actions = new string[] { "Follow", "Attack", "Skill" };
    #endregion

    #region Enemy Parameter
    [Header("Movement & Detection")]
    public Transform player;
    public LayerMask playerLayer;
    public float moveSpeed = 2.0f;
    public float stopDistance = 20f;
    public float fleeDistance = 10f;
    public float detectionRange = 3f;
    private float distanceToPlayer;



    [Header("Attack parameters")]
    public Transform attackDetector;
    public GameObject laserPrefab;
    public Transform firepoint;
    public float laserRange = 5.0f;
    private bool canShoot = true;
    public float shootCooldown = 2.0f;

    [Header("Skill parameters")]
    public Transform firePointSkill;
    public LineRenderer lineRendererSkill;
    public float skillCooldown = 10f;
    public int skillDamage = 20;
    public float skillDuration = 1f;
    public GameObject startVFXSkill;
    public GameObject endVFXSkill;
    private bool canUseSkill = true;
    private List<ParticleSystem> particlesSkill = new();
    #endregion

    #region Agent Check Variables
    private bool isGrounded = true;
    private bool facingRight = true;
    private bool isWallDetected;
    private bool isPlayerDetected;
    private bool isFollowing;
    private bool isAttackingPlayer;
    #endregion

    private string GetState()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        string state = $"D:{distanceToPlayer:F1}_PD:{isPlayerDetected}_FL:{isFollowing}_G:{isGrounded}_W:{isWallDetected}_F:{facingRight}_AT:{isAttackingPlayer}_CS:{canShoot}_CUS:{canUseSkill}";

        return state;
    }
    #region Unity Callbacks
    private void Awake()
    {
        statsCore = GetComponentInChildren<Stats>();
        Core = GetComponentInChildren<Core>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        FillListSkill();
        DisableLaserSkill();
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
        statusHealth = statsCore.CurrentHealth / statsCore.MaxHealth;
        sensor.AddObservation(distanceToPlayer);
        sensor.AddObservation(isGrounded);
        sensor.AddObservation(isWallDetected);
        sensor.AddObservation(isPlayerDetected);
        sensor.AddObservation(facingRight);
        sensor.AddObservation(isAttackingPlayer);
        sensor.AddObservation(isFollowing);
        sensor.AddObservation(canShoot);
        sensor.AddObservation(canUseSkill);
        sensor.AddObservation(statusHealth);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        currentAction = actions[actionBuffers.DiscreteActions[0]];

        isGrounded = CollisionSenses.Ground;
        isWallDetected = CollisionSenses.WallFront;
        float reward = -0.01f;

        switch (currentAction)
        {
            case "Follow":
                reward = Follow();
                UpdateQValue(reward);
                break;
            case "Attack":
                reward = ShootLaser();
                UpdateQValue(reward);
                break;
            case "Skill":

                if (statsCore.CurrentHealth <= statsCore.MaxHealth * .5f)
                {
                    StartCoroutine(SkillCoroutine(reward =>
                    {
                        UpdateQValue(reward);
                    }));
                }
                else
                {
                    reward = -0.5f;
                    UpdateQValue(reward);
                }
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
    private float Follow()
    {
        isFollowing = true;
        distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        if (distanceToPlayer > stopDistance)
        {
            anim.SetBool("Movement", true);

            rb.velocity = directionToPlayer * moveSpeed;
            FlipDirection(directionToPlayer);
            return 0.1f;
        }
        else if (distanceToPlayer < stopDistance && distanceToPlayer > fleeDistance)
        {
            anim.SetBool("Movement", false);
            anim.SetBool("Idle", true);


            rb.velocity = Vector2.zero;
            return 0.05f;
            /*float shootReward = ShootLaser();
            return 0.05f + shootReward;*/
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

    private float ShootLaser()
    {
        if (!canShoot) return -1.0f;

        Collider2D laserDetection = Physics2D.OverlapCircle(transform.position, laserRange, playerLayer);

        if (laserDetection != null && laserDetection.CompareTag("Player"))
        {
            isAttackingPlayer = true;
            anim.SetBool("Attack", true);

            isPlayerDetected = true;
            Instantiate(laserPrefab, firepoint.position, firepoint.rotation);
            canShoot = false;
            Invoke(nameof(ResetShoot), shootCooldown);
            return 2.0f;
        }
        else
        {
            isAttackingPlayer = false;

            isPlayerDetected = false;
            anim.SetBool("Attack", false);

            return -.05f;
        }
    }

    private void ResetShoot()
    {
        canShoot = true;
    }

    void EnableLaserSkill()
    {
        lineRendererSkill.enabled = true;


        for (int i = 0; i < particlesSkill.Count; i++)
        {
            particlesSkill[i].Play();
        }
    }

    void DisableLaserSkill()
    {
        lineRendererSkill.enabled = false;

        for (int i = 0; i < particlesSkill.Count; i++)
        {
            particlesSkill[i].Stop();
        }
    }

    void SkillLaser()
    {
        EnableLaserSkill();

        Vector2 rayDirection = facingRight ? Vector2.right : Vector2.left;
        Vector2 origin = firePointSkill.position;

        lineRendererSkill.SetPosition(0, firePointSkill.position);
        startVFXSkill.transform.position = (Vector2)firePointSkill.position;

        RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, Mathf.Infinity, playerLayer);

        if (hit)
        {
            lineRendererSkill.SetPosition(1, hit.point);
            endVFXSkill.transform.position = lineRendererSkill.GetPosition(1);

            Combat playerCombatCore = hit.collider.GetComponentInChildren<Combat>();
            if (playerCombatCore != null)
            {
                playerCombatCore.Damage(skillDamage);
                AddReward(0.5f);
            }
        }
        else
        {
            lineRendererSkill.SetPosition(1, origin + rayDirection * 100);
            endVFXSkill.transform.position = lineRendererSkill.GetPosition(1);
        }
    }

    void FillListSkill()
    {
        for (int i = 0; i < startVFXSkill.transform.childCount; i++)
        {
            var ps = startVFXSkill.transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null)
            {
                particlesSkill.Add(ps);
            }
        }

        for (int i = 0; i < endVFXSkill.transform.childCount; i++)
        {
            var ps = endVFXSkill.transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null)
            {
                particlesSkill.Add(ps);
            }
        }
    }

    private IEnumerator SkillCoroutine(System.Action<float> callback)
    {
        Vector2 rayDirection = facingRight ? Vector2.right : Vector2.left;
        if (Physics2D.Raycast(attackDetector.position, rayDirection, laserRange, playerLayer))
        {

            rb.velocity = Vector2.zero;
            isAttackingPlayer = true;
            float reward = 0f;

            yield return StartCoroutine(SkillLaserCoroutine());

            reward = 1.0f;
            callback(reward);
        }
        else
        {

            DisableLaserSkill();
            anim.SetBool("isAttacking", false);
            isAttackingPlayer = false;

            callback(-0.5f);
        }
    }

    private IEnumerator SkillLaserCoroutine()
    {
        canUseSkill = false;
        SkillLaser();
        yield return new WaitForSeconds(skillDuration);
        DisableLaserSkill();
        yield return new WaitForSeconds(skillCooldown);
        canUseSkill = true;
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
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, laserRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(attackDetector.position, new Vector3(attackDetector.position.x + laserRange,attackDetector.position.y, attackDetector.position.z));
    }
    #endregion
}
