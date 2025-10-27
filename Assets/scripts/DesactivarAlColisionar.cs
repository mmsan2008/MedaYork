using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesactivarAlColisionar : MonoBehaviour
{
    [Header("Objeto que se desactivará al colisionar")]
    public GameObject objetoADesactivar;

    [Header("Etiqueta del objeto que activa la colisión")]
    public string tagObjetivo = "Player";

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tagObjetivo))
        {
            if (objetoADesactivar != null)
            {
                objetoADesactivar.SetActive(false);
            }
        }
    }

    // También puedes usar OnTriggerEnter2D si usas colliders marcados como "Is Trigger"
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(tagObjetivo))
        {
            if (objetoADesactivar != null)
            {
                objetoADesactivar.SetActive(false);
            }
        }
    }
}
