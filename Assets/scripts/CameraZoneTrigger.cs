using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraZoneTrigger : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera playerCam;
    [SerializeField] private CinemachineVirtualCamera bossCam;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCam.Priority = 0;
            bossCam.Priority = 10; // la c�mara del boss tiene prioridad
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCam.Priority = 10;
            bossCam.Priority = 0;
        }
    }
}