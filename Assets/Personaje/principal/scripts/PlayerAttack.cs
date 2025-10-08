using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private GameObject attackHitbox; // El objeto hijo con el trigger
    [SerializeField] private float attackDuration = 0.2f; // Cuánto tiempo está activo el hitbox

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackAnimParam = "Attack";

    [Header("Input")]
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0; // Clic izquierdo

    private bool canAttack = true;
    private PlayerCombat playerCombat;

    void Start()
    {
        playerCombat = GetComponent<PlayerCombat>();

        // Asegurar que el hitbox esté desactivado al inicio
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    void Update()
    {
        // Detectar input de ataque
        if (Input.GetKeyDown(attackKey) && canAttack)
        {
            // Verificar que no esté bloqueando
            if (playerCombat == null || playerCombat.CanAttack())
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        canAttack = false;

        Debug.Log("¡Atacando!");

        // Activar hitbox
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);

            // Desactivar después de un tiempo
            Invoke(nameof(DeactivateHitbox), attackDuration);
        }

        // Cooldown
        Invoke(nameof(ResetAttack), attackCooldown);


        if (animator != null)
        {
            animator.SetTrigger(attackAnimParam);
        }
    }

    void DeactivateHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    void ResetAttack()
    {
        canAttack = true;
    }

    // Método público para obtener el daño (usado por AttackHitbox)
    public float GetAttackDamage()
    {
        return attackDamage;
    }


    

    
}