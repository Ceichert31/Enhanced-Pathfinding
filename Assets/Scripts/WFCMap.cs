using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCMap : MonoBehaviour
{
    public int tileSize;
    public int mapSize;
    [SerializeField] ConnectionManager connectionManager;
    [SerializeField] ConnectionData connectionData;

    public MapTile[,] mapGrid;
    private Transform gridParent;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        gridParent = new GameObject("GridCells").transform;
        gridParent.parent = transform;

        mapGrid = new MapTile[mapSize, mapSize];

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                GameObject tileObj = new GameObject($"Cell_{x}_{y}");
                tileObj.transform.parent = gridParent;

                MapTile tile = tileObj.AddComponent<MapTile>();
                tile.gridPosition = new Vector2(x, y);
                tile.tilePossibilities = new List<MapTile>(connectionData.standardSet);
                tile.collapsed = false;

                mapGrid[x, y] = tile;
            }
        }

        int iterations = 0;
        int maxIterations = mapSize * mapSize * 10;

        while (iterations < maxIterations)
        {
            iterations++;

            MapTile currentTile = FindLowestEntropy();
            if (currentTile == null)
            {
                // All tiles collapsed successfully!
                break;
            }

            if (!CollapseTile(currentTile))
            {
                Debug.LogError("Failed to collapse tile - no valid possibilities!");
                break;
            }

            if (!PropagateConstraints(currentTile))
            {
                Debug.LogError($"Contradiction detected at ({currentTile.gridPosition.x}, {currentTile.gridPosition.y})!");
                break;
            }
        }

        if (iterations >= maxIterations)
        {
            Debug.LogError("Timeout: WFC");
        }

        Destroy(gridParent.gameObject);

        InstantiateMap();
    }

    MapTile FindLowestEntropy()
    {
        MapTile minCell = null;
        int minEntropy = int.MaxValue;
        List<MapTile> tilesWithMinEntropy = new List<MapTile>();

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (mapGrid[x, y].collapsed) continue;

                int entropy = mapGrid[x, y].tilePossibilities.Count;

                if (entropy == 0)
                {
                    return null;
                }

                if (entropy < minEntropy)
                {
                    minEntropy = entropy;
                    tilesWithMinEntropy.Clear();
                    tilesWithMinEntropy.Add(mapGrid[x, y]);
                }
                else if (entropy == minEntropy)
                {
                    tilesWithMinEntropy.Add(mapGrid[x, y]);
                }
            }
        }

        if (tilesWithMinEntropy.Count > 0)
        {
            return tilesWithMinEntropy[Random.Range(0, tilesWithMinEntropy.Count)];
        }

        return minCell;
    }

    bool CollapseTile(MapTile tile)
    {
        if (tile.tilePossibilities.Count == 0)
        {
            return false;
        }

        int tileIndex = Random.Range(0, tile.tilePossibilities.Count);

        tile.collapsedTile = tile.tilePossibilities[tileIndex];
        tile.collapsed = true;

        tile.tilePossibilities.Add(tile.collapsedTile);

        return true;
    }

    bool PropagateConstraints(MapTile tile)
    {
        Queue<MapTile> propagationQueue = new Queue<MapTile>();
        HashSet<MapTile> inQueue = new HashSet<MapTile>();

        propagationQueue.Enqueue(tile);
        inQueue.Add(tile);

        while (propagationQueue.Count > 0)
        {
            MapTile current = propagationQueue.Dequeue();
            inQueue.Remove(current);

            List<MapTile> neighbors = GetNeighbors(current);

            foreach (MapTile neighbor in neighbors)
            {
                if (neighbor.collapsed) continue;

                int oldCount = neighbor.tilePossibilities.Count;

                List<MapTile> validPossibilities = GetValidPossibilities(neighbor);

                if (validPossibilities.Count == 0)
                {
                    //Contradiction detected
                    return false;
                }

                if (validPossibilities.Count < oldCount)
                {
                    neighbor.tilePossibilities = validPossibilities;

                    if (!inQueue.Contains(neighbor))
                    {
                        propagationQueue.Enqueue(neighbor);
                        inQueue.Add(neighbor);
                    }
                }
            }
        }

        return true;
    }

    List<MapTile> GetValidPossibilities(MapTile tile)
    {
        List<MapTile> validTiles = new List<MapTile>(tile.tilePossibilities);

        int x = (int)tile.gridPosition.x;
        int y = (int)tile.gridPosition.y;

        if (x > 0)
        {
            MapTile leftNeighbor = mapGrid[x - 1, y];
            if (leftNeighbor.collapsed)
            {
                validTiles = FilterByConnection(validTiles, leftNeighbor.collapsedTile.rightConnections, Direction.Left);
            }
        }

        if (x < mapSize - 1)
        {
            MapTile rightNeighbor = mapGrid[x + 1, y];
            if (rightNeighbor.collapsed)
            {
                validTiles = FilterByConnection(validTiles, rightNeighbor.collapsedTile.leftConnections, Direction.Right);
            }
        }

        if (y > 0)
        {
            MapTile downNeighbor = mapGrid[x, y - 1];
            if (downNeighbor.collapsed)
            {
                validTiles = FilterByConnection(validTiles, downNeighbor.collapsedTile.frontConnections, Direction.Down);
            }
        }

        if (y < mapSize - 1)
        {
            MapTile upNeighbor = mapGrid[x, y + 1];
            if (upNeighbor.collapsed)
            {
                validTiles = FilterByConnection(validTiles, upNeighbor.collapsedTile.backConnections, Direction.Up);
            }
        }

        return validTiles;
    }

    List<MapTile> FilterByConnection(List<MapTile> possibilities, List<int> allowedConnections, Direction direction)
    {
        List<MapTile> filtered = new List<MapTile>();

        foreach (MapTile possibility in possibilities)
        {
            List<int> connectionsToCheck = direction switch
            {
                Direction.Left => possibility.leftConnections,
                Direction.Right => possibility.rightConnections,
                Direction.Up => possibility.frontConnections,
                Direction.Down => possibility.backConnections,
                _ => new List<int>()
            };

            bool canConnect = false;
            foreach (int connection in connectionsToCheck)
            {
                if (allowedConnections.Contains(connection))
                {
                    canConnect = true;
                    break;
                }
            }

            if (canConnect)
            {
                filtered.Add(possibility);
            }
        }

        return filtered;
    }

    List<MapTile> GetNeighbors(MapTile tile)
    {
        List<MapTile> neighbors = new List<MapTile>();
        int x = (int)tile.gridPosition.x;
        int y = (int)tile.gridPosition.y;

        if (x > 0)
            neighbors.Add(mapGrid[x - 1, y]);
        if (x < mapSize - 1)
            neighbors.Add(mapGrid[x + 1, y]);
        if (y > 0)
            neighbors.Add(mapGrid[x, y - 1]);
        if (y < mapSize - 1)
            neighbors.Add(mapGrid[x, y + 1]);

        return neighbors;
    }

    void InstantiateMap()
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                MapTile tile = mapGrid[x, y];
                if (tile.collapsed && tile.collapsedTile != null)
                {
                    GameObject prefab = connectionData.mapTilePrefabs.Find(p =>
                        p.GetComponent<MapTile>().tileID == tile.collapsedTile.tileID);

                    if (prefab != null)
                    {
                        Instantiate(prefab,
                            new Vector3(x * tileSize, prefab.transform.position.y, y * tileSize),
                            Quaternion.Inverse(prefab.transform.rotation),
                            transform);
                    }
                }
            }
        }
    }

    enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }
}