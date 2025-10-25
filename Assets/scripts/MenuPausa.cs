using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [SerializeField] private GameObject BotonPausa;
    [SerializeField] private GameObject menuPausa;
    [SerializeField] private AudioMixer audioMixer;
    public void pausa()
    {
        Time.timeScale = 0f;
        BotonPausa.SetActive(false);
        menuPausa.SetActive(true);
    }
    public void reanudar()
    {
        Time.timeScale = 1f;
        BotonPausa.SetActive(true);
        menuPausa.SetActive(false);
    }
    public void CambiarVolumen(float volumen)
    {
        audioMixer.SetFloat("Volumen", volumen);
    }

    public void Salir()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        Time.timeScale = 1f;
    }
}
