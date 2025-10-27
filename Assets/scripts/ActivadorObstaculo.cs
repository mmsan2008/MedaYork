using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivadorObstaculo : MonoBehaviour
{
    public GameObject rayoArriba;
    public GameObject rayoAbajo;

    private bool obstaculoActivo = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Cambia el estado del obstáculo (como un interruptor)
            obstaculoActivo = !obstaculoActivo;

            // Activa o desactiva ambos rayos según el estado
            rayoArriba.SetActive(obstaculoActivo);
            rayoAbajo.SetActive(obstaculoActivo);
        }
    }
}
