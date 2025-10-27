using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivarAlColisionar : MonoBehaviour
{
    [Header("Objeto que se activará al colisionar")]
    public GameObject objetoAActivar;

    [Header("Etiqueta del objeto que activa la colisión")]
    public string tagObjetivo = "Player";

    // Si el collider NO tiene "Is Trigger" usa este método
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tagObjetivo))
        {
            ActivarObjeto();
        }
    }

    // Si el collider SÍ tiene "Is Trigger" usa este método
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(tagObjetivo))
        {
            ActivarObjeto();
        }
    }

    private void ActivarObjeto()
    {
        if (objetoAActivar != null && !objetoAActivar.activeSelf)
        {
            objetoAActivar.SetActive(true);
        }
    }
}
