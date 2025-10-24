using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlotarYRotar : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    public float amplitudFlotacion = 0.25f; // altura del movimiento vertical
    public float velocidadFlotacion = 2f;   // velocidad del movimiento vertical
    public float velocidadRotacion = 30f;   // grados por segundo

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Movimiento vertical tipo “flotación”
        float nuevaY = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);

        // Rotación lenta y continua
        transform.Rotate(0, 0, velocidadRotacion * Time.deltaTime);
    }
}
