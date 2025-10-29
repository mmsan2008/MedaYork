using UnityEngine;
using UnityEngine.UI;

public class BossNea : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Slider healthBar;
    public GameObject smokePrefab;
    public GameObject knifePrefab;
    public HealthSystem healthSystem; // Tu sistema de vida

    [Header("Stats de Movimiento")]
    public float moveSpeed = 2f;
    public float chargeSpeed = 8f;

    [Header("Configuración")]
    public float minX = -8f;
    public float maxX = 8f;
    public float attackCooldown = 3f;
    public float damage = 10;

    private BossState currentState = BossState.Moving;
    private float direction = -1f;
    private float attackTimer = 0f;
    private string currentAttack = "";
    private float attackPhaseTimer = 0f;
    private bool isFacingRight = false;

    private enum BossState { Moving, Attacking, Charging }

    // ------------------------------------------------------------

    void Start()
    {
        // Obtener el HealthSystem automáticamente si no está asignado
        if (healthSystem == null)
            healthSystem = GetComponent<HealthSystem>();

        // Suscribirse a eventos del sistema de vida
        if (healthSystem != null)
        {
            healthSystem.onHealthChanged.AddListener(UpdateHealthBar);
            healthSystem.onDeath.AddListener(OnBossDeath);
            UpdateHealthBar(healthSystem.CurrentHealth);
        }
    }

    // ------------------------------------------------------------

    void Update()
    {
        // Si está muerto, no hacer nada
        if (healthSystem != null && healthSystem.IsDead)
            return;

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

    // ------------------------------------------------------------
    // MOVIMIENTO
    // ------------------------------------------------------------

    void HandleMovement()
    {
        // Mover de izquierda a derecha
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        // Cambiar dirección en los límites
        if (transform.position.x <= minX || transform.position.x >= maxX)
            direction *= -1f;

        // Decidir si atacar
        if (attackTimer <= 0)
            StartAttack();
    }

    // ------------------------------------------------------------
    // ATAQUES
    // ------------------------------------------------------------

    void StartAttack()
    {
        currentState = BossState.Attacking;
        attackPhaseTimer = 0f;

        int attackIndex = Random.Range(0, 3);
        switch (attackIndex)
        {
            case 0: currentAttack = "smoke"; break;
            case 1: currentAttack = "knives"; break;
            case 2: currentAttack = "charge"; break;
        }

        // Mirar hacia el jugador
        if (player != null)
            direction = player.position.x > transform.position.x ? 1f : -1f;

        Debug.Log("Boss usa ataque: " + currentAttack);
    }

    void HandleAttack()
    {
        attackPhaseTimer += Time.deltaTime;

        switch (currentAttack)
        {
            case "smoke": ExecuteSmokeAttack(); break;
            case "knives": ExecuteKnivesAttack(); break;
            case "charge": ExecuteChargeAttack(); break;
        }
    }

    // ---------------------
    // ATAQUE DE HUMO
    // ---------------------
    void ExecuteSmokeAttack()
    {
        if (attackPhaseTimer >= 0.5f && attackPhaseTimer < 0.6f)
            SpawnSmoke(true); // Humo arriba
        if (attackPhaseTimer >= 1.5f && attackPhaseTimer < 1.6f)
            SpawnSmoke(false); // Humo abajo

        if (attackPhaseTimer >= 3f)
            EndAttack();
    }

    void SpawnSmoke(bool isTop)
    {
        if (smokePrefab == null) return;

        float yPos = isTop ? 3f : -3f;
        GameObject smoke = Instantiate(smokePrefab, new Vector3(minX - 1f, yPos, 0), Quaternion.identity);

        SmokeProjectile smokeScript = smoke.GetComponent<SmokeProjectile>();
        if (smokeScript != null)
            smokeScript.Initialize(isTop);
    }

    // ---------------------
    // ATAQUE DE CUCHILLOS
    // ---------------------
    void ExecuteKnivesAttack()
    {
        if (attackPhaseTimer >= 0.5f && attackPhaseTimer < 0.6f)
            SpawnKnives();

        if (attackPhaseTimer >= 2.5f)
            EndAttack();
    }

    void SpawnKnives()
    {
        if (knifePrefab == null) return;

        int gap1 = Random.Range(1, 4);
        int gap2 = Random.Range(5, 8);

        for (int i = 0; i < 8; i++)
        {
            if (i == gap1 || i == gap2) continue;

            float xPos = Mathf.Lerp(minX, maxX, i / 7f);
            Instantiate(knifePrefab, new Vector3(xPos, 6f, 0), Quaternion.identity);
        }
    }

    // ---------------------
    // ATAQUE DE CARGA
    // ---------------------
    void ExecuteChargeAttack()
    {
        if (attackPhaseTimer >= 0.5f && attackPhaseTimer < 0.6f)
        {
            currentState = BossState.Charging;
            attackPhaseTimer = 0f;
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }


        }
    }

    void HandleCharge()
    {
        attackPhaseTimer += Time.deltaTime;

        transform.position += Vector3.right * direction * chargeSpeed * Time.deltaTime;

        if (transform.position.x <= minX || transform.position.x >= maxX)
            direction *= -1f;

        // Daño al jugador si está cerca
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance < 1f)
            {
                HealthSystem playerHealth = player.GetComponent<HealthSystem>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(25, transform);
            }
        }

        if (attackPhaseTimer >= 3f)
            EndAttack();
    }

    // ------------------------------------------------------------
    // UTILIDADES
    // ------------------------------------------------------------

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

    // ------------------------------------------------------------
    // VIDA
    // ------------------------------------------------------------

    void UpdateHealthBar(float currentHealth)
    {
        if (healthBar == null || healthSystem == null) return;

        healthBar.value = healthSystem.HealthPercentage;

        Image fillImage = healthBar.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            if (healthSystem.HealthPercentage > 0.5f)
                fillImage.color = new Color(1f, 0.3f, 0.3f); // Rojo claro
            else if (healthSystem.HealthPercentage > 0.25f)
                fillImage.color = new Color(1f, 0.5f, 0f);   // Naranja
            else
                fillImage.color = Color.red;                // Rojo oscuro
        }
    }

    void OnBossDeath()
    {
        Debug.Log("¡Boss Nea ha sido derrotado!");
        // Aquí puedes agregar lógica adicional: victoria, recompensas, etc.
    }

    // ------------------------------------------------------------
    // DEBUG
    // ------------------------------------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(minX, transform.position.y, 0), new Vector3(maxX, transform.position.y, 0));
    }

    void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.onHealthChanged.RemoveListener(UpdateHealthBar);
            healthSystem.onDeath.RemoveListener(OnBossDeath);
        }
    }
}
