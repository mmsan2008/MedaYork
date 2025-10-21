using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isInvulnerable = false;

    [Header("Damage Settings")]
    [SerializeField] private bool useDamageInvincibility = true;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private bool canDieFromDamage = true;

    [Header("Regeneration")]
    [SerializeField] private bool enableHealthRegen = false;
    [SerializeField] private float regenAmount = 5f;
    [SerializeField] private float regenInterval = 1f;
    [SerializeField] private float regenDelay = 3f; // Espera después de recibir daño
    private float timeSinceLastDamage = 0f;
    private Coroutine regenCoroutine;

    [Header("Knockback")]
    [SerializeField] private bool enableKnockback = true;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Visual Feedback")]
    [SerializeField] private bool enableDamageFlash = true;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Death Settings")]
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private bool autoRespawn = false;
    [SerializeField] private Vector3 respawnPosition;

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Events")]
    public UnityEvent<float> onHealthChanged;
    public UnityEvent<float> onDamageTaken;
    public UnityEvent<float> onHealed;
    public UnityEvent onDeath;
    public UnityEvent onRespawn;
    public UnityEvent onInvincibilityStart;
    public UnityEvent onInvincibilityEnd;

    // Estados
    private bool isDead = false;
    private bool isInvincible = false;
    private Rigidbody2D rb;
    private PlayerCombat playerCombat;
    private Color originalColor;

    // Propiedades públicas
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsDead => isDead;
    public bool IsInvincible => isInvincible;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCombat = GetComponent<PlayerCombat>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        respawnPosition = transform.position;
        onHealthChanged?.Invoke(currentHealth);
    }

    void Update()
    {
        // Actualizar regeneración
        if (enableHealthRegen && !isDead)
        {
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= regenDelay && currentHealth < maxHealth)
            {
                if (regenCoroutine == null)
                {
                    regenCoroutine = StartCoroutine(RegenerateHealth());
                }
            }
        }
    }

    #region Damage Methods

    public void TakeDamage(float damage, Vector2 damageSource = default)
    {
        if (isDead || isInvincible || isInvulnerable) return;

        // Aplicar reducción de daño si está bloqueando
        if (playerCombat != null && playerCombat.IsBlocking())
        {
            damage = playerCombat.TakeDamage(damage);
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Resetear timer de regeneración
        timeSinceLastDamage = 0f;
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }

        // Eventos
        onDamageTaken?.Invoke(damage);
        onHealthChanged?.Invoke(currentHealth);

        // Feedback visual
        if (enableDamageFlash && spriteRenderer != null)
        {
            StartCoroutine(DamageFlash());
        }

        // Audio
        PlaySound(damageSound);

        // Knockback
        if (enableKnockback && damageSource != default)
        {
            ApplyKnockback(damageSource);
        }

        // Invencibilidad temporal
        if (useDamageInvincibility)
        {
            StartCoroutine(InvincibilityFrames());
        }

        // Verificar muerte
        if (currentHealth <= 0 && canDieFromDamage)
        {
            Die();
        }

        Debug.Log($"Daño recibido: {damage}. Vida actual: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(float damage, Transform damageSource)
    {
        if (damageSource != null)
        {
            TakeDamage(damage, damageSource.position);
        }
        else
        {
            TakeDamage(damage);
        }
    }

    public void InstantKill()
    {
        currentHealth = 0;
        Die();
    }

    #endregion

    #region Healing Methods

    public void Heal(float amount)
    {
        if (isDead) return;

        float oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        float actualHealing = currentHealth - oldHealth;

        if (actualHealing > 0)
        {
            onHealed?.Invoke(actualHealing);
            onHealthChanged?.Invoke(currentHealth);
            PlaySound(healSound);

            Debug.Log($"Curación: +{actualHealing}. Vida actual: {currentHealth}/{maxHealth}");
        }
    }

    public void HealToFull()
    {
        Heal(maxHealth);
    }

    public void SetHealth(float amount)
    {
        currentHealth = Mathf.Clamp(amount, 0, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }

    IEnumerator RegenerateHealth()
    {
        while (currentHealth < maxHealth && timeSinceLastDamage >= regenDelay)
        {
            yield return new WaitForSeconds(regenInterval);
            Heal(regenAmount);
        }

        regenCoroutine = null;
    }

    #endregion

    #region Death and Respawn

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;

        onDeath?.Invoke();
        PlaySound(deathSound);

        Debug.Log("¡Jugador ha muerto!");

        // Desactivar controles (opcional)
        DisableControls();

        if (autoRespawn)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;

        // Restaurar posición
        transform.position = respawnPosition;

        // Restaurar visual
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            spriteRenderer.enabled = true;
        }

        // Resetear velocidad
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        onRespawn?.Invoke();
        onHealthChanged?.Invoke(currentHealth);
        EnableControls();

        // Invencibilidad temporal al reaparecer
        StartCoroutine(InvincibilityFrames(2f));

        Debug.Log("¡Jugador ha reaparecido!");
    }

    public void SetRespawnPosition(Vector3 position)
    {
        respawnPosition = position;
    }

    #endregion

    #region Visual and Audio Feedback

    IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    IEnumerator InvincibilityFrames(float duration = -1)
    {
        if (duration < 0)
            duration = invincibilityDuration;

        isInvincible = true;
        onInvincibilityStart?.Invoke();

        // Parpadeo visual
        if (spriteRenderer != null)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            spriteRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        isInvincible = false;
        onInvincibilityEnd?.Invoke();
    }

    void ApplyKnockback(Vector2 damageSource)
    {
        if (rb == null) return;

        Vector2 direction = ((Vector2)transform.position - damageSource).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(KnockbackDuration());
    }

    IEnumerator KnockbackDuration()
    {
        // Desactivar controles temporalmente
        yield return new WaitForSeconds(knockbackDuration);
        // Aquí puedes reactivar controles si es necesario
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Utility Methods

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    public void ModifyMaxHealth(float newMaxHealth)
    {
        float healthPercentage = HealthPercentage;
        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = maxHealth * healthPercentage;
        onHealthChanged?.Invoke(currentHealth);
    }

    void DisableControls()
    {
        // Desactivar scripts de control
        var movement = GetComponent<playerMovement>();
        if (movement != null) movement.enabled = false;

        if (playerCombat != null) playerCombat.enabled = false;

        // Detener animaciones
        var animator = GetComponent<Animator>();
        if (animator != null) animator.SetTrigger("Death");
    }

    void EnableControls()
    {
        // Reactivar scripts de control
        var movement = GetComponent<playerMovement>();
        if (movement != null) movement.enabled = true;

        if (playerCombat != null) playerCombat.enabled = true;
    }

    #endregion

    // Método para debug en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(respawnPosition, 0.5f);
    }



}