
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MapGenerator))]
public class PlantSpawner : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public GameObject[] plantPrefabs;
    public int numberOfPlants = 10;

    public int tileSize = 1;
    public Vector2 mapOffset = Vector2.zero;

    public int spawnSafeRadius = 4;    // Min distance from player spawn
    public int distantMinDistance = 20; // Min distance for forced spawns

    void Start()
    {
        if (mapGenerator == null)
            mapGenerator = GetComponent<MapGenerator>() ?? FindObjectOfType<MapGenerator>();

        if (mapGenerator == null || mapGenerator.mapData == null)
        {
            Debug.LogError("[PlantSpawner] Missing MapGenerator or mapData is null.");
            return;
        }

        SpawnPlants();
    }

    public void SpawnPlants()
    {
        ClearOldPlants();

        int[,] map = mapGenerator.mapData;
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int floorID = mapGenerator.floorTileID;
        Vector2Int spawnPoint = mapGenerator.playerSpawnPoint;

        // Step 1: gather possible positions
        List<Vector2Int> possiblePositions = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (map[x, y] == floorID)
                {
                    var pos = new Vector2Int(x, y);
                    if (Vector2Int.Distance(pos, spawnPoint) > spawnSafeRadius)
                        possiblePositions.Add(pos);
                }

        // Step 2: forced spawn based on scene
        string sceneName = SceneManager.GetActiveScene().name;
        int forcedCount = 0;

        if (sceneName.Equals("FieldScene", System.StringComparison.OrdinalIgnoreCase))
        {
            forcedCount += ForceSpawn("pimpernel", spawnPoint, possiblePositions);
        }
        else if (sceneName.Equals("FieldScene-1", System.StringComparison.OrdinalIgnoreCase))
        {
            // Force spawn foxglove
            Vector2Int foxPos = Vector2Int.zero;
            var distantFox = possiblePositions.FindAll(p => Vector2Int.Distance(p, spawnPoint) >= distantMinDistance);
            if (distantFox.Count > 0)
            {
                foxPos = distantFox[Random.Range(0, distantFox.Count)];
                Instantiate(FindPrefabByName("foxglove"), ToWorldPosition(foxPos), Quaternion.identity, transform);
                possiblePositions.Remove(foxPos);
                forcedCount++;
                Debug.Log($"[PlantSpawner] Forced spawn foxglove at {foxPos}");
            }

            // Force spawn ginger at least 2 tiles from foxglove
            var distantGinger = possiblePositions.FindAll(p =>
                Vector2Int.Distance(p, spawnPoint) >= distantMinDistance &&
                Vector2Int.Distance(p, foxPos) >= 2f
            );
            Vector2Int gingerPos;
            if (distantGinger.Count > 0)
                gingerPos = distantGinger[Random.Range(0, distantGinger.Count)];
            else
            {
                // Fallback: any distant from player
                var fallback = possiblePositions.FindAll(p => Vector2Int.Distance(p, spawnPoint) >= distantMinDistance);
                gingerPos = fallback.Count > 0 ? fallback[Random.Range(0, fallback.Count)] : possiblePositions[Random.Range(0, possiblePositions.Count)];
            }
            Instantiate(FindPrefabByName("ginger"), ToWorldPosition(gingerPos), Quaternion.identity, transform);
            possiblePositions.Remove(gingerPos);
            forcedCount++;
            Debug.Log($"[PlantSpawner] Forced spawn ginger at {gingerPos}");
        }

        // Step 3: spawn remaining plants
        List<GameObject> filtered = new List<GameObject>(plantPrefabs);
        filtered.RemoveAll(p =>
            p.name.Equals("pimpernel", System.StringComparison.OrdinalIgnoreCase) ||
            p.name.Equals("foxglove", System.StringComparison.OrdinalIgnoreCase) ||
            p.name.Equals("ginger", System.StringComparison.OrdinalIgnoreCase)
        );

        int spawnCount = Mathf.Min(numberOfPlants - forcedCount, possiblePositions.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2Int gridPos = GetSafeRandomPosition(possiblePositions);
            Vector3 worldPos = ToWorldPosition(gridPos);

            // Skip if obstacle present
            if (Physics.CheckSphere(worldPos + Vector3.up * 0.5f, 0.4f, LayerMask.GetMask("Obstacle")))
                continue;

            GameObject prefab = filtered[Random.Range(0, filtered.Count)];
            Instantiate(prefab, worldPos, Quaternion.identity, transform);
        }

        Debug.Log($"[PlantSpawner] Spawned {spawnCount + forcedCount} plants. ({spawnCount} random + {forcedCount} forced)");
    }

    int ForceSpawn(string name, Vector2Int spawnPoint, List<Vector2Int> available)
    {
        GameObject prefab = FindPrefabByName(name);
        if (prefab == null || available.Count == 0)
            return 0;

        var distant = available.FindAll(pos => Vector2Int.Distance(pos, spawnPoint) >= distantMinDistance);
        foreach (var pos in distant)
        {
            var worldPos = ToWorldPosition(pos);
            if (!Physics.CheckSphere(worldPos + Vector3.up * 0.5f, 0.4f, LayerMask.GetMask("Obstacle")))
            {
                Instantiate(prefab, worldPos, Quaternion.identity, transform);
                available.Remove(pos);
                return 1;
            }
        }
        return 0;
    }

    public void ClearOldPlants()
    {
        foreach (Transform t in transform)
            Destroy(t.gameObject);
    }

    GameObject FindPrefabByName(string name)
    {
        foreach (var p in plantPrefabs)
            if (p.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return p;
        Debug.LogWarning($"[PlantSpawner] Prefab not found: {name}");
        return null;
    }

    Vector2Int GetSafeRandomPosition(List<Vector2Int> list)
    {
        int i = Random.Range(0, list.Count);
        var pos = list[i];
        list.RemoveAt(i);
        return pos;
    }

    Vector3 ToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * tileSize + mapOffset.x,
            -gridPos.y * tileSize + mapOffset.y,
            0f
        );
    }
}

