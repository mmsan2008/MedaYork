using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaFuego : MonoBehaviour
{
    [Header("Configuración del fuego")]
    public SpriteRenderer fuegoVisual; // Sprite del fuego
    public Color colorEncendido = Color.red;
    public Color colorApagado = new Color(1f, 0.5f, 0.5f); // tono más apagado
    public float dañoPorSegundo = 30f;
    public float intervaloEncendido = 2f; // tiempo encendido
    public float intervaloApagado = 2f;   // tiempo apagado

    private bool fuegoEncendido = true;
    private float temporizador;

    private void Start()
    {
        temporizador = intervaloEncendido;
        if (fuegoVisual != null)
            fuegoVisual.color = colorEncendido;
    }

    private void Update()
    {
        temporizador -= Time.deltaTime;

        if (fuegoEncendido && temporizador <= 0)
        {
            // Apagar visualmente el fuego (cambia color, no desaparece)
            fuegoEncendido = false;
            temporizador = intervaloApagado;

            if (fuegoVisual != null)
                fuegoVisual.color = colorApagado;
        }
        else if (!fuegoEncendido && temporizador <= 0)
        {
            // Encender el fuego
            fuegoEncendido = true;
            temporizador = intervaloEncendido;

            if (fuegoVisual != null)
                fuegoVisual.color = colorEncendido;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Solo hace daño si el fuego está encendido
        if (!fuegoEncendido) return;

        if (collision.CompareTag("Player"))
        {
            HealthSystem vida = collision.GetComponent<HealthSystem>();
            if (vida != null)
            {
                vida.TakeDamage(dañoPorSegundo);
            }
        }
    }
}
