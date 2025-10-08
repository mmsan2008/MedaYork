using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem healthSystem;

    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarBackground;
    [SerializeField] private Gradient healthBarGradient;
    [SerializeField] private bool useGradient = true;

    [Header("Delayed Bar (Optional)")]
    [SerializeField] private bool useDelayedBar = true;
    [SerializeField] private Image delayedHealthBar;
    [SerializeField] private float delayedBarSpeed = 2f;
    [SerializeField] private Color delayedBarColor = new Color(1, 0.5f, 0.5f, 0.5f);

    [Header("Text Display")]
    [SerializeField] private bool showHealthText = true;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private bool showAsPercentage = false;

    [Header("Hearts System (Optional)")]
    [SerializeField] private bool useHeartsSystem = false;
    [SerializeField] private Transform heartsContainer;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private float healthPerHeart = 20f;

    [Header("Animation")]
    [SerializeField] private bool animateOnDamage = true;
    [SerializeField] private float shakeIntensity = 10f;
    [SerializeField] private float shakeDuration = 0.2f;

    [Header("Flash Effect")]
    [SerializeField] private bool flashOnDamage = true;
    [SerializeField] private float flashSpeed = 5f;

    private float targetHealthBarValue;
    private float currentDelayedValue;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Start()
    {
        if (healthSystem == null)
        {
            healthSystem = FindObjectOfType<HealthSystem>();
        }

        if (healthSystem != null)
        {
            // Suscribirse a eventos
            healthSystem.onHealthChanged.AddListener(UpdateHealthBar);
            healthSystem.onDamageTaken.AddListener(OnDamageTaken);
            healthSystem.onHealed.AddListener(OnHealed);
        }

        // Configuración inicial
        if (healthBarFill != null)
        {
            originalPosition = healthBarFill.transform.parent.localPosition;
        }

        if (useDelayedBar && delayedHealthBar != null)
        {
            delayedHealthBar.color = delayedBarColor;
            currentDelayedValue = 1f;
        }

        // Crear corazones si se usa ese sistema
        if (useHeartsSystem)
        {
            CreateHearts();
        }

        // Inicializar UI
        UpdateHealthBar(healthSystem != null ? healthSystem.CurrentHealth : 100f);
    }

    void Update()
    {
        // Actualizar barra retrasada suavemente
        if (useDelayedBar && delayedHealthBar != null)
        {
            if (currentDelayedValue > targetHealthBarValue)
            {
                currentDelayedValue = Mathf.Lerp(currentDelayedValue, targetHealthBarValue, Time.deltaTime * delayedBarSpeed);
                delayedHealthBar.fillAmount = currentDelayedValue;
            }
        }
    }

    void UpdateHealthBar(float currentHealth)
    {
        if (healthSystem == null) return;

        float fillAmount = currentHealth / healthSystem.MaxHealth;
        targetHealthBarValue = fillAmount;

        // Actualizar barra principal
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = fillAmount;

            // Aplicar gradiente de color
            if (useGradient && healthBarGradient != null)
            {
                healthBarFill.color = healthBarGradient.Evaluate(fillAmount);
            }
        }

        // Actualizar texto
        if (showHealthText && healthText != null)
        {
            if (showAsPercentage)
            {
                healthText.text = $"{Mathf.RoundToInt(fillAmount * 100)}%";
            }
            else
            {
                healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(healthSystem.MaxHealth)}";
            }
        }

        // Actualizar corazones
        if (useHeartsSystem)
        {
            UpdateHearts(currentHealth);
        }
    }

    void OnDamageTaken(float damage)
    {
        if (animateOnDamage)
        {
            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeHealthBar());
        }

        if (flashOnDamage && healthBarFill != null)
        {
            StartCoroutine(FlashHealthBar());
        }
    }

    void OnHealed(float amount)
    {
        // Efecto de curación (opcional)
        if (healthBarFill != null)
        {
            StartCoroutine(PulseHealthBar());
        }
    }

    #region Hearts System

    void CreateHearts()
    {
        if (heartsContainer == null || heartPrefab == null || healthSystem == null) return;

        // Limpiar corazones existentes
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        // Crear corazones según la vida máxima
        int heartCount = Mathf.CeilToInt(healthSystem.MaxHealth / healthPerHeart);

        for (int i = 0; i < heartCount; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartsContainer);
            heart.name = $"Heart_{i}";
        }
    }

    void UpdateHearts(float currentHealth)
    {
        if (heartsContainer == null) return;

        int heartCount = heartsContainer.childCount;

        for (int i = 0; i < heartCount; i++)
        {
            Image heartImage = heartsContainer.GetChild(i).GetComponent<Image>();
            if (heartImage == null) continue;

            float heartMinHealth = i * healthPerHeart;
            float heartMaxHealth = (i + 1) * healthPerHeart;

            if (currentHealth >= heartMaxHealth)
            {
                // Corazón lleno
                heartImage.sprite = fullHeartSprite;
            }
            else if (currentHealth > heartMinHealth)
            {
                // Corazón medio
                heartImage.sprite = halfHeartSprite != null ? halfHeartSprite : fullHeartSprite;
            }
            else
            {
                // Corazón vacío
                heartImage.sprite = emptyHeartSprite;
            }
        }
    }

    #endregion

    #region Visual Effects

    IEnumerator ShakeHealthBar()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            healthBarFill.transform.parent.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        healthBarFill.transform.parent.localPosition = originalPosition;
    }

    IEnumerator FlashHealthBar()
    {
        if (healthBarFill == null) yield break;

        Color originalColor = healthBarFill.color;
        Color flashColor = Color.white;

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            healthBarFill.color = Color.Lerp(flashColor, originalColor, elapsed / duration);
            elapsed += Time.deltaTime * flashSpeed;
            yield return null;
        }

        healthBarFill.color = originalColor;
    }

    IEnumerator PulseHealthBar()
    {
        if (healthBarFill == null) yield break;

        Vector3 originalScale = healthBarFill.transform.localScale;
        Vector3 targetScale = originalScale * 1.1f;

        float duration = 0.3f;
        float elapsed = 0f;

        // Expandir
        while (elapsed < duration / 2)
        {
            healthBarFill.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / (duration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        // Contraer
        while (elapsed < duration / 2)
        {
            healthBarFill.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / (duration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        healthBarFill.transform.localScale = originalScale;
    }

    #endregion

    // Métodos públicos para control manual
    public void SetHealthSystem(HealthSystem newHealthSystem)
    {
        if (healthSystem != null)
        {
            healthSystem.onHealthChanged.RemoveListener(UpdateHealthBar);
            healthSystem.onDamageTaken.RemoveListener(OnDamageTaken);
            healthSystem.onHealed.RemoveListener(OnHealed);
        }

        healthSystem = newHealthSystem;

        if (healthSystem != null)
        {
            healthSystem.onHealthChanged.AddListener(UpdateHealthBar);
            healthSystem.onDamageTaken.AddListener(OnDamageTaken);
            healthSystem.onHealed.AddListener(OnHealed);

            UpdateHealthBar(healthSystem.CurrentHealth);
        }
    }

    void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.onHealthChanged.RemoveListener(UpdateHealthBar);
            healthSystem.onDamageTaken.RemoveListener(OnDamageTaken);
            healthSystem.onHealed.RemoveListener(OnHealed);
        }
    }
}