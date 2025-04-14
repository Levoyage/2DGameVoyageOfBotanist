using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSpawner : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public GameObject snakePrefab;
    public int numberOfSnakes = 2;
    public int tileSize = 1;
    public Vector2 mapOffset = Vector2.zero;
    public int spawnSafeRadius = 10; // Minimum distance from the player spawn point

    void Start()
    {
        SpawnSnakes();
    }

    void SpawnSnakes()
    {
        int[,] map = mapGenerator.mapData;
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int floorID = mapGenerator.floorTileID;
        Vector2Int playerPos = mapGenerator.playerSpawnPoint;

        List<Vector2Int> validSpots = new List<Vector2Int>();

        // 1. loop through the map to find valid spawn points
        //    1.1. Check if the tile is a floor tile
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] == floorID)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (Vector2Int.Distance(pos, playerPos) > spawnSafeRadius)
                    {
                        validSpots.Add(pos);
                    }
                }
            }
        }

        if (validSpots.Count == 0)
        {
            Debug.LogWarning("[SnakeSpawner] No valid floor found to spawn snake.");
            return;
        }

        // 2. Randomly select a valid spawn point
        int snakeCount = Mathf.Min(numberOfSnakes, validSpots.Count);

        for (int i = 0; i < snakeCount; i++)
        {
            Vector2Int spawnPos = validSpots[Random.Range(0, validSpots.Count)];
            validSpots.Remove(spawnPos); // 避免重复
            Vector3 worldPos = new Vector3(
                spawnPos.x * tileSize + mapOffset.x,
                -spawnPos.y * tileSize + mapOffset.y,
                0f
            );

            GameObject snake = Instantiate(snakePrefab, worldPos, Quaternion.identity);
            SnakeController sc = snake.GetComponent<SnakeController>();
            if (sc != null)
                sc.SaveInitialPosition();
        }
    }
}
