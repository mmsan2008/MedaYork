using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossNea : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Slider healthBar;
    public GameObject smokePrefab;
    public GameObject knifePrefab;
    public Transform attackPoint;

    [Header("Stats")]
    public float maxHealth = 1000f;
    public float currentHealth;
    public float moveSpeed = 2f;
    public float chargeSpeed = 8f;

    [Header("Configuración")]
    public float minX = -8f;
    public float maxX = 8f;
    public float attackCooldown = 3f;

    private BossState currentState = BossState.Moving;
    private float direction = -1f;
    private float attackTimer = 0f;
    private string currentAttack = "";
    private float attackPhaseTimer = 0f;
    private bool isFacingRight = false;

    private enum BossState
    {
        Moving,
        Attacking,
        Charging
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case BossState.Moving:
                HandleMovement();
                break;
            case BossState.Attacking:
                HandleAttack();
                break;
            case BossState.Charging:
                HandleCharge();
                break;
        }

        FlipSprite();
    }

    void HandleMovement()
    {
        // Mover de izquierda a derecha
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        // Cambiar dirección en los límites
        if (transform.position.x <= minX || transform.position.x >= maxX)
        {
            direction *= -1f;
        }

        // Decidir si atacar
        if (attackTimer <= 0)
        {
            StartAttack();
        }
    }

    void StartAttack()
    {
        currentState = BossState.Attacking;
        attackPhaseTimer = 0f;

        // Elegir ataque aleatorio
        int attackIndex = Random.Range(0, 3);
        switch (attackIndex)
        {
            case 0:
                currentAttack = "smoke";
                break;
            case 1:
                currentAttack = "knives";
                break;
            case 2:
                currentAttack = "charge";
                break;
        }

        // Mirar hacia el jugador
        if (player != null)
        {
            direction = player.position.x > transform.position.x ? 1f : -1f;
        }

        Debug.Log("Boss usa ataque: " + currentAttack);
    }

    void HandleAttack()
    {
        attackPhaseTimer += Time.deltaTime;

        switch (currentAttack)
        {
            case "smoke":
                ExecuteSmokeAttack();
                break;
            case "knives":
                ExecuteKnivesAttack();
                break;
            case "charge":
                ExecuteChargeAttack();
                break;
        }
    }

    void ExecuteSmokeAttack()
    {
        // Humo arriba en 0.5 segundos
        if (attackPhaseTimer >= 0.5f && attackPhaseTimer < 0.6f)
        {
            SpawnSmoke(true); // true = arriba
        }

        // Humo abajo en 1.5 segundos
        if (attackPhaseTimer >= 1.5f && attackPhaseTimer < 1.6f)
        {
            SpawnSmoke(false); // false = abajo
        }

        // Terminar ataque
        if (attackPhaseTimer >= 3f)
        {
            EndAttack();
        }
    }

    void SpawnSmoke(bool isTop)
    {
        if (smokePrefab != null)
        {
            float yPos = isTop ? 3f : -3f;
            GameObject smoke = Instantiate(smokePrefab, new Vector3(minX - 1f, yPos, 0), Quaternion.identity);
            smoke.GetComponent<SmokeProjectile>().Initialize(isTop);
        }
    }

    void ExecuteKnivesAttack()
    {
        // Lanzar cuchillos en 0.5 segundos
        if (attackPhaseTimer >= 0.5f && attackPhaseTimer < 0.6f)
        {
            SpawnKnives();
        }

        // Terminar ataque
        if (attackPhaseTimer >= 2.5f)
        {
            EndAttack();
        }
    }

    void SpawnKnives()
    {
        if (knifePrefab != null)
        {
            // Crear patrón de cuchillos con huecos aleatorios
            int gap1 = Random.Range(1, 4);
            int gap2 = Random.Range(5, 8);

            for (int i = 0; i < 8; i++)
            {
                if (i != gap1 && i != gap2)
                {
                    float xPos = minX + (maxX - minX) * (i / 7f);
                    Instantiate(knifePrefab, new Vector3(xPos, 6f, 0), Quaternion.identity);
                }
            }
        }
    }

    void ExecuteChargeAttack()
    {
        // Cambiar a estado de carga inmediatamente
        if (attackPhaseTimer >= 0.5f && attackPhaseTimer < 0.6f)
        {
            currentState = BossState.Charging;
            attackPhaseTimer = 0f;
        }
    }

    void HandleCharge()
    {
        attackPhaseTimer += Time.deltaTime;

        // Moverse rápido
        transform.position += Vector3.right * direction * chargeSpeed * Time.deltaTime;

        // Rebotar en los bordes
        if (transform.position.x <= minX || transform.position.x >= maxX)
        {
            direction *= -1f;
        }

        // Verificar colisión con jugador
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance < 1f)
            {
                HealthSystem playerHealth = player.GetComponent<HealthSystem>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(25);
                }
            }
        }

        // Terminar carga después de 3 segundos
        if (attackPhaseTimer >= 3f)
        {
            EndAttack();
        }
    }

    void EndAttack()
    {
        currentState = BossState.Moving;
        attackTimer = attackCooldown;
        currentAttack = "";
    }

    void FlipSprite()
    {
        if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Debug.Log("¡Boss derrotado!");
        // Aquí puedes agregar animación de muerte, drop de items, etc.
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(minX, transform.position.y, 0),
                       new Vector3(maxX, transform.position.y, 0));
    }
}