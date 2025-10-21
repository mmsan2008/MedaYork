using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [Header("Parámetros del Dash")]
    public float dashForce = 15f;         // fuerza del impulso
    public float dashDuration = 0.2f;     // cuánto dura el dash
    public float dashCooldown = 1f;       // tiempo antes de poder volver a hacer dash

    private bool canDash = true;
    private bool isDashing = false;

    private Rigidbody2D rb2d;
    private playerMovement playerMovement;
    private Animator animator;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<playerMovement>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<playerMovement>();

        if (playerMovement != null && playerMovement.controles != null)
            playerMovement.controles.Movimento.Dash.performed += _ => TryDash();
    }

    private void OnDisable()
    {
        if (playerMovement != null && playerMovement.controles != null)
            playerMovement.controles.Movimento.Dash.performed -= _ => TryDash();
    }


    

    private void TryDash()
    {
        if (canDash && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    private System.Collections.IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Opcional: animación o efecto visual
        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }

        // Desactivar temporalmente la gravedad y aplicar impulso
        float originalGravity = rb2d.gravityScale;
        rb2d.gravityScale = 0;

        float dashDirection = playerMovement.mirandoderecha ? 1f : -1f;
        rb2d.velocity = new Vector2(dashDirection * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb2d.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public bool EstaDashing()
    {
        return isDashing;
    }
}
