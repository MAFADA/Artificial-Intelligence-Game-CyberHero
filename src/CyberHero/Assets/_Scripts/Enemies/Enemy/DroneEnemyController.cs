using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneEnemyController : MonoBehaviour,IDamageable
{
    private enum State
    {
        Movement,
        Knockback,
        Dead
    }

    private State currentState;

    [SerializeField]
    private float
        groundCheckDistance,
        wallCheckDistance,
        movementSpeed,
        maxHealth,
        knockbackDuration;

    [SerializeField]
    private Transform groundCheck, wallCheck;

    [SerializeField]
    private LayerMask whatIsGround;

    [SerializeField]
    private Vector2 knockbackSpeed;

    [SerializeField]
    private GameObject
        hitParticle,
        deathChunkParticle;

    private float
        currentHealth,
        knockBackStartTime;

    private int
        facingDirecton,
        damageDirection;
    private Vector2 movement;

    private bool groundDetected, wallDetected;

    private GameObject alive;
    private Rigidbody2D RB;
    private Animator animator;

    private void Start()
    {
        alive = transform.Find("Alive").gameObject;
        RB = alive.GetComponent<Rigidbody2D>();
        animator = alive.GetComponent<Animator>();

        currentHealth = maxHealth;
        facingDirecton = 1;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Movement:
                UpdateMovementState();
                break;

            case State.Knockback:
                UpdateKnockbackState();
                break;

            case State.Dead:
                UpdateDeadState();
                break;

        }
    }


    #region Movement State

    private void EnterMovementState()
    {

    }

    private void UpdateMovementState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if (!groundDetected || wallDetected)
        {
            Flip();

        }
        else
        {
            movement.Set(movementSpeed * facingDirecton, RB.velocity.y);
            RB.velocity = movement;
        }
    }

    private void ExitMovementState()
    {

    }

    #endregion

    #region Knockback State

    private void EnterKncokbackState()
    {
        knockBackStartTime = Time.time;
        movement.Set(knockbackSpeed.x * damageDirection, knockbackSpeed.y);
        RB.velocity = movement;
        animator.SetBool("knockback", true);
    }

    private void UpdateKnockbackState()
    {
        if (Time.time >= knockBackStartTime * knockbackDuration)
        {
            SwitchState(State.Movement);
        }
    }

    private void ExitKnockbackState()
    {
        animator.SetBool("knockback", false);

    }

    #endregion

    #region Dead State

    private void EnterDeadState()
    {
        Instantiate(deathChunkParticle, alive.transform.position, deathChunkParticle.transform.rotation);

        Destroy(gameObject);
    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    #endregion


    #region OtherFunctions

    private void Damage(float[] attackDetails)
    {
        currentHealth -= attackDetails[0];

        Instantiate(hitParticle, alive.transform.position, Quaternion.Euler(.0f, .0f, Random.Range(.0f, 360.0f)));

        if (attackDetails[1] > alive.transform.position.x)
        {
            damageDirection = -1;
        }
        else
        {
            damageDirection = 1;
        }

        if (currentHealth > .0f)
        {
            SwitchState(State.Knockback);
        }
        else if (currentHealth <= .0f)
        {
            SwitchState(State.Dead);
        }
    }

    private void Flip()
    {
        facingDirecton *= -1;

        alive.transform.Rotate(.0f, 180.0f, .0f);
    }

    private void SwitchState(State state)
    {
        switch (currentState)
        {
            case State.Movement:
                ExitMovementState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;
        }

        switch (state)
        {
            case State.Movement:
                EnterMovementState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;
        }

        currentState = state;

    }

    public void Damage(float amount)
    {
        throw new System.NotImplementedException();
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));
    }

   
}
