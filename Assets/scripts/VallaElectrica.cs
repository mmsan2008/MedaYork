using System.Collections;
using UnityEngine;

public class VallaElectrica : MonoBehaviour
{
    [Header("Configuraci�n de la valla")]
    [SerializeField] private float da�o = 20f;
    [SerializeField] private float tiempoEntreDa�os = 1f;
    [SerializeField] private float duracionActiva = 2f;
    [SerializeField] private float duracionInactiva = 2f;

    [Header("Efecto de ralentizaci�n")]
    [SerializeField] private float duracionStun = 1.5f;
    [SerializeField] private float factorRalentizacion = 0.4f;

    [Header("Referencias visuales / sonido")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color colorActivo = Color.cyan;
    [SerializeField] private Color colorInactivo = Color.gray;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoElectrico;

    private bool vallaActiva = true;
    private bool puedeHacerDa�o = true;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(CicloValla());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (vallaActiva && puedeHacerDa�o && other.CompareTag("Player"))
        {
            StartCoroutine(AplicarEfecto(other));
        }
    }

    private IEnumerator AplicarEfecto(Collider2D jugador)
    {
        puedeHacerDa�o = false;

        // Aplica da�o usando tu HealthSystem
        HealthSystem vida = jugador.GetComponent<HealthSystem>();
        if (vida != null)
        {
            vida.TakeDamage(da�o, transform); // totalmente compatible con tu script
        }

        // Aplica ralentizaci�n tipo stun
        playerMovement movimiento = jugador.GetComponent<playerMovement>();
        if (movimiento != null)
        {
            float velocidadOriginal = movimiento.velocidadmovimento; // aseg�rate de que esta variable exista
            movimiento.velocidadmovimento *= factorRalentizacion;
            yield return new WaitForSeconds(duracionStun);
            movimiento.velocidadmovimento = velocidadOriginal;
        }

        yield return new WaitForSeconds(tiempoEntreDa�os);
        puedeHacerDa�o = true;
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
