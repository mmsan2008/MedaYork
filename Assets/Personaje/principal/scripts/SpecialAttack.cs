using UnityEngine;
using System.Collections;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEngine.UI;
using TMPro;
#endif

public class SpecialAttack : MonoBehaviour
{
    [Header("Special Attack Settings")]
    [SerializeField] public GameObject specialProjectilePrefab; // Prefab de la papa bomba
    [SerializeField] private Transform firePoint; // Punto de disparo
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float projectileDamage = 50f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] public LayerMask enemyLayer;

    [Header("Charge System")]
    [SerializeField] private int killsRequired = 5; // Enemigos a matar
    [SerializeField] private int maxCharges = 3; // Máximo de cargas acumulables
    [SerializeField] private bool resetOnUse = false; // ¿Reiniciar contador al usar?

    [Header("Input")]
    [SerializeField] private KeyCode specialAttackKey = KeyCode.E;

    [Header("Cooldown")]
    [SerializeField] private bool useCooldown = true;
    [SerializeField] private float cooldownTime = 2f;
    private float cooldownTimer = 0f;

    [Header("UI")]
    [SerializeField] public Image chargeBar; // Barra de carga
    [SerializeField] public TextMeshProUGUI chargeText; // Texto "3/5"
    [SerializeField] public TextMeshProUGUI chargesAvailableText; // "x2"
    [SerializeField] public GameObject specialAttackIcon; // Ícono del ataque
    [SerializeField] public Color availableColor = Color.green;
    [SerializeField] public Color unavailableColor = Color.gray;

    [Header("Visual Effects")]
    [SerializeField] private GameObject chargeReadyEffect; // Efecto cuando se carga
    [SerializeField] private float effectDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip chargeReadySound;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string throwAnimParam = "ThrowBomb";

   

    // Estado
    private int currentKills = 0;
    private int currentCharges = 0;
    private bool canUseSpecial = false;

    // Referencias
    private PlayerCombat playerCombat;
    private PlayerCrouch playerCrouch;

    // Eventos para otros sistemas
    public System.Action<int> onKillCountChanged; // Se dispara cuando cambia el contador
    public System.Action onChargeReady; // Se dispara cuando se completa una carga
    public System.Action onSpecialUsed; // Se dispara al usar el especial

    void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();
        playerCrouch = GetComponent<PlayerCrouch>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (firePoint == null)
            firePoint = transform;
    }

    void Start()
    {
        // Suscribirse a las muertes de enemigos
        SubscribeToEnemies();

        UpdateUI();

        if (chargeReadyEffect != null)
            chargeReadyEffect.SetActive(false);
    }

    void Update()
    {
        // Actualizar cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Input para usar ataque especial
        if (Input.GetKeyDown(specialAttackKey))
        {
            TryUseSpecialAttack();
        }

        UpdateUI();
    }



    #region Kill Tracking

    void SubscribeToEnemies()
    {
        // Encontrar todos los enemigos en la escena
        HealthSystem[] enemies = FindObjectsOfType<HealthSystem>();

        foreach (HealthSystem enemy in enemies)
        {
            // Solo suscribirse a enemigos, no al jugador
            if (enemy.gameObject != gameObject && enemy.CompareTag("Enemy"))
            {
                enemy.onDeath.AddListener(() => OnEnemyKilled(enemy.gameObject));
            }
        }
    }

    public void OnEnemyKilled(GameObject enemy)
    {
        // Incrementar contador de muertes
        currentKills++;

        Debug.Log($"Enemigo muerto. Progreso: {currentKills}/{killsRequired}");

        onKillCountChanged?.Invoke(currentKills);

        // Verificar si se completó una carga
        if (currentKills >= killsRequired)
        {
            AddCharge();
        }

        UpdateUI();
    }

    void AddCharge()
    {
        if (currentCharges < maxCharges)
        {
            currentCharges++;
            currentKills = 0; // Reiniciar contador
            canUseSpecial = true;

            Debug.Log($"¡Carga lista! Total: {currentCharges}/{maxCharges}");

            // Efectos visuales y sonoros
            ShowChargeReadyEffect();
            PlaySound(chargeReadySound);

            onChargeReady?.Invoke();
        }
        else
        {
            // Ya tiene el máximo, solo reiniciar contador
            currentKills = 0;
            Debug.Log("Ya tienes el máximo de cargas");
        }

        UpdateUI();
    }

    #endregion

    #region Special Attack

    void TryUseSpecialAttack()
    {
        // Verificaciones
        if (currentCharges <= 0)
        {
            Debug.Log("No tienes cargas disponibles");
            return;
        }

        if (cooldownTimer > 0)
        {
            Debug.Log("Ataque especial en cooldown");
            return;
        }

        // Verificar si puede atacar (no bloqueando, etc.)
        if (playerCombat != null && !playerCombat.CanAttack())
        {
            Debug.Log("No puedes usar ataque especial mientras bloqueas");
            return;
        }

        if (playerCrouch != null && playerCrouch.IsCrouching)
        {
            Debug.Log("No puedes usar ataque especial agachado");
            return;
        }

        // Usar ataque especial
        UseSpecialAttack();
    }

    void UseSpecialAttack()
    {
        // Consumir carga
        currentCharges--;

        if (resetOnUse)
        {
            currentKills = 0;
        }

        // Cooldown
        if (useCooldown)
        {
            cooldownTimer = cooldownTime;
        }

        // Actualizar estado
        canUseSpecial = currentCharges > 0;

        // Lanzar proyectil
        StartCoroutine(ThrowBombCoroutine());

        onSpecialUsed?.Invoke();

        Debug.Log($"¡Ataque especial usado! Cargas restantes: {currentCharges}");

        UpdateUI();
    }

    IEnumerator ThrowBombCoroutine()
    {
        // Animación
        if (animator != null)
        {
            animator.SetTrigger(throwAnimParam);
        }

        // Sonido
        PlaySound(throwSound);

        // Pequeño delay para sincronizar con animación
        yield return new WaitForSeconds(0.2f);

        Debug.Log("=== CREANDO BOMBA ===");

        // Instanciar proyectil
        if (specialProjectilePrefab != null)
        {
            Debug.Log("Prefab OK");

            // Determinar dirección
            Vector2 direction = GetThrowDirection();
            Debug.Log($"Dirección calculada: {direction}");

            // Crear proyectil
            GameObject projectile = Instantiate(specialProjectilePrefab, firePoint.position, Quaternion.identity);
            Debug.Log($"Proyectil creado en: {firePoint.position}");

            // Configurar proyectil
            SpecialProjectile projScript = projectile.GetComponent<SpecialProjectile>();
            if (projScript != null)
            {
                Debug.Log("Script encontrado, inicializando...");
                projScript.Initialize(direction, projectileSpeed, projectileDamage, explosionRadius, enemyLayer, gameObject);
            }
            else
            {
                Debug.LogError("NO SE ENCONTRÓ EL SCRIPT SpecialProjectile!");
            }

            // Rotar proyectil hacia la dirección
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            Debug.LogError("PREFAB ES NULL!");
        }
    }

    Vector2 GetThrowDirection()
    {
        // Obtener dirección según donde mira el jugador
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        float dirX = 1f; // Por defecto hacia la derecha

        if (sprite != null)
        {
            dirX = sprite.flipX ? -1f : 1f;
        }
        else
        {
            // Si no hay SpriteRenderer, usar la escala
            dirX = transform.localScale.x > 0 ? 1f : -1f;
        }

        // Lanzar en ángulo de 45 grados hacia arriba
        Vector2 throwDir = new Vector2(dirX * 0.8f, 0.6f).normalized;

        Debug.Log($"Dirección de lanzamiento: {throwDir}");

        return throwDir;
    }


    #endregion

    #region UI Updates

    void UpdateUI()
    {
        // Actualizar barra de progreso
        if (chargeBar != null)
        {
            float progress = (float)currentKills / killsRequired;
            chargeBar.fillAmount = progress;
        }

        // Actualizar texto de progreso
        if (chargeText != null)
        {
            chargeText.text = $"{currentKills}/{killsRequired}";
        }

        // Actualizar cargas disponibles
        if (chargesAvailableText != null)
        {
            chargesAvailableText.text = $"x{currentCharges}";

            // Cambiar color según disponibilidad
            chargesAvailableText.color = currentCharges > 0 ? availableColor : unavailableColor;
        }

        // Actualizar ícono
        if (specialAttackIcon != null)
        {
            Image iconImage = specialAttackIcon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.color = currentCharges > 0 ? availableColor : unavailableColor;
            }
        }
    }

    void ShowChargeReadyEffect()
    {
        if (chargeReadyEffect != null)
        {
            StartCoroutine(ShowEffectCoroutine());
        }
    }

    IEnumerator ShowEffectCoroutine()
    {
        chargeReadyEffect.SetActive(true);
        yield return new WaitForSeconds(effectDuration);
        chargeReadyEffect.SetActive(false);
    }

    #endregion

    #region Audio

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Public Methods

    // Método para agregar kills manualmente (útil para testing)
    public void AddKill()
    {
        OnEnemyKilled(null);
    }

    // Método para agregar carga directamente
    public void AddChargeDirectly()
    {
        if (currentCharges < maxCharges)
        {
            currentCharges++;
            canUseSpecial = true;
            UpdateUI();
        }
    }

    // Getters
    public int GetCurrentKills() => currentKills;
    public int GetCurrentCharges() => currentCharges;
    public int GetMaxCharges() => maxCharges;
    public bool CanUse() => currentCharges > 0 && cooldownTimer <= 0;




    #endregion
}