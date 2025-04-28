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

    public void SpawnSnakes()
    {
        ClearOldSnakes(); // ✅ Step 0: clear any existing snakes

        int[,] map = mapGenerator.mapData;
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int floorID = mapGenerator.floorTileID;
        Vector2Int playerPos = mapGenerator.playerSpawnPoint;

        List<Vector2Int> validSpots = new List<Vector2Int>();

        // ✅ Step 1: collect all valid spawn positions
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

        // ✅ Step 2: randomly spawn new snakes
        int snakeCount = Mathf.Min(numberOfSnakes, validSpots.Count);

        for (int i = 0; i < snakeCount; i++)
        {
            Vector2Int spawnPos = validSpots[Random.Range(0, validSpots.Count)];
            validSpots.Remove(spawnPos); // avoid reuse
            Vector3 worldPos = new Vector3(
                spawnPos.x * tileSize + mapOffset.x,
                -spawnPos.y * tileSize + mapOffset.y,
                0f
            );

            GameObject snake = Instantiate(snakePrefab, worldPos, Quaternion.identity, transform);

            SnakeController sc = snake.GetComponent<SnakeController>();
            if (sc != null)
                sc.SaveInitialPosition();
        }

        Debug.Log($"[SnakeSpawner] Spawned {snakeCount} snakes.");
    }

    // ✅ New: Clear all previous snake GameObjects
    public void ClearOldSnakes()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
