using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorEnemigo : MonoBehaviour
{
    public Transform jugador;
    public float radiodedeteccion = 5.0f;
    public float velocidad = 2.0f;

    private Rigidbody2D rb2d;
    private Vector2 movimiento;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float distanciajugador = Vector2.Distance(transform.position, jugador.position);

        if (distanciajugador < radiodedeteccion)
        {

            Vector2 direccion = (jugador.position - transform.position).normalized;

            movimiento = new Vector2(direccion.x, 0);


        }
        else
        {
            movimiento = Vector2.zero;
        }
        
        rb2d.MovePosition(rb2d.position + movimiento * velocidad * Time.deltaTime);
        




    }
}
