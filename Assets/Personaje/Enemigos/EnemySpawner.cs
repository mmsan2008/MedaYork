using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;       // Prefab del enemigo
    public Transform player;             // Referencia al jugador
    public float spawnDistance = 10f;    // Qué tan lejos aparecen de cada lado
    public float spawnHeight = 0f;       // Altura fija del spawn
    public float spawnInterval = 2f;     // Tiempo entre spawns
    public Camera mainCamera;            // Cámara principal

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        // Cada cierto tiempo, spawnea un enemigo
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0;
        }
    }

    void SpawnEnemy()
    {
        // Decidimos si el enemigo aparece a la derecha o izquierda
        int side = Random.Range(0, 2);
        Vector3 spawnPos;

        if (side == 0)
            spawnPos = new Vector3(player.position.x - spawnDistance, spawnHeight, 0); // izquierda
        else
            spawnPos = new Vector3(player.position.x + spawnDistance, spawnHeight, 0); // derecha

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Asigna el movimiento hacia el jugador
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.SetDirection(side == 0 ? Vector2.right : Vector2.left);
            enemyMovement.mainCamera = mainCamera;
        }
    }
}
