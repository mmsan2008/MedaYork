using System.Collections;
using UnityEngine;

public class RayoAlternante : MonoBehaviour
{
    [Header("Configuración general")]
    [SerializeField] private GameObject rayoSuperior;
    [SerializeField] private GameObject rayoInferior;
    [SerializeField] private float duracionRayo = 1.5f;
    [SerializeField] private float tiempoEntreRayo = 1.5f;

    [Header("Advertencia visual")]
    [SerializeField] private GameObject iconoAdvertenciaSuperior;
    [SerializeField] private GameObject iconoAdvertenciaInferior;
    [SerializeField] private float duracionAdvertencia = 0.8f;

    [Header("Daño al jugador")]
    [SerializeField] private float daño = 25f;

    [Header("Efectos opcionales")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoRayo;

    private bool activandoSuperior = true;

    private void Start()
    {
        // Asegurarse de que los rayos estén apagados al inicio
        if (rayoSuperior) rayoSuperior.SetActive(false);
        if (rayoInferior) rayoInferior.SetActive(false);

        if (iconoAdvertenciaSuperior) iconoAdvertenciaSuperior.SetActive(false);
        if (iconoAdvertenciaInferior) iconoAdvertenciaInferior.SetActive(false);

        StartCoroutine(CicloRayos());
    }

    private IEnumerator CicloRayos()
    {
        while (true)
        {
            if (activandoSuperior)
            {
                yield return StartCoroutine(ActivarRayo(rayoSuperior, iconoAdvertenciaSuperior));
            }
            else
            {
                yield return StartCoroutine(ActivarRayo(rayoInferior, iconoAdvertenciaInferior));
            }

            activandoSuperior = !activandoSuperior;
            yield return new WaitForSeconds(tiempoEntreRayo);
        }
    }

    private IEnumerator ActivarRayo(GameObject rayo, GameObject icono)
    {
        // Muestra la advertencia primero
        if (icono)
        {
            icono.SetActive(true);
            yield return new WaitForSeconds(duracionAdvertencia);
            icono.SetActive(false);
        }

        // Activa el rayo eléctrico
        if (rayo)
        {
            rayo.SetActive(true);

            if (audioSource && sonidoRayo)
                audioSource.PlayOneShot(sonidoRayo);

            yield return new WaitForSeconds(duracionRayo);

            rayo.SetActive(false);
        }
    }
}
