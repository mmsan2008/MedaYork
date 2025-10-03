using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Movimiento : MonoBehaviour
{
    public Controles controles;

    public Vector2 direccion;

    public Rigidbody2D rb2d;

    public float velocidadmovimento;

    public bool mirandoderecha = true;

    public float fuerzasalto;

    public LayerMask queEsSuelo;
    public Transform controladorSuelo;
    public Vector2 dimensionesCaja;
    public bool enSuelo;

    public Animator animator;




    private void Awake()
    {
        controles = new();

    }

    private void OnEnable()
    {
        controles.Enable();
        controles.Movimento.Saltar.started += _ => Saltar();
    }

    private void OnDisable()
    {
        controles.Disable();
        controles.Movimento.Saltar.started -= _ => Saltar();
    }

    private void Update()
    {
        direccion = controles.Movimento.Mover.ReadValue<Vector2>();

        AjustarRotacion(direccion.x);

        enSuelo = Physics2D.OverlapBox(controladorSuelo.position, dimensionesCaja, 0f, queEsSuelo);

    }

    private void FixedUpdate()
    {

        rb2d.velocity = new Vector2(direccion.x * velocidadmovimento, rb2d.velocity.y);


        animator.SetFloat("movimiento", direccion.x);

    }

    private void AjustarRotacion(float direccionX)
    {
        if (direccionX > 0 && !mirandoderecha)
        {
            Girar();
        }
        else if (direccionX < 0 && mirandoderecha)
        {
            Girar();
        }
    }

    private void Girar()
    {
        mirandoderecha = !mirandoderecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    private void Saltar()
    {
        if (enSuelo)
        {
            rb2d.AddForce(new Vector2(0, fuerzasalto), ForceMode2D.Impulse);
        }
        

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(controladorSuelo.position, dimensionesCaja);
    }


}
