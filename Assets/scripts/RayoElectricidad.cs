using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayoElectricidad : MonoBehaviour
{
    [SerializeField] private float da�o = 25f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthSystem vida = other.GetComponent<HealthSystem>();
            if (vida != null)
            {
                vida.TakeDamage(da�o, transform);
            }
        }
    }
}
