using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Enemy : MonoBehaviour
{
    public A_healthSys A_Health;
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            A_Health.PerderVida();
        }

    }
}
