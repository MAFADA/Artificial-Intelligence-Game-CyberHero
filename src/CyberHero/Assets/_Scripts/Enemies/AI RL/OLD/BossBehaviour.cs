using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BossBehaviour : Agent
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
    private Stats agentStatsCore;
    #endregion

    #region Agent Parameter

    private Vector2 workspace;
    private int facingDirection;
    public float moveSpeed = 2f;

    public float detectionRange = 3f;
    [SerializeField] private LayerMask whatIsTargetDetection;
    [SerializeField] private Transform targetToFollow;


    [Header("Attack parameters")]

    public Transform attackDetector;
    [SerializeField] private LayerMask whatIsTargetAttack;
    public Transform firePoint;

    [Header("Projectile Attack")]
    public float attackRange;
    public GameObject projectilePrefab;

    [Header("Melee Attack")]
    public float meleeAttackRange = 1.5f;
    public float meleeAttackCooldown = 2.0f;
    private bool canMeleeAttack = true;

    [Header("Skill")]
    public float barrierDuration = 5.0f;
    /*public float barrierHealth = 50.0f;*/
    private bool canUseSkill;
    public float skillCooldown = 10f;

    #endregion

    #region Agent Check Variables
    private bool isGrounded;
    private bool isWallDetected;
    private bool isPlayerDetected;
    private bool isFollowing;
    private bool isAttackingPlayer;
    #endregion

    private Transform currentTransform;

    #region Unity Callbacks

    private void Awake()
    {
        Core = GetComponentInChildren<Core>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        agentStatsCore = GetComponentInChildren<Stats>();
        currentTransform = transform;

    }

    private void Start()
    {

        /*  FillListSkill();
          DisableLaserSkill();*/
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
        sensor.AddObservation(agentStatsCore.CurrentHealth);
        sensor.AddObservation(canUseSkill); // Observe if the skill can be used
        sensor.AddObservation(canMeleeAttack); // Observe if melee attack can be used
        sensor.AddObservation(agentStatsCore.BarrierActive); // Observe if barrier is active
        sensor.AddObservation(agentStatsCore.CurrentBarrierHealth);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int movementAction = actions.DiscreteActions[0];
        int attackAction = actions.DiscreteActions[1];
        int skillAction = actions.DiscreteActions[2];

        #region Movement Action
        isGrounded = CollisionSenses.Ground;
        isWallDetected = CollisionSenses.WallFront;

        if (!isGrounded || isWallDetected)
        {
            anim.SetBool("Movement", false);
            Flip();
        }
        else if (movementAction == 1 && !agentStatsCore.BarrierActive)
        {
            anim.SetBool("Movement", true);
            workspace.Set(movementAction * moveSpeed * facingDirection, rb.velocity.y);
            rb.velocity = workspace;
            AddReward(0.01f);
        }
        else
        {
            AddReward(-0.01f); // Penalty for not moving
        }
        #endregion

        #region Detection & Following
        Collider2D agentCollider = Physics2D.OverlapCircle(transform.position, detectionRange, whatIsTargetDetection);
        if (agentCollider != null)
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
        #endregion

        #region AttackActionRegion

        if (Physics2D.Raycast(attackDetector.position, Vector2.right * facingDirection, attackRange, whatIsTargetAttack))
        {
            HandleAttack(attackAction);
            AddReward(0.3f);
        }
        else
        {
            /*DisableLaserSkill();*/
            /*   anim.SetBool("isAttacking", false);*/
            isAttackingPlayer = false;
        }
        #endregion

        #region Skill

        #endregion

        if (targetToFollow.GetComponentInChildren<Combat>().IsDead())
        {
            SetReward(2f);
            EndEpisode();
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteAction = actionsOut.DiscreteActions;

        if (Input.GetKeyDown(KeyCode.V))
        {
            discreteAction[1] = 2;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            discreteAction[1] = 1;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            discreteAction[2] = 1;
        }
    }

    #endregion

    #region TEMP
    /* IEnumerator SkillLaserCoroutine()
     {
         canUseSkill = false;
         SkillLaser();
         yield return new WaitForSeconds(2f);
         DisableLaserSkill();
         yield return new WaitForSeconds(skillCooldown);
         canUseSkill = true;
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

         Vector2 direction = Vector2.right * facingDirection;
         Vector2 origin = firePointSkill.position;

         lineRendererSkill.SetPosition(0, firePointSkill.position);
         startVFXSkill.transform.position = (Vector2)firePointSkill.position;

         RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Infinity, whatIsTargetAttack);

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

             lineRendererSkill.SetPosition(1, origin + direction * 100);
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
     }*/
    #endregion

    private void HandleAttack(int attackAction)
    {
        switch (attackAction)
        {
            case 1:
                if (canMeleeAttack)
                {
                    MeleeAttack();
                }
                break;
            case 2: 
                ShootProjectile();
                break;
            default:
                break;
        }
    }

    private void HandleSkill(int skillAction)
    {
        
        if (skillAction == 1 &&  agentStatsCore.GetHealth() < 50.0f && canUseSkill)
        {
            UseBarrierSkill();
        }
    }

    private void MeleeAttack()
    {
        if (Vector2.Distance(transform.position, targetToFollow.position) <= meleeAttackRange)
        {
            anim.SetTrigger("Melee");

            canMeleeAttack = false;
            StartCoroutine(MeleeAttackCooldownCoroutine());

            Debug.Log("Melee attack executed");
        }
    }

    private void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Debug.Log("Projectile attack executed");
    }

    private void UseBarrierSkill()
    {
        Debug.Log("Barrier skill used");
        anim.SetTrigger("Barrier");
        agentStatsCore.BarrierActive= true;
        agentStatsCore.CurrentBarrierHealth = 50.0f; 
    
        canUseSkill = false;
        StartCoroutine(BarrierCooldownCoroutine());
    }

    private IEnumerator BarrierCooldownCoroutine()
    {
        yield return new WaitForSeconds(barrierDuration);
        agentStatsCore.BarrierActive = false;
  

        yield return new WaitForSeconds(skillCooldown);
        canUseSkill = true;
    }

    private IEnumerator MeleeAttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(meleeAttackCooldown);
        canMeleeAttack = true;
    }

    private void Flip()
    {
        facingDirection *= -1;
        transform.Rotate(.0f, 180.0f, .0f);
    }

    private void ResetParameters()
    {
        facingDirection = 1;
        transform.position = currentTransform.position;
        transform.rotation = Quaternion.identity;
        isPlayerDetected = false;
        isFollowing = false;
        isAttackingPlayer = false;
        canUseSkill = true;
        canMeleeAttack = true;
        agentStatsCore.BarrierActive = false;
        agentStatsCore.CurrentBarrierHealth = 0;
        targetToFollow.gameObject.SetActive(true);
        targetToFollow.transform.GetComponentInChildren<Stats>().IncreaseHealth(100);
        rb.velocity = Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
      /*  Gizmos.color = Color.blue;*/
        /*  Gizmos.DrawWireSphere(attackPoint.transform.position, attackRange);*/
        Gizmos.color = Color.red;
        Gizmos.DrawLine(attackDetector.position, new Vector3(attackDetector.position.x + attackRange, attackDetector.position.y, attackDetector.position.z));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(attackDetector.position, new Vector3(attackDetector.position.x + meleeAttackRange, attackDetector.position.y, attackDetector.position.z));
    }
}
