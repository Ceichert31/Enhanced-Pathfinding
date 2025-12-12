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
    public List<MapTile> toCollapse = new List<MapTile>();

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        toCollapse.Clear();
        mapGrid = new MapTile[mapSize, mapSize];

        //initialize grid with all possibilities
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                mapGrid[x, y] = connectionData.emptyTile.GetComponent<MapTile>(); // Create new instance for each cell
                mapGrid[x, y].gridPosition = new Vector2Int(x, y);
                mapGrid[x, y].tilePossibilities = new List<MapTile>(connectionData.standardSet);
                mapGrid[x, y].collapsed = false;
            }
        }
        MapTile firstTile = FindLowestEntropy();
        toCollapse.Add(firstTile);

        // Wave Function Collapse main loop
        while (toCollapse.Count > 0)
        {
            //int x = toCollapse[0].x;
            //int y = toCollapse[0].y;
            //List<MapTile> allNodes = new List<MapTile>(connectionData.standardSet);
            List<MapTile> neighbors = GetNeighbors(toCollapse[0]);
            for (int i = 0; i < neighbors.Count - 1; i++)
            {
                if (neighbors[i].collapsed == false)
                {
                    if (!toCollapse.Contains(neighbors[i])) toCollapse.Add(neighbors[i]);
                }
                else
                {
                    Constrain(toCollapse[0], neighbors[i]);
                }
            }
            if (toCollapse[0].tilePossibilities.Count < 1)
            {
                mapGrid[toCollapse[0].gridPosition.x, toCollapse[0].gridPosition.y] = connectionData.emptyTile.GetComponent<MapTile>();
                mapGrid[toCollapse[0].gridPosition.x, toCollapse[0].gridPosition.y].tilePossibilities = new List<MapTile>(connectionData.standardSet);
            }
            else
            {
                CollapseTile(toCollapse[0]);
            }
            // Find tile with lowest entropy
            /*MapTile currentTile = FindLowestEntropy();
            if (currentTile == null)
            {
                // All tiles collapsed successfully!
                break;
            }

            // Collapse the tile
            if (!CollapseTile(currentTile))
            {
                Debug.LogError("Failed to collapse tile - no valid possibilities!");
                break;
            }

            // Propagate constraints to neighbors
            if (!PropagateConstraints(currentTile))
            {
                Debug.LogError("Contradiction detected during propagation!");
                // In a full implementation, you'd backtrack here
                break;
            }*/
        }

        // Instantiate the final map
        InstantiateMap();
    }

    MapTile FindLowestEntropy()
    {
        MapTile minCell = null;
        int minEntropy = int.MaxValue;

        // Fixed: Check all cells including edges
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (mapGrid[x, y].collapsed) continue;

                int entropy = mapGrid[x, y].tilePossibilities.Count;

                if (entropy == 0)
                {
                    // Contradiction - this shouldn't happen in a working WFC
                    return null;
                }

                if (entropy < minEntropy)
                {
                    minEntropy = entropy;
                    minCell = mapGrid[x, y];
                }
            }
        }

        return minCell;
    }

    void ReducePossibilities(List<MapTile> potential, List<MapTile> limitations)
    {
        for (int i = 0; i < potential.Count - 1; i++)
        {
            if (!limitations.Contains(potential[i]))
            {
                potential.Remove(potential[i]);
            }
        }
    }
    
    void Constrain(MapTile target, MapTile neighbor)
    {
        if (neighbor.gridPosition.x > target.gridPosition.x) // neighbor to the right
        {
            ReducePossibilities(target.tilePossibilities, neighbor.leftCompatible);
        }
        if (neighbor.gridPosition.x < target.gridPosition.x) // neighbor to the left
        {
            ReducePossibilities(target.tilePossibilities, neighbor.rightCompatible);
        }
        if (neighbor.gridPosition.y > target.gridPosition.y) // neighbor above
        {
            ReducePossibilities(target.tilePossibilities, neighbor.backCompatible);
        }
        if(neighbor.gridPosition.y < target.gridPosition.y) // neighbor below
        {
            ReducePossibilities(target.tilePossibilities, neighbor.frontCompatible);
        }
    }

    bool CollapseTile(MapTile tile)
    {
        if (tile.tilePossibilities.Count == 0)
        {
            return false;
        }

        // Fixed: Use correct Random.Range (upper bound is exclusive)
        int tileIndex = Random.Range(0, tile.tilePossibilities.Count);
        var position = tile.gridPosition;
        tile.collapsedTile = tile.tilePossibilities[tileIndex];
        tile.collapsed = true;
        tile.gridPosition = position;
        tile.tilePossibilities.Clear();
        tile.tilePossibilities.Add(tile.collapsedTile);

        return true;
    }

    bool PropagateConstraints(MapTile tile)
    {
        // Use a queue for propagation (breadth-first)
        Queue<MapTile> propagationQueue = new Queue<MapTile>();
        propagationQueue.Enqueue(tile);

        while (propagationQueue.Count > 0)
        {
            MapTile current = propagationQueue.Dequeue();
            List<MapTile> neighbors = GetNeighbors(current);

            foreach (MapTile neighbor in neighbors)
            {
                if (neighbor.collapsed) continue;

                // Get valid possibilities for this neighbor based on all its neighbors
                List<MapTile> validPossibilities = GetValidPossibilities(neighbor);

                if (validPossibilities.Count == 0)
                {
                    // Contradiction detected
                    return false;
                }

                // If possibilities were reduced, add to queue for further propagation
                if (validPossibilities.Count < neighbor.tilePossibilities.Count)
                {
                    neighbor.tilePossibilities = validPossibilities;
                    propagationQueue.Enqueue(neighbor);
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

        // Check each direction
        if (x > 0) // Left neighbor
        {
            MapTile leftNeighbor = mapGrid[x - 1, y];
            if (leftNeighbor.collapsed)
            {
                validTiles = FilterByConnection(validTiles, leftNeighbor.collapsedTile.rightConnections, Direction.Left);
            }
        }

        if (x < mapSize - 1) // Right neighbor
        {
            MapTile rightNeighbor = mapGrid[x + 1, y];
            if (rightNeighbor.collapsed)
            {
                validTiles = FilterByConnection(validTiles, rightNeighbor.collapsedTile.leftConnections, Direction.Right);
            }
        }

        if (y > 0) // Down neighbor (assuming Y+ is up)
        {
            MapTile downNeighbor = mapGrid[x, y - 1];
            if (downNeighbor.collapsed)
            {
                validTiles = FilterByConnection(validTiles, downNeighbor.collapsedTile.frontConnections, Direction.Down);
            }
        }

        if (y < mapSize - 1) // Up neighbor
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

            // Check if this tile can connect to the neighbor
            foreach (int connection in connectionsToCheck)
            {
                if (allowedConnections.Contains(connection))
                {
                    filtered.Add(possibility);
                    break;
                }
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
                        p.GetComponent<MapTile>() == tile.collapsedTile);

                    if (prefab != null)
                    {
                        Instantiate(prefab,
                            new Vector3(x * tileSize, 0, y * tileSize),
                            Quaternion.identity);
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