using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject birdPrefab;

    [Header("Configuración de Spawn")]
    public float minTimeBetweenSpawns = 2f;
    public float maxTimeBetweenSpawns = 5f;

    [Header("Alturas de Spawn")]
    public float alturaAlta = 3f;      // Para saltar
    public float alturaBaja = 0.5f;    // Para agacharse
    public float alturaMedia = 1.5f;   // Opcional: altura media

    [Header("Posición de Spawn")]
    public float spawnOffsetX = 2f; // Qué tan lejos del borde derecho aparecen

    [Header("Probabilidades")]
    [Range(0, 100)]
    public int probabilidadDosPajaros = 40; // 40% de que vengan 2 pájaros

    private float nextSpawnTime;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnBirds();
            ScheduleNextSpawn();
        }
    }

    private void SpawnBirds()
    {
        // Decidir si spawn 1 o 2 pájaros
        int birdCount = Random.Range(0, 100) < probabilidadDosPajaros ? 2 : 1;

        // Posición X de spawn (fuera de la pantalla a la derecha)
        float spawnX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x + spawnOffsetX;

        if (birdCount == 1)
        {
            // Spawn 1 pájaro a una altura aleatoria
            float altura = GetRandomHeight();
            SpawnBird(spawnX, altura);
        }
        else
        {
            // Spawn 2 pájaros a diferentes alturas
            float altura1 = alturaAlta;
            float altura2 = alturaBaja;

            // Alternar aleatoriamente cuál viene primero
            if (Random.value > 0.5f)
            {
                (altura1, altura2) = (altura2, altura1);
            }

            SpawnBird(spawnX, altura1);
            SpawnBird(spawnX + 1.5f, altura2); // Pequeña separación horizontal
        }
    }

    private void SpawnBird(float x, float y)
    {
        Vector3 spawnPosition = new Vector3(x, y, 0);
        Instantiate(birdPrefab, spawnPosition, Quaternion.identity);
    }

    private float GetRandomHeight()
    {
        // Elegir aleatoriamente entre altura alta, media o baja
        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                return alturaAlta;
            case 1:
                return alturaMedia;
            default:
                return alturaBaja;
        }
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns);
    }

    // Visualizar las alturas en el editor
    private void OnDrawGizmos()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        float spawnX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x + spawnOffsetX;

        // Dibujar líneas para visualizar las alturas
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(spawnX - 1, alturaAlta, 0), new Vector3(spawnX + 1, alturaAlta, 0));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(spawnX - 1, alturaMedia, 0), new Vector3(spawnX + 1, alturaMedia, 0));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(spawnX - 1, alturaBaja, 0), new Vector3(spawnX + 1, alturaBaja, 0));
    }
}