using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_HealthUIi : MonoBehaviour
{
    public GameObject[] vidas;

    public void DesactivarVida(int indice)
    {
        vidas[indice].SetActive(false);
    }
    public void ActivarVida(int indice)
    {
        vidas[indice].SetActive(true);
    }
}
