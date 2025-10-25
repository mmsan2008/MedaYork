using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatarJugadorInstantaneamente : MonoBehaviour
{
    [Header("Etiqueta del jugador")]
    public string tagJugador = "Player";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(tagJugador))
        {
            HealthSystem vida = collision.GetComponent<HealthSystem>();
            if (vida != null)
            {
                vida.InstantKill(); // Mata instantáneamente
            }
        }
    }
}