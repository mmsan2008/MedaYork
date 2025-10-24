using UnityEngine;

public class Teleport : MonoBehaviour
{
    [Header("Destino del Teleport")]
    public Transform puntoDestino; // Aquí arrastras el objeto destino en el inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Asegúrate de que tu jugador tenga el tag "Player"
        {
            other.transform.position = puntoDestino.position; // Cambia la posición del jugador
        }
    }
}
