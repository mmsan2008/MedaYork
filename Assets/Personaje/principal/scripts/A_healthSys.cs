using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_healthSys : MonoBehaviour
{
    private int health = 10;
    public A_HealthUIi A_Health;
    // Update is called once per frame
    public void PerderVida()
    {
        health -= 1;
        A_Health.DesactivarVida(health);
    }
    public bool RecuperarVida()
    {
        if (health == 10)
        {
            return false;
        }
       A_Health.ActivarVida(health);
        health += 1;
        return true;
    }
}
