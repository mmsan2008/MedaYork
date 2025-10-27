using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    public float fallSpeed = 4f;
    public float damage = 20f;
    private bool hasHit = false;

    void Update()
    {
        if (!hasHit)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            // Destruir si sale de la pantalla
            if (transform.position.y < -6f)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasHit)
        {
            hasHit = true;
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Suelo"))
        {
            hasHit = true;
            Destroy(gameObject, 1f);
        }
    }
}