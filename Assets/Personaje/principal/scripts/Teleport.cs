using UnityEngine;

public class Teleport : MonoBehaviour
{
    [Header("Destino del Teleport")]
    public Transform puntoDestino; // Aqu� arrastras el objeto destino en el inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Aseg�rate de que tu jugador tenga el tag "Player"
        {
            other.transform.position = puntoDestino.position; // Cambia la posici�n del jugador
        }
    }
}
