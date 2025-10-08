using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform detectionPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDelay = 0.3f; // Tiempo antes de aplicar daño
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new Vector2(1.5f, 1f);

    [Header("Patrol (Optional)")]
    [SerializeField] private bool enablePatrol = true;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTimeAtPoint = 2f;
    private int currentPatrolIndex = 0;

    [Header("Behavior")]
    [SerializeField] private bool returnToPatrol = true;
    [SerializeField] private float losePlayerDistance = 12f;
    
    

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string walkAnimParam = "IsWalking";
    [SerializeField] private string attackAnimParam = "Attack";
    [SerializeField] private string deathAnimParam = "Death";

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool flipToFacePlayer = true;

    [Header("Audio")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip detectionSound;
    [SerializeField] private AudioSource audioSource;

    // Estados del enemigo
    private enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }

    private EnemyState currentState = EnemyState.Idle;
    private Transform player;
    private Rigidbody2D rb;
    private HealthSystem healthSystem;
    private bool canAttack = true;
    private bool isAttacking = false;
    private float waitTimer = 0f;
    private Vector3 startPosition;
    private bool hasDetectedPlayer = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (attackPoint == null)
            attackPoint = transform;

        if (detectionPoint == null)
            detectionPoint = transform;
    }

    void Start()
    {
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Suscribirse a eventos de muerte
        if (healthSystem != null)
        {
            healthSystem.onDeath.AddListener(OnDeath);
        }

        // Iniciar patrullaje si está habilitado
        if (enablePatrol && patrolPoints.Length > 0)
        {
            currentState = EnemyState.Patrol;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;

        // Máquina de estados
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;

            case EnemyState.Patrol:
                HandlePatrolState();
                break;

            case EnemyState.Chase:
                HandleChaseState(distanceToPlayer);
                break;

            case EnemyState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
        }

        // Detectar jugador
        if (player != null && currentState != EnemyState.Attack)
        {
            if (distanceToPlayer <= detectionRange && currentState != EnemyState.Chase)
            {
                OnPlayerDetected();
            }
            else if (distanceToPlayer > losePlayerDistance && currentState == EnemyState.Chase)
            {
                LosePlayer();
            }
        }

        // Actualizar animaciones
        UpdateAnimations();
    }

    #region State Handlers

    void HandleIdleState()
    {
        // Simplemente esperar
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void HandlePatrolState()
    {
        if (patrolPoints.Length == 0)
        {
            currentState = EnemyState.Idle;
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        float distanceToPoint = Vector2.Distance(transform.position, targetPoint.position);

        if (distanceToPoint > 1f)
        {
            // Moverse hacia el punto de patrullaje
            Vector2 direction = (targetPoint.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

            // Voltear hacia la dirección del movimiento
            if (flipToFacePlayer && spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }
        else
        {
            // Llegó al punto, esperar
            rb.velocity = new Vector2(0, rb.velocity.y);
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeAtPoint)
            {
                waitTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
    }

    void HandleChaseState(float distanceToPlayer)
    {
        if (player == null) return;

        // Si está en rango de ataque, cambiar a estado de ataque
        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attack;
            return;
        }

        // Perseguir al jugador
        if (distanceToPlayer > stopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * chaseSpeed, rb.velocity.y);

            // Voltear hacia el jugador
            if (flipToFacePlayer && spriteRenderer != null)
            {
                spriteRenderer.flipX = player.position.x < transform.position.x;
            }
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void HandleAttackState(float distanceToPlayer)
    {
        if (player == null) return;

        // Detener movimiento
        rb.velocity = new Vector2(0, rb.velocity.y);

        // Si el jugador se aleja del rango, volver a perseguir
        if (distanceToPlayer > attackRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        // Mirar hacia el jugador
        if (flipToFacePlayer && spriteRenderer != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }

        // Atacar si puede
        if (canAttack && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }

    #endregion

    #region Attack

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        canAttack = false;

        // Trigger animación
        if (animator != null)
        {
            animator.SetTrigger(attackAnimParam);
        }

        // Sonido de ataque
        PlaySound(attackSound);

        // Esperar antes de aplicar daño (para sincronizar con animación)
        yield return new WaitForSeconds(attackDelay);

        // Aplicar daño
        DealDamage();

        // Cooldown del ataque
        yield return new WaitForSeconds(attackCooldown - attackDelay);

        canAttack = true;
        isAttacking = false;
    }

    void DealDamage()
    {
        // Detectar jugador en área de ataque
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPoint.position, attackSize, 0f, playerLayer);

        foreach (Collider2D hit in hits)
        {
            HealthSystem playerHealth = hit.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage, transform.position);
                Debug.Log($"Enemigo atacó al jugador por {attackDamage} de daño");
            }
        }
    }

    #endregion

    #region Detection

    void OnPlayerDetected()
    {
        if (!hasDetectedPlayer)
        {
            hasDetectedPlayer = true;
            PlaySound(detectionSound);
            Debug.Log("¡Enemigo detectó al jugador!");
        }

        currentState = EnemyState.Chase;
    }

    void LosePlayer()
    {
        hasDetectedPlayer = false;

        if (returnToPatrol && enablePatrol && patrolPoints.Length > 0)
        {
            currentState = EnemyState.Patrol;
            // Encontrar el punto de patrullaje más cercano
            FindNearestPatrolPoint();
        }
        else
        {
            currentState = EnemyState.Idle;
            // Volver a la posición inicial
            StartCoroutine(ReturnToStart());
        }

        Debug.Log("Enemigo perdió al jugador");
    }

    void FindNearestPatrolPoint()
    {
        float minDistance = Mathf.Infinity;
        int nearestIndex = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float distance = Vector2.Distance(transform.position, patrolPoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        currentPatrolIndex = nearestIndex;
    }

    IEnumerator ReturnToStart()
    {
        while (Vector2.Distance(transform.position, startPosition) > 0.5f)
        {
            if (currentState != EnemyState.Idle) yield break;

            Vector2 direction = (startPosition - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

            yield return null;
        }

        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    #endregion

    #region Animations and Visuals

    void UpdateAnimations()
    {
        if (animator == null) return;

        // Actualizar parámetro de caminar
        bool isWalkin = Mathf.Abs(rb.velocity.x) > 0.1f;
        animator.SetBool(walkAnimParam, isWalkin);
    }

    void OnDeath()
    {
        currentState = EnemyState.Dead;

        if (animator != null)
        {
            animator.SetTrigger(deathAnimParam);
        }

        // Desactivar componentes
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Desactivar colisiones
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Destruir después de un tiempo
        Destroy(gameObject, 2f);

        Debug.Log("Enemigo murió");
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Public Methods

    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
        if (points.Length > 0 && currentState == EnemyState.Idle)
        {
            currentState = EnemyState.Patrol;
        }
    }

    public void ForceChasePlayer()
    {
        if (player != null)
        {
            OnPlayerDetected();
        }
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        if (detectionPoint != null)
            Gizmos.DrawWireSphere(detectionPoint.position, detectionRange);
        else
            Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque
        Gizmos.color = Color.red;
        if (detectionPoint != null)
            Gizmos.DrawWireSphere(detectionPoint.position, attackRange);
        else
            Gizmos.DrawWireSphere(transform.position, attackRange);

        // Área de ataque
        if (attackPoint != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(attackPoint.position, attackSize);
        }

        // Puntos de patrullaje
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);

                    // Líneas entre puntos
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                }
            }

            // Línea de vuelta al primer punto
            if (patrolPoints[0] != null && patrolPoints[patrolPoints.Length - 1] != null)
            {
                Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
            }
        }
    }

    #endregion
}