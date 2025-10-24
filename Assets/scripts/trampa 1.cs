using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenduloConEncendido : MonoBehaviour
{
    [Header("Movimiento del péndulo")]
    public float anguloMaximo = 45f;
    public float velocidad = 2f;

    [Header("Encendido / Apagado")]
    public float tiempoEncendido = 2f;  // duración del estado activo
    public float tiempoApagado = 2f;    // duración del estado inactivo
    public Color colorEncendido = Color.red;
    public Color colorApagado = Color.gray;

    private bool estaEncendido = false;
    private SpriteRenderer spriteRenderer;
    private float temporizador;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        temporizador = tiempoApagado; // empieza apagado
        spriteRenderer.color = colorApagado;
    }

    void Update()
    {
        // Movimiento pendular
        float angulo = anguloMaximo * Mathf.Sin(Time.time * velocidad);
        transform.localRotation = Quaternion.Euler(0, 0, angulo);

        // Cambiar entre encendido/apagado
        temporizador -= Time.deltaTime;
        if (temporizador <= 0f)
        {
            estaEncendido = !estaEncendido;
            temporizador = estaEncendido ? tiempoEncendido : tiempoApagado;
            spriteRenderer.color = estaEncendido ? colorEncendido : colorApagado;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (estaEncendido && other.CompareTag("Player"))
        {
            other.GetComponent<HealthSystem>()?.TakeDamage(10);
        }
    }
}
