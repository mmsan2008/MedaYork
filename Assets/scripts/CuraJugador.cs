using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuraJugador : MonoBehaviour
{
    [Header("Configuración de curación")]
    public int cantidadCuracion = 20;
    public float tiempoParaDesaparecer = 0.2f; // efecto visual antes de destruirse

    private bool fueUsado = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D colisionador;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (fueUsado) return; // evita duplicar curación

        if (other.CompareTag("Player"))
        {
            HealthSystem saludJugador = other.GetComponent<HealthSystem>();

            if (saludJugador != null)
            {
                saludJugador.Heal(cantidadCuracion);
            }

            // Desactivar visual y colisión
            fueUsado = true;
            spriteRenderer.enabled = false;
            colisionador.enabled = false;

            // Destruir después de un pequeño retraso
            Destroy(gameObject, tiempoParaDesaparecer);
        }
    }
}
