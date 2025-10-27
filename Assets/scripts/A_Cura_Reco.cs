using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Cura_Reco : MonoBehaviour
{
    public A_healthSys A_Health;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            bool VidaRecuperada = A_Health.RecuperarVida();
            if (VidaRecuperada)
            {
                Destroy(this.gameObject); 
            }
        }
    }
}
