using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private PlayerAttack playerAttack;

    void Start()
    {
        // Obtener referencia al script de ataque del padre (Player)
        playerAttack = GetComponentInParent<PlayerAttack>();

        if (playerAttack == null)
        {
            Debug.LogWarning("AttackHitbox no encontró PlayerAttack en el padre!");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hitbox tocó: " + collision.gameObject.name);

        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("¡Es un enemigo!");

            HealthSystem enemyHealth = collision.GetComponent<HealthSystem>();

            if (enemyHealth != null)
            {
                float damage = playerAttack != null ? playerAttack.GetAttackDamage() : 10f;

                enemyHealth.TakeDamage(damage, transform.position);
                Debug.Log($"¡Daño aplicado: {damage}!");
            }
            else
            {
                Debug.Log("Enemigo NO tiene HealthSystem");
            }
        }
    }
}