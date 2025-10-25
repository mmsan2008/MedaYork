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
            // Cambia el estado del obst�culo (como un interruptor)
            obstaculoActivo = !obstaculoActivo;

            // Activa o desactiva ambos rayos seg�n el estado
            rayoArriba.SetActive(obstaculoActivo);
            rayoAbajo.SetActive(obstaculoActivo);
        }
    }
}
