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
        //collapse random tile to start
        int randX = Random.Range(0, mapSize);
        int randY = Random.Range(0, mapSize);
        Stack<MapTile> tileStack = new();
        MapTile currentTile = mapGrid[randX, randY];
        tileStack.Push(currentTile);
    }

    MapTile findLowestEntropy(MapTile[,] mapGrid)
    {
        MapTile minCell = null;
        int minEntropy = int.MaxValue;
        for(int x = 0; x < mapGrid.Length; x++)
        {
            for(int y = 0; y < mapGrid.Length; y++)
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

}
