using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;

    [Header("Daño")]
    public int damage = 1;

    private void Update()
    {
        // Mover de derecha a izquierda
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        // Destruir si sale de la pantalla (izquierda)
        if (transform.position.x < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x - 2f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("¡Pájaro golpeó al jugador!");

            // Buscar componente de salud del jugador
            HealthSystem playerHealth = collision.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("¡Pájaro golpeó al jugador! (Collision)");

            // Buscar componente de salud del jugador
            HealthSystem playerHealth = collision.gameObject.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}