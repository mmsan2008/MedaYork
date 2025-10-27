using System.Collections;
using UnityEngine;

public class VallaElectrica : MonoBehaviour
{
    [Header("Configuración de la valla")]
    [SerializeField] private float daño = 20f;
    [SerializeField] private float tiempoEntreDaños = 1f;
    [SerializeField] private float duracionActiva = 2f;
    [SerializeField] private float duracionInactiva = 2f;

    [Header("Efecto de ralentización")]
    [SerializeField] private float duracionStun = 1.5f;
    [SerializeField] private float factorRalentizacion = 0.4f;

    [Header("Referencias visuales / sonido")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color colorActivo = Color.cyan;
    [SerializeField] private Color colorInactivo = Color.gray;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoElectrico;

    private bool vallaActiva = true;
    private bool puedeHacerDaño = true;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(CicloValla());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (vallaActiva && puedeHacerDaño && other.CompareTag("Player"))
        {
            StartCoroutine(AplicarEfecto(other));
        }
    }

    private IEnumerator AplicarEfecto(Collider2D jugador)
    {
        puedeHacerDaño = false;

        // Aplica daño usando tu HealthSystem
        HealthSystem vida = jugador.GetComponent<HealthSystem>();
        if (vida != null)
        {
            vida.TakeDamage(daño, transform); // totalmente compatible con tu script
        }

        // Aplica ralentización tipo stun
        playerMovement movimiento = jugador.GetComponent<playerMovement>();
        if (movimiento != null)
        {
            float velocidadOriginal = movimiento.velocidadmovimento; // asegúrate de que esta variable exista
            movimiento.velocidadmovimento *= factorRalentizacion;
            yield return new WaitForSeconds(duracionStun);
            movimiento.velocidadmovimento = velocidadOriginal;
        }

        yield return new WaitForSeconds(tiempoEntreDaños);
        puedeHacerDaño = true;
    }

    private IEnumerator CicloValla()
    {
        while (true)
        {
            // Encender valla
            vallaActiva = true;
            CambiarColor(colorActivo);
            if (audioSource != null && sonidoElectrico != null)
                audioSource.PlayOneShot(sonidoElectrico);

            yield return new WaitForSeconds(duracionActiva);

            // Apagar valla
            vallaActiva = false;
            CambiarColor(colorInactivo);

            yield return new WaitForSeconds(duracionInactiva);
        }
    }

    private void CambiarColor(Color nuevoColor)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = nuevoColor;
    }
}
