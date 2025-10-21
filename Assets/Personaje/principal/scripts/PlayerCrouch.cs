using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouch : MonoBehaviour
{
    [Header("Crouch Settings")]
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private bool holdToCrouch = true; // true = mantener, false = toggle
    [SerializeField] private float crouchSpeed = 2f; // Velocidad mientras está agachado
    [SerializeField] private float crouchHeight = 0.5f; // Multiplicador de altura (50%)

    [Header("Collider Settings")]
    [SerializeField] private CapsuleCollider2D playerCollider; // O BoxCollider2D
    [SerializeField] private Vector2 standingColliderSize;
    [SerializeField] private Vector2 standingColliderOffset;
    [SerializeField] private Vector2 crouchingColliderSize;
    [SerializeField] private Vector2 crouchingColliderOffset;

    [Header("Ground Check (para evitar levantarse en obstáculos)")]
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private Vector2 ceilingCheckSize = new Vector2(0.8f, 0.2f);
    [SerializeField] private LayerMask ceilingLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string crouchAnimParam = "IsCrouching";

    [Header("Visual (Opcional)")]
    [SerializeField] private Transform playerVisual; // El sprite del jugador
    [SerializeField] private bool scaleVisual = true; // Escalar el sprite al agacharse
    [SerializeField] private Vector3 crouchScale = new Vector3(1f, 0.5f, 1f);
    private Vector3 originalScale;

    // Estados
    private bool isCrouching = false;
    private bool canStandUp = true;

    // Referencias
    private Rigidbody2D rb;
    private playerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerAttack playerAttack;
    private BoxCollider2D boxCollider; // Si usas BoxCollider2D

    // Propiedades públicas
    public bool IsCrouching => isCrouching;
    public float CrouchSpeedMultiplier => crouchSpeed / (playerMovement != null ? playerMovement.velocidadmovimento : 5f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<playerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        playerAttack = GetComponent<PlayerAttack>();

        // Detectar tipo de collider
        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider2D>();
            if (playerCollider == null)
            {
                boxCollider = GetComponent<BoxCollider2D>();
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (playerVisual == null)
        {
            playerVisual = transform;
        }

        // Crear ceiling check si no existe
        if (ceilingCheck == null)
        {
            GameObject checkObj = new GameObject("CeilingCheck");
            checkObj.transform.SetParent(transform);
            checkObj.transform.localPosition = new Vector3(0, 1f, 0); // Arriba de la cabeza
            ceilingCheck = checkObj.transform;
        }
    }

    void Start()
    {
        // Guardar valores originales
        if (playerCollider != null)
        {
            standingColliderSize = playerCollider.size;
            standingColliderOffset = playerCollider.offset;

            // Calcular tamaño agachado automáticamente
            if (crouchingColliderSize == Vector2.zero)
            {
                crouchingColliderSize = new Vector2(standingColliderSize.x, standingColliderSize.y * crouchHeight);
            }
            if (crouchingColliderOffset == Vector2.zero)
            {
                float heightDifference = (standingColliderSize.y - crouchingColliderSize.y) / 2f;
                crouchingColliderOffset = new Vector2(standingColliderOffset.x, standingColliderOffset.y - heightDifference);
            }
        }
        else if (boxCollider != null)
        {
            standingColliderSize = boxCollider.size;
            standingColliderOffset = boxCollider.offset;

            if (crouchingColliderSize == Vector2.zero)
            {
                crouchingColliderSize = new Vector2(standingColliderSize.x, standingColliderSize.y * crouchHeight);
            }
            if (crouchingColliderOffset == Vector2.zero)
            {
                float heightDifference = (standingColliderSize.y - crouchingColliderSize.y) / 2f;
                crouchingColliderOffset = new Vector2(standingColliderOffset.x, standingColliderOffset.y - heightDifference);
            }
        }

        originalScale = playerVisual.localScale;
    }

    void Update()
    {
        HandleCrouchInput();
        CheckIfCanStandUp();
    }

    void HandleCrouchInput()
    {
        if (holdToCrouch)
        {
            // Modo: Mantener tecla presionada
            if (Input.GetKey(crouchKey) && !isCrouching)
            {
                StartCrouch();
            }
            else if (!Input.GetKey(crouchKey) && isCrouching && canStandUp)
            {
                StandUp();
            }
        }
        else
        {
            // Modo: Toggle (presionar para agacharse/levantarse)
            if (Input.GetKeyDown(crouchKey))
            {
                if (!isCrouching)
                {
                    StartCrouch();
                }
                else if (canStandUp)
                {
                    StandUp();
                }
            }
        }
    }

    void CheckIfCanStandUp()
    {
        if (isCrouching)
        {
            // Verificar si hay obstáculos arriba
            canStandUp = !Physics2D.OverlapBox(ceilingCheck.position, ceilingCheckSize, 0f, ceilingLayer);

            // Debug visual
            if (!canStandUp)
            {
                Debug.Log("No puedo levantarme, hay un obstáculo arriba");
            }
        }
    }

    void StartCrouch()
    {
        isCrouching = true;

        // Cambiar collider
        UpdateCollider(true);

        // Reducir velocidad de movimiento
        if (playerMovement != null)
        {
            playerMovement.SetSpeedMultiplier(crouchSpeed / playerMovement.velocidadmovimento);
        }

        // Escalar visual - CORREGIDO
        if (scaleVisual && playerVisual != null)
        {
            Vector3 newScale = playerVisual.localScale;
            newScale.y = Mathf.Abs(newScale.y) * crouchScale.y; // Mantener dirección Y positiva
            playerVisual.localScale = newScale;
        }

        // Animación
        if (animator != null)
        {
            animator.SetBool(crouchAnimParam, true);
        }

        Debug.Log("Agachado");
    }

    void StandUp()
    {
        isCrouching = false;

        // Restaurar collider
        UpdateCollider(false);

        // Restaurar velocidad normal
        if (playerMovement != null)
        {
            playerMovement.SetSpeedMultiplier(1f);
        }

        // Restaurar escala visual - CORREGIDO
        if (scaleVisual && playerVisual != null)
        {
            Vector3 newScale = playerVisual.localScale;
            newScale.y = Mathf.Abs(originalScale.y); // Restaurar Y positivo original
            playerVisual.localScale = newScale;
        }

        // Animación
        if (animator != null)
        {
            animator.SetBool(crouchAnimParam, false);
        }

        Debug.Log("De pie");
    }

    void UpdateCollider(bool crouch)
    {
        if (playerCollider != null)
        {
            if (crouch)
            {
                playerCollider.size = crouchingColliderSize;
                playerCollider.offset = crouchingColliderOffset;
            }
            else
            {
                playerCollider.size = standingColliderSize;
                playerCollider.offset = standingColliderOffset;
            }
        }
        else if (boxCollider != null)
        {
            if (crouch)
            {
                boxCollider.size = crouchingColliderSize;
                boxCollider.offset = crouchingColliderOffset;
            }
            else
            {
                boxCollider.size = standingColliderSize;
                boxCollider.offset = standingColliderOffset;
            }
        }
    }

    // Método público para verificar si puede realizar acciones
    public bool CanJump()
    {
        return !isCrouching;
    }

    public bool CanDash()
    {
        return !isCrouching;
    }

    public bool CanAttack()
    {
        // Puedes cambiar esto a true si quieres atacar agachado
        return !isCrouching;
    }

    // Método para forzar levantarse (útil para ciertas mecánicas)
    public void ForceStandUp()
    {
        if (isCrouching && canStandUp)
        {
            StandUp();
        }
    }

    // Gizmos para visualizar en el editor
    void OnDrawGizmos()
    {
        if (ceilingCheck != null)
        {
            Gizmos.color = canStandUp ? Color.green : Color.red;
            Gizmos.DrawWireCube(ceilingCheck.position, ceilingCheckSize);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Mostrar colliders en el editor
        if (playerCollider != null || boxCollider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);

            // Collider de pie
            Vector3 standPos = transform.position + (Vector3)standingColliderOffset;
            Gizmos.DrawWireCube(standPos, standingColliderSize);

            // Collider agachado
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Vector3 crouchPos = transform.position + (Vector3)crouchingColliderOffset;
            Gizmos.DrawWireCube(crouchPos, crouchingColliderSize);
        }
    }
}