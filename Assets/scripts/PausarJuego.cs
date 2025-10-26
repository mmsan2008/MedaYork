using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PausarJuego : MonoBehaviour
{
    public GameObject MenuPausa;
    public bool JuegoPausado = false;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (JuegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Reanudar()
    {
        MenuPausa.SetActive(false);
        Time.timeScale = 1;
        JuegoPausado = false;
    }

    public void Pause()
    {
        MenuPausa.SetActive(true);
        Time.timeScale = 0;
        JuegoPausado=true;
    }


}
