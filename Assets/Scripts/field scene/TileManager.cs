using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public MapGenerator generator; // Reference to the procedural map generator
    public GameObject[] tilePrefabs; // Tile prefab array indexed by tile ID
    public int tileSize = 1; // Size of each tile in world units
    public Vector2 mapOffset = new Vector2(0, 0); // Position offset of the map
    public Transform player; // Reference to the player for view culling
    public int screenTileWidth = 10; // Number of tiles to render horizontally
    public int screenTileHeight = 6; // Number of tiles to render vertically

    public bool[] solidTiles; // Indicates which tile IDs are solid
    public int[,] mapData; // 2D map data array from the generator
    private GameObject[,] tileInstances; // Stores tile GameObject instances

    void Awake()
    {
        // Initialize the solidTiles array based on the number of tile prefabs
        solidTiles = new bool[tilePrefabs.Length];

        // Example: manually set which tile IDs are solid
        solidTiles[0] = false;
        solidTiles[2] = false;
        solidTiles[4] = true; // tree
        // Other tile types default to non-solid
    }

    void Start()
    {
        mapData = generator.mapData;

        InitializeTileInstances();
        UpdateVisibleTiles();
    }

    void Update()
    {
        Transform player = GameManager.Instance.PlayerTransform;
        // 自动尝试重新连接 Player 引用
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                // 没找到，跳过更新
                return;
            }
        }

        UpdateVisibleTiles();
    }


    // Instantiate tiles based on mapData but deactivate all initially
    void InitializeTileInstances()
    {
        int mapWidth = mapData.GetLength(0);
        int mapHeight = mapData.GetLength(1);

        tileInstances = new GameObject[mapWidth, mapHeight];

        for (int row = 0; row < mapHeight; row++)
        {
            for (int col = 0; col < mapWidth; col++)
            {
                int tileID = mapData[col, row];

                if (tileID >= 0 && tileID < tilePrefabs.Length)
                {
                    Vector3 position = new Vector3(col * tileSize + mapOffset.x, -row * tileSize + mapOffset.y, 0);
                    tileInstances[col, row] = Instantiate(tilePrefabs[tileID], position, Quaternion.identity, transform);

                    // If the tile is marked as solid, add a BoxCollider2D
                    if (solidTiles.Length > tileID && solidTiles[tileID])
                    {
                        BoxCollider2D collider = tileInstances[col, row].AddComponent<BoxCollider2D>();
                        collider.isTrigger = false;
                    }

                    tileInstances[col, row].SetActive(false); // Initially hidden
                }
            }
        }
    }

    // Update visibility of tiles based on the player's current position
    void UpdateVisibleTiles()
    {


        int playerTileX = Mathf.RoundToInt((player.position.x - mapOffset.x) / tileSize);
        int playerTileY = Mathf.RoundToInt((player.position.y - mapOffset.y) / -tileSize);

        for (int row = 0; row < mapData.GetLength(1); row++)
        {
            for (int col = 0; col < mapData.GetLength(0); col++)
            {
                bool inView = true;
                if (tileInstances[col, row] != null)
                {
                    tileInstances[col, row].SetActive(inView);
                }
            }
        }
    }

    // ✅ NEW: Refresh tilemap display after map regeneration
    public void RefreshTiles()
    {
        mapData = generator.mapData;

        // Remove all old tiles
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Re-initialize with new map
        InitializeTileInstances();
        UpdateVisibleTiles();
    }
}
