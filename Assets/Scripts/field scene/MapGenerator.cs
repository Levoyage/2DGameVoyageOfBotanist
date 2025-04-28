using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum MapTheme { Forest, Mountain, Canyon }
    public MapTheme theme = MapTheme.Forest;

    public int width = 70;
    public int height = 70;
    public int iterations = 5;

    [Range(0f, 1f)]
    public float initialWallChance = 0.3f;

    public int floorTileID = 0;
    public int treeTileID = 4;

    public int wallThreshold = 3;

    public int[,] mapData;
    public Vector2Int playerSpawnPoint = new Vector2Int(-1, -1);

    private int maxRetries = 20;
    private int retryCount = 0;

    void Start()
    {
        GenerateMap();
    }


    // ... 你的 using 和 class 保持不变

    public void GenerateMap()
    {
        retryCount++;
        if (retryCount > maxRetries)
        {
            Debug.LogError("Map generation failed too many times. Check your settings!");
            return;
        }

        bool[,] tiles = new bool[width, height];
        SetInitialWallDensity(theme, tiles);

        for (int i = 0; i < iterations; i++)
        {
            tiles = SmoothMap(tiles);
        }

        playerSpawnPoint = FindValidSpawnPoint(tiles, 3);

        mapData = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mapData[x, y] = tiles[x, y] ? treeTileID : floorTileID;
            }
        }

        int safeRadius = 2;
        for (int dx = -safeRadius; dx <= safeRadius; dx++)
        {
            for (int dy = -safeRadius; dy <= safeRadius; dy++)
            {
                int px = playerSpawnPoint.x + dx;
                int py = playerSpawnPoint.y + dy;
                if (IsInBounds(px, py))
                {
                    mapData[px, py] = floorTileID;
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            mapData[x, 0] = treeTileID;
            mapData[x, height - 1] = treeTileID;
        }
        for (int y = 0; y < height; y++)
        {
            mapData[0, y] = treeTileID;
            mapData[width - 1, y] = treeTileID;
        }

        HashSet<Vector2Int> connectedFloors = FloodFillFrom(playerSpawnPoint);
        int totalFloors = CountAllFloorTiles();
        float ratio = connectedFloors.Count / (float)totalFloors;

        Debug.Log($"[FloodFill] Reachable tiles: {connectedFloors.Count} / {totalFloors} ({ratio * 100:F1}%)");

        if (ratio < 0.88f)
        {
            Debug.LogWarning("[MapGen] Connectivity too low, regenerating map...");
            GenerateMap();
            return;
        }

        // ✅ Step 8: Find largest connected region and force player spawn there
        HashSet<Vector2Int> mainRegion = GetLargestConnectedRegion();
        List<Vector2Int> candidates = new List<Vector2Int>();

        foreach (Vector2Int pos in mainRegion)
        {
            bool safe = true;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = pos.x + dx;
                    int ny = pos.y + dy;
                    if (!IsInBounds(nx, ny) || mapData[nx, ny] != floorTileID)
                    {
                        safe = false;
                        break;
                    }
                }
                if (!safe) break;
            }

            if (safe)
                candidates.Add(pos);
        }

        if (candidates.Count > 0)
        {
            playerSpawnPoint = candidates[Random.Range(0, candidates.Count)];
            Debug.Log($"[Spawn] Forced spawn in main region at {playerSpawnPoint}");
        }
        else
        {
            Debug.LogWarning("[Spawn] No valid safe tile in main region. Fallback to center.");
            playerSpawnPoint = new Vector2Int(width / 2, height / 2);
        }

        // ✅ Step 9: Remove isolated floor regions
        foreach (Vector2Int pos in AllFloorPositions())
        {
            if (!mainRegion.Contains(pos))
            {
                mapData[pos.x, pos.y] = treeTileID;
            }
        }

        // ✅ Step 10: Final spawn safety check — ensure spawn has sufficient open space
        if (!IsSpawnPointTrulyFree(playerSpawnPoint, 100))
        {
            Debug.LogWarning("[SpawnCheck] Spawn area too constrained — regenerating map...");
            GenerateMap();
            return;
        }

        retryCount = 0;
        Debug.Log($"Spawn point tileID = {mapData[playerSpawnPoint.x, playerSpawnPoint.y]}");
    }

    // Check if spawn point is truly free
    private bool IsSpawnPointTrulyFree(Vector2Int spawnPoint, int minReachableTiles = 100)
    {
        HashSet<Vector2Int> reachable = FloodFillFrom(spawnPoint);
        return reachable.Count >= minReachableTiles;
    }


    private HashSet<Vector2Int> GetLargestConnectedRegion()
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        HashSet<Vector2Int> largest = new HashSet<Vector2Int>();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                Vector2Int start = new Vector2Int(x, y);
                if (mapData[x, y] != floorTileID || visited.Contains(start))
                    continue;

                HashSet<Vector2Int> region = new HashSet<Vector2Int>();
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                queue.Enqueue(start);
                region.Add(start);

                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();
                    visited.Add(current);

                    foreach (Vector2Int dir in new Vector2Int[] {
                        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
                    })
                    {
                        Vector2Int next = current + dir;
                        if (!IsInBounds(next.x, next.y)) continue;
                        if (mapData[next.x, next.y] != floorTileID) continue;
                        if (visited.Contains(next)) continue;

                        region.Add(next);
                        queue.Enqueue(next);
                        visited.Add(next);
                    }
                }

                if (region.Count > largest.Count)
                {
                    largest = region;
                }
            }
        }

        return largest;
    }

    private void SetInitialWallDensity(MapTheme theme, bool[,] tiles)
    {
        switch (theme)
        {
            case MapTheme.Forest:
                initialWallChance = 0.60f;
                break;
            case MapTheme.Mountain:
                initialWallChance = 0.45f;
                break;
            case MapTheme.Canyon:
                initialWallChance = 0.25f;
                break;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = Random.value < initialWallChance;
            }
        }
    }

    private bool[,] SmoothMap(bool[,] tiles)
    {
        bool[,] newTiles = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int wallCount = GetWallNeighborCount(tiles, x, y);
                newTiles[x, y] = wallCount >= wallThreshold;
            }
        }

        return newTiles;
    }

    private int GetWallNeighborCount(bool[,] tiles, int x, int y)
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (IsInBounds(nx, ny))
                {
                    if (tiles[nx, ny]) count++;
                }
                else
                {
                    count++;
                }
            }
        }

        return count;
    }

    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private Vector2Int FindValidSpawnPoint(bool[,] tiles, int requiredOpenNeighbors)
    {
        for (int x = 8; x < width - 8; x++)
        {
            for (int y = 8; y < height - 8; y++)
            {
                if (tiles[x, y]) continue;

                bool safe = true;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (!IsInBounds(nx, ny) || tiles[nx, ny])
                        {
                            safe = false;
                            break;
                        }
                    }
                    if (!safe) break;
                }

                if (!safe) continue;

                int openCount = 0;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        int nx = x + dx;
                        int ny = y + dy;

                        if (IsInBounds(nx, ny) && !tiles[nx, ny])
                            openCount++;
                    }
                }

                if (openCount >= requiredOpenNeighbors)
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(width / 2, height / 2);
    }

    private int CountAllFloorTiles()
    {
        int count = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapData[x, y] == floorTileID)
                    count++;
            }
        }
        return count;
    }

    private HashSet<Vector2Int> FloodFillFrom(Vector2Int start)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int dir in new Vector2Int[] {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            })
            {
                Vector2Int neighbor = current + dir;
                if (!IsInBounds(neighbor.x, neighbor.y)) continue;
                if (visited.Contains(neighbor)) continue;
                if (mapData[neighbor.x, neighbor.y] != floorTileID) continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return visited;
    }

    private List<Vector2Int> AllFloorPositions()
    {
        List<Vector2Int> floors = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapData[x, y] == floorTileID)
                    floors.Add(new Vector2Int(x, y));
            }
        }
        return floors;
    }
}
