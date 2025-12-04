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
                mapGrid[x, y].tilePossibilities = connectionData.standardSet;
            }
        }
        //collapse tile to start
        Stack<MapTile> tileStack = new();
        while(true)
        {
            MapTile currentTile = findLowestEntropy(mapGrid);
            if (currentTile == null) break;

            currentTile = collapseRandomTile(currentTile);

            tileStack.Push(currentTile);

            //check constraints

            if(tileStack.Count != 0 && tileStack.Peek().tilePossibilities.Count != 0)
            {
                //backtrack
            }
        }
        
    }

    //finds a tile with the lowest entropy in a list
    MapTile findLowestEntropy(MapTile[,] mapGrid)
    {
        MapTile minCell = null;
        int minEntropy = int.MaxValue;
        for (int x = 0; x < mapGrid.Length; x++)
        {
            for (int y = 0; y < mapGrid.Length; y++)
            {
                int entropy = mapGrid[x, y].tilePossibilities.Count;
                if (entropy > 1 && entropy < minEntropy)
                {
                    minEntropy = entropy;
                    minCell = mapGrid[x, y];
                }
            }
        }
        return minCell;
    }

    MapTile collapseRandomTile(MapTile tile)
    {
        int tileIndex = Random.Range(0, tile.tilePossibilities.Count - 1);
        int prefabID = tile.tilePossibilities[tileIndex];
        tile = connectionData.mapTilePrefabs[prefabID].GetComponent<MapTile>();
        return tile;
    }
    
    Stack<MapTile> backtrackStack(Stack<MapTile> stack)
    {
        while (stack.Count > 0)
        {
            MapTile top = stack.Pop();
            top.tilePossibilities = connectionData.standardSet;
            top.Reset();
        }
        return stack;
    }

}
