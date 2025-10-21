using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Block Settings")]
    [SerializeField] private KeyCode blockKey = KeyCode.Mouse1; // Click derecho
    [SerializeField] private float blockDamageReduction = 0.7f; // Reduce 70% del daño
    [SerializeField] private float blockMovementSpeed = 0.3f; // 30% de velocidad al bloquear
    [SerializeField] private bool canMoveWhileBlocking = true;

    [Header("Block Cooldown")]
    [SerializeField] private bool useBlockCooldown = false;
    [SerializeField] private float blockCooldown = 1f;
    private float blockCooldownTimer = 0f;

    [Header("Stamina (Opcional)")]
    [SerializeField] private bool useStamina = false;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float staminaDrainPerSecond = 20f;
    [SerializeField] private float staminaRegenPerSecond = 15f;
    [SerializeField] private float minStaminaToBlock = 10f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject blockVisual; // Sprite o efecto visual del escudo

    // Estados
    private bool isBlocking = false;
    private bool canBlock = true;

    // Referencias a otros componentes (ajusta según tu código)
    private Rigidbody2D rb;
    private playerMovement playerMovement; // Tu script de movimiento

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<playerMovement>();

        if (blockVisual != null)
            blockVisual.SetActive(false);

        currentStamina = maxStamina;
    }

    void Update()
    {
        HandleBlockInput();
        UpdateStamina();
        UpdateCooldown();
    }

    void HandleBlockInput()
    {
        // Verificar si se puede bloquear
        bool canStartBlock = canBlock &&
                            (!useStamina || currentStamina >= minStaminaToBlock) &&
                            (!useBlockCooldown || blockCooldownTimer <= 0);

        // Iniciar bloqueo
        if (Input.GetKey(blockKey) && canStartBlock && !isBlocking)
        {
            StartBlock();
            
        }
        // Mantener bloqueo
        else if (Input.GetKey(blockKey) && isBlocking)
        {
            MaintainBlock();
        }
        // Terminar bloqueo
        else if (Input.GetKeyUp(blockKey) && isBlocking)
        {
            EndBlock();
        }

        // Terminar bloqueo si se acaba la stamina
        if (useStamina && isBlocking && currentStamina <= 0)
        {
            EndBlock();
        }
    }

    void StartBlock()
    {
        isBlocking = true;

        // Activar animación
        animator.SetTrigger("isBlocking");


        // Mostrar visual del bloqueo
        if (blockVisual != null)
            blockVisual.SetActive(true);

        // Reducir velocidad de movimiento
        if (playerMovement != null && canMoveWhileBlocking)
        {
            playerMovement.SetSpeedMultiplier(blockMovementSpeed);
        }
        else if (!canMoveWhileBlocking)
        {
            // Detener completamente el movimiento
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        Debug.Log("Bloqueo activado");
    }

    void MaintainBlock()
    {
        // Drenar stamina mientras se bloquea
        if (useStamina)
        {
            currentStamina -= staminaDrainPerSecond * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
        }

        // Prevenir movimiento si está configurado
        if (!canMoveWhileBlocking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void EndBlock()
    {
        isBlocking = false;

        // Desactivar animación
        
            

        // Ocultar visual del bloqueo
        if (blockVisual != null)
            blockVisual.SetActive(false);

        // Restaurar velocidad normal
        if (playerMovement != null)
        {
            playerMovement.SetSpeedMultiplier(1f);
        }

        // Iniciar cooldown
        if (useBlockCooldown)
        {
            blockCooldownTimer = blockCooldown;
        }

        Debug.Log("Bloqueo desactivado");
    }

    void UpdateStamina()
    {
        if (useStamina && !isBlocking)
        {
            // Regenerar stamina cuando no se está bloqueando
            currentStamina += staminaRegenPerSecond * Time.deltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
        }
    }

    void UpdateCooldown()
    {
        if (blockCooldownTimer > 0)
        {
            blockCooldownTimer -= Time.deltaTime;
        }
    }

    // Método para recibir daño (llamar desde tu sistema de salud)
    public float TakeDamage(float damage)
    {
        if (isBlocking)
        {
            // Reducir el daño según el porcentaje de reducción
            float reducedDamage = damage * (1 - blockDamageReduction);
            Debug.Log($"¡Bloqueado! Daño reducido de {damage} a {reducedDamage}");
            return reducedDamage;
        }

        return damage; // Daño completo si no está bloqueando
    }

    // Prevenir otras acciones mientras se bloquea
    public bool CanAttack()
    {
        return !isBlocking;
    }

    public bool CanDash()
    {
        return !isBlocking;
    }

    // Getters públicos
    public bool IsBlocking()
    {
        return isBlocking;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public float GetMaxStamina()
    {
        return maxStamina;
    }

    public bool IsOnCooldown()
    {
        return blockCooldownTimer > 0;
    }

    // Método opcional para forzar el fin del bloqueo (ej: al ser golpeado)
    public void ForceEndBlock()
    {
        if (isBlocking)
        {
            EndBlock();
        }
    }
}