using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float damage;
    private float explosionRadius;
    private LayerMask enemyLayer;
    private GameObject shooter;

    [Header("Projectile Settings")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float gravity = 1f; // Gravedad del proyectil
    [SerializeField] private bool explodeOnImpact = true;
    [SerializeField] private bool explodeOnLifetime = true;

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionEffectDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;

    private Rigidbody2D rb;
    private bool hasExploded = false;
    private float timer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
    }

    public void Initialize(Vector2 dir, float spd, float dmg, float radius, LayerMask layer, GameObject shooterObj)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        explosionRadius = radius;
        enemyLayer = layer;
        shooter = shooterObj;

        // ASEGURAR que tenga Rigidbody2D
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.gravityScale = 0;
        }

        // APLICAR VELOCIDAD INMEDIATAMENTE
        rb.velocity = direction * speed;

        Debug.Log($"Bomba inicializada. Dirección: {direction}, Velocidad: {rb.velocity}");
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Aplicar gravedad simulada
        if (rb != null)
        {
            rb.velocity += Vector2.down * gravity * Time.deltaTime;
        }

        // Explotar por tiempo
        if (explodeOnLifetime && timer >= lifetime && !hasExploded)
        {
            Explode();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignorar al que disparó
        if (collision.gameObject == shooter) return;

        // Explotar al tocar enemigo
        if (collision.CompareTag("Enemy") && explodeOnImpact)
        {
            Explode();
        }
        // Explotar al tocar suelo/paredes
        else if ((collision.CompareTag("Suelo") || collision.CompareTag("Wall")) && explodeOnImpact)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log("¡EXPLOSIÓN!");

        // Detectar enemigos en el radio de explosión
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            HealthSystem enemyHealth = hit.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                // Calcular daño según distancia (opcional)
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                float damageFalloff = 1f - (distance / explosionRadius);
                float finalDamage = damage * Mathf.Clamp01(damageFalloff);

                enemyHealth.TakeDamage(finalDamage, transform.position);
                Debug.Log($"Enemigo recibió {finalDamage} de daño por explosión");
            }
        }

        // Efecto visual de explosión
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, explosionEffectDuration);
        }

        // Sonido de explosión
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Destruir proyectil
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        // Mostrar radio de explosión en editor
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}