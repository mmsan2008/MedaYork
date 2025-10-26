using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NeaBoss : MonoBehaviour
{
    [Header("General Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthBar;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform leftLimit;
    public Transform rightLimit;
    private bool movingRight = true;

    [Header("Attacks")]
    public GameObject smokePrefab;
    public GameObject knifePrefab;
    public GameObject chargeEffect;
    public Transform topSpawnPoint;
    public Transform bottomSpawnPoint;

    private bool isAttacking = false;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        StartCoroutine(BossBehavior());
    }

    void Update()
    {
        healthBar.value = currentHealth;
    }

    IEnumerator BossBehavior()
    {
        while (currentHealth > 0)
        {
            if (!isAttacking)
            {
                yield return MovePattern();
                yield return ChooseAttack();
            }
            yield return null;
        }
        // Aquí puedes poner animación de muerte o transición
    }

    IEnumerator MovePattern()
    {
        while (!isAttacking)
        {
            Transform target = movingRight ? rightLimit : leftLimit;
            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target.position) < 0.1f)
            {
                movingRight = !movingRight;
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator ChooseAttack()
    {
        isAttacking = true;
        int attackIndex = Random.Range(0, 3);

        switch (attackIndex)
        {
            case 0:
                yield return SmokeAttack();
                break;
            case 1:
                yield return KnifeAttack();
                break;
            case 2:
                yield return ChargeAttack();
                break;
        }

        isAttacking = false;
    }

    IEnumerator SmokeAttack()
    {
        yield return new WaitForSeconds(1f);
        Instantiate(smokePrefab, topSpawnPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(2f);
        Instantiate(smokePrefab, bottomSpawnPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(2f);
    }

    IEnumerator KnifeAttack()
    {
        int knifeCount = 10;
        for (int i = 0; i < knifeCount; i++)
        {
            Vector2 pos = new Vector2(Random.Range(-8f, 8f), topSpawnPoint.position.y);
            Instantiate(knifePrefab, pos, Quaternion.identity);
        }
        yield return new WaitForSeconds(3f);
    }

    IEnumerator ChargeAttack()
    {
        yield return new WaitForSeconds(1f); // carga
        Vector2 dir = movingRight ? Vector2.right : Vector2.left;
        float chargeTime = 1.5f;
        float chargeSpeed = 15f;

        float timer = 0;
        while (timer < chargeTime)
        {
            transform.Translate(dir * chargeSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(1f);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        // Animación y lógica de muerte
        Debug.Log("Nea derrotada");
    }
}
