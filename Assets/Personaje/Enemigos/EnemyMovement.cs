using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 3f;
    private Vector2 moveDirection;
    public Camera mainCamera;
    public int damage = 1;
    public Animator animator;
    private float despawnMargin = 1.2f;
    private bool isDespawning = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Animator animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isDespawning)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime);

            // Voltear sprite según dirección horizontal
            if (moveDirection.x > 0)
            {
                // Va hacia la derecha
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else if (moveDirection.x < 0)
            {
                // Va hacia la izquierda
                GetComponent<SpriteRenderer>().flipX = true;
            }


            if (IsFarOutsideCamera())
                StartCoroutine(FadeAndDestroy());
        }
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction;
    }

    bool IsFarOutsideCamera()
    {
        if (mainCamera == null) return false;
        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        return viewPos.x < -despawnMargin || viewPos.x > 1 + despawnMargin;
    }

    private System.Collections.IEnumerator FadeAndDestroy()
    {
        isDespawning = true;
        Color c = spriteRenderer.color;
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1, 0, t);
            spriteRenderer.color = c;
            yield return null;
        }
        Destroy(gameObject);
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
}
