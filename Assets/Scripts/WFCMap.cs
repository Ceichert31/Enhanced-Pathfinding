using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class WFCMap : MonoBehaviour
{
    public int tileSize; //dimension of a tile, given that the tile is square
    public int mapSize; //dimension of the map, for a map of size (mapsize X mapsize)
    [SerializeField] ConnectionManager connectionManager;
    [SerializeField] ConnectionData connectionData;

    public MapTile[,] mapGrid; //grid of tiles, coordinates (x,y)


    // Start is called before the first frame update
    void Start()
    {
        mapGrid = new MapTile[mapSize, mapSize];
        
        //initialize grid with all possibilities
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {   
                mapGrid[x, y] = connectionData.emptyTile.GetComponent<MapTile>();
                mapGrid[x, y].gridPosition = new Vector2(x, y);
                mapGrid[x, y].tilePossibilities = connectionData.standardSet;
            }
        }
        //collapse tile to start
        Stack<MapTile> tileStack = new();
        while (true)
        {
            MapTile currentTile = findLowestEntropy(mapGrid);
            if (currentTile == null) break;

            currentTile = collapseRandomTile(currentTile);

            tileStack.Push(currentTile);

            //check constraints
            propagateNeighbors(getNeighbors(currentTile, mapGrid), mapGrid);

            if (tileStack.Count != 0 && tileStack.Peek().tilePossibilities.Count != 0)
            {
                tileStack = backtrackStack(tileStack);
            }
        }
        foreach (MapTile tile in mapGrid)
        {
            //all of these damn calculations are with the assumption that z is the "up" vector and x-y are in a 2D space, but i forgot thast unity doesnt do that
            Instantiate(connectionData.mapTilePrefabs.Find(x => x.GetComponent<MapTile>() == tile.collapsedTile), new Vector3(tile.gridPosition.x * tileSize, 0, tile.gridPosition.y * tileSize), Quaternion.identity);
        }
    }

    //finds a tile with the lowest entropy in a list
    MapTile findLowestEntropy(MapTile[,] mapGrid)
    {
        MapTile minCell = null;
        int minEntropy = int.MaxValue;
        for (int x = 0; x < mapSize - 1; x++)
        {
            for (int y = 0; y < mapSize - 1; y++)
            {
                int entropy = mapGrid[x, y].tilePossibilities.Count;
                //Debug.Log("" + entropy);
                if (entropy > 1 && entropy < minEntropy)
                {
                    minEntropy = entropy;
                    minCell = mapGrid[x, y];
                }
            }
        }
        //Debug.Log("" + minCell);
        return minCell;
    }

    MapTile collapseRandomTile(MapTile tile)
    {
        int tileIndex = Random.Range(0, tile.tilePossibilities.Count - 1);
        //Debug.Log("tile possibilities: " + tile.tilePossibilities.Count + " random tile index: " + tileIndex);
        Vector2 gridPos = tile.gridPosition;
        tile.collapsedTile = tile.tilePossibilities[tileIndex];
        tile.collapsed = true;
        tile.setGridPosition(gridPos);
        return tile;
    }

    Stack<MapTile> backtrackStack(Stack<MapTile> stack)
    {
        while (stack.Count > 0)
        {
            MapTile top = stack.Peek();
            stack.Pop();

            top.tilePossibilities = connectionData.standardSet;
            top.Reset();
        }
        return stack;
    }

    List<MapTile> getNeighbors(MapTile tile, MapTile[,] grid)
    {
        List<MapTile> neighbors = new List<MapTile>();
        if (tile.gridPosition.x != 0)
        {
            neighbors.Add(grid[(int)(tile.gridPosition.x - 1), (int)tile.gridPosition.y]);
            Debug.Log("x inner edge");
        }
        if (tile.gridPosition.x != grid.GetLength(0) - 1)
        {
            neighbors.Add(grid[(int)(tile.gridPosition.x + 1), (int)tile.gridPosition.y]);
            Debug.Log("x outer edge");
        }
        if (tile.gridPosition.y != 0)
        {
            neighbors.Add(grid[(int)tile.gridPosition.x, (int)(tile.gridPosition.y - 1)]);
            Debug.Log("y inner edge");
        }
        if (tile.gridPosition.y != grid.GetLength(1) - 1)
        {
            neighbors.Add(grid[(int)tile.gridPosition.x, (int)(tile.gridPosition.y + 1)]);
            Debug.Log("y outer edge");
        }
        return neighbors;
    }
    
    //takes the neighbors of a tile, checks each of their neighbors, combines all lists of connections and reduces down to final possibilities
    /*void propagateNeighbors(List<MapTile> neighbors, MapTile[,] grid)
    {
        foreach(MapTile tile in neighbors) //each neighbor tile
        {
            tile.tilePossibilities.Clear(); //clear possibilities to replace
            List<MapTile> tileNeighbors = getNeighbors(tile, grid);//find each neighbor of new tile
            Debug.Log("tNeighbors: " + tileNeighbors);
            int collapseNum = 0;
            foreach (MapTile collapseCheck in tileNeighbors) //fill possibilities with all of the possible connections
            {
                Debug.Log("neighbors neighbor run through");
                if (collapseCheck.collapsed == true && collapseCheck.gridPosition.y == tile.gridPosition.y - 1) //if newtile is below a collapsed tile
                {
                    collapseNum++;
                    foreach (int socketType in collapseCheck.backConnections)
                    {
                        tile.tilePossibilities.AddRange(connectionData.retrieveSet(2, socketType));
                    }
                }
                if (collapseCheck.collapsed == true && collapseCheck.gridPosition.y == tile.gridPosition.y + 1) //if newtile is above a collapsed tile
                {
                    collapseNum++;
                    foreach (int socketType in collapseCheck.backConnections)
                    {
                        tile.tilePossibilities.AddRange(connectionData.retrieveSet(1, socketType));
                    }
                }
                if (collapseCheck.collapsed == true && collapseCheck.gridPosition.x == tile.gridPosition.x - 1) //if newtile is to the right of a collapsed tile
                {
                    collapseNum++;
                    foreach (int socketType in collapseCheck.backConnections)
                    {
                        tile.tilePossibilities.AddRange(connectionData.retrieveSet(4, socketType));
                    }
                }
                if (collapseCheck.collapsed == true && collapseCheck.gridPosition.x == tile.gridPosition.x + 1) //if newtile is to the left of a collapsed tile
                {
                    collapseNum++;
                    foreach (int socketType in collapseCheck.backConnections)
                    {
                        tile.tilePossibilities.AddRange(connectionData.retrieveSet(3, socketType));
                    }
                }
            }
            //tilepossibilities is filled out with duplicate data - we only want a tile that has been repeated the number of times it has been checked by a collapsed tile
            List<int> finalList = new();
            for (int i = 0; i < tile.tilePossibilities.Count; i++)
            {
                int possibilityCount = 0;
                for (int j = 0; j < tile.tilePossibilities.Count; j++)
                {
                    if (tile.tilePossibilities[j] == tile.tilePossibilities[i])
                        possibilityCount++;
                }
                Debug.Log("" + possibilityCount + " >= " + collapseNum);
                if (possibilityCount >= collapseNum)
                {
                    finalList.Add(tile.tilePossibilities[i]);
                }
            }
            //finally set the new list of possibilities to the focused neighboring tile
            foreach (int i in finalList)
            {
                Debug.Log("[" + tile.gridPosition.x + " , " + tile.gridPosition.y + "] list element: " + i);
            }
            tile.tilePossibilities = finalList;
        }
    }*/

    void propagateNeighbors(MapTile tile, MapTile[,] grid, Stack<MapTile> backtrackStack)
    {
        List<MapTile> neighbors = new List<MapTile>();
        if (tile.gridPosition.x != 0) //tile to left
        {
            neighbors.Add(grid[(int)(tile.gridPosition.x - 1), (int)tile.gridPosition.y]);
            for (int i = 0; i < grid[(int)tile.gridPosition.x - 1, (int)tile.gridPosition.y].tilePossibilities.Count; i++)
            {
                if(tile.leftConnections.Find(grid[(int)tile.gridPosition.x - 1, (int)tile.gridPosition.y].tilePossibilities))
            }
        }
        if (tile.gridPosition.x != grid.GetLength(0) - 1) //tile to right
        {
            neighbors.Add(grid[(int)(tile.gridPosition.x + 1), (int)tile.gridPosition.y]);
        }
        if (tile.gridPosition.y != 0) //tile to down
        {
            neighbors.Add(grid[(int)tile.gridPosition.x, (int)(tile.gridPosition.y - 1)]);
        }
        if (tile.gridPosition.y != grid.GetLength(1) - 1) //tile to up
        {
            neighbors.Add(grid[(int)tile.gridPosition.x, (int)(tile.gridPosition.y + 1)]);
        }
    }

}
