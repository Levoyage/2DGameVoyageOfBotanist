using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public GameObject[] plantPrefabs;
    public int numberOfPlants = 10;

    public int tileSize = 1;
    public Vector2 mapOffset = Vector2.zero;

    public int spawnSafeRadius = 4; // Minimum distance from the player spawn point
    public int pimpernelMinDistance = 20; // ✅ pimpernel must be this far from player

    void Start()
    {
        SpawnPlants();
    }

    public void SpawnPlants()
    {
        ClearOldPlants(); // ✅ 清除之前生成的植物

        if (mapGenerator.mapData == null)
        {
            Debug.LogError("[PlantSpawner] Map data is null! Make sure MapGenerator ran first.");
            return;
        }

        int[,] map = mapGenerator.mapData;
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int floorID = mapGenerator.floorTileID;

        Vector2Int spawnPoint = mapGenerator.playerSpawnPoint;

        List<Vector2Int> possiblePositions = new List<Vector2Int>();

        // ✅ Step 1: collect all possible positions for plants
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == floorID)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (Vector2Int.Distance(pos, spawnPoint) > spawnSafeRadius)
                    {
                        possiblePositions.Add(pos);
                    }
                }
            }
        }

        // ✅ Step 2: ensure at least one pimpernel is spawned far from player
        GameObject pimpernelPrefab = FindPrefabByName("pimpernel");

        if (pimpernelPrefab != null)
        {
            List<Vector2Int> distantPositions = possiblePositions.FindAll(pos =>
                Vector2Int.Distance(pos, spawnPoint) >= pimpernelMinDistance
            );

            if (distantPositions.Count > 0)
            {
                Vector2Int pos = GetSafeRandomPosition(distantPositions);
                Vector3 worldPos = ToWorldPosition(pos);
                Instantiate(pimpernelPrefab, worldPos, Quaternion.identity, transform);
                possiblePositions.Remove(pos); // 🚫 avoid reuse
                Debug.Log("[PlantSpawner] Guaranteed pimpernel at " + worldPos);
            }
            else
            {
                Debug.LogWarning("[PlantSpawner] No valid distant position for pimpernel!");
            }
        }

        // ✅ Step 2.5: remove pimpernel from pool to prevent duplication
        List<GameObject> filteredPrefabs = new List<GameObject>(plantPrefabs);
        filteredPrefabs.Remove(pimpernelPrefab);

        // ✅ Step 3: spawn the rest of the plants
        int spawnCount = Mathf.Min(numberOfPlants - 1, possiblePositions.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2Int pos = GetSafeRandomPosition(possiblePositions);
            GameObject prefab = filteredPrefabs[Random.Range(0, filteredPrefabs.Count)];
            Vector3 worldPos = ToWorldPosition(pos);
            Instantiate(prefab, worldPos, Quaternion.identity, transform);
        }

        Debug.Log($"[PlantSpawner] Total plants spawned: {spawnCount + 1} (safe radius: {spawnSafeRadius}, pimpernelDist: {pimpernelMinDistance})");
    }

    // ✅ 清除旧植物对象
    public void ClearOldPlants()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private GameObject FindPrefabByName(string name)
    {
        foreach (GameObject prefab in plantPrefabs)
        {
            if (prefab.name.ToLower() == name.ToLower())
                return prefab;
        }
        Debug.LogWarning("[PlantSpawner] Prefab not found: " + name);
        return null;
    }

    private Vector2Int GetSafeRandomPosition(List<Vector2Int> available)
    {
        int index = Random.Range(0, available.Count);
        Vector2Int chosen = available[index];
        available.RemoveAt(index);
        return chosen;
    }

    private Vector3 ToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * tileSize + mapOffset.x,
            -gridPos.y * tileSize + mapOffset.y,
            0f
        );
    }
}
