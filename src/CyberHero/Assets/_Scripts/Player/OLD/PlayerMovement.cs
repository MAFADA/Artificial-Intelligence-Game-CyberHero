using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Animator animator;

    [SerializeField] private Image attackBuff;

    [SerializeField] private PlayerCombat playerCombat;

    private float horizontal;
    private float speed = 8f;
    private float groundCheckRadius = 0.8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;

    public bool IsFacingRight { get => isFacingRight; }

    private void Update()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        if (!isFacingRight && horizontal > 0f)
        {
            Flip();
        }
        else if (isFacingRight && horizontal < 0f)
        {
            Flip();
        }

        if(horizontal == 0f)
        {
            animator.SetBool("isRunning", false);
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        
        transform.localScale = localScale;
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
        animator.SetBool("isRunning", true);
    }
    public void SizeUp(int duration)
    {
        if (IsFacingRight)
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        else
            transform.localScale = new Vector3(-1.5f, 1.5f, 1f);

        attackBuff.gameObject.SetActive(true);

        playerCombat.attackDamage += 20;

        StartCoroutine(WaitForFunction(duration));
    }

    IEnumerator WaitForFunction(int duration)
    {
        yield return new WaitForSeconds(duration);
        if (IsFacingRight)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else
            transform.localScale = new Vector3(-1f, 1f, 1f);

        attackBuff.gameObject.SetActive(false);

        playerCombat.attackDamage -= 20;
    }
}