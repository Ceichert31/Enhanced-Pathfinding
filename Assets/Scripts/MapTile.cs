using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    //front, back, left, right sockets for sorting
    public Vector2Int gridPosition;
    public int frontSocket;
    public int backSocket;
    public int leftSocket;
    public int rightSocket;
    //front, back, left, right list of connections
    public List<int> frontConnections;
    public List<int> backConnections;
    public List<int> leftConnections;
    public List<int> rightConnections;
    //front, back, left, right list of connections
    public List<MapTile> frontCompatible;
    public List<MapTile> backCompatible;
    public List<MapTile> leftCompatible;
    public List<MapTile> rightCompatible;
    public int tileID; //used for organization within connectionData instead of storing prefabs
    // Start is called before the first frame update
    public List<MapTile> tilePossibilities;
    public MapTile collapsedTile;
    public bool collapsed = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetSockets(int choice)
    {
        switch (choice)
        {
            case 1:
                return frontSocket;
            case 2:
                return backSocket;
            case 3:
                return leftSocket;
            case 4:
                return rightSocket;
        }
        return 0;
    }

    public List<int> GetConnections(int choice)
    {
        switch (choice)
        {
            case 1:
                return frontConnections;
            case 2:
                return backConnections;
            case 3:
                return leftConnections;
            case 4:
                return rightConnections;
        }
        return null;
    }

    public List<MapTile> GetCompatibleConnections(int choice)
    {
        switch (choice)
        {
            case 1:
                return frontCompatible;
            case 2:
                return backCompatible;
            case 3:
                return leftCompatible;
            case 4:
                return rightCompatible;
        }
        return null;
    }

    public void Reset()
    {
        tileID = 0;
        collapsed = false;
        frontSocket = 0;
        backSocket = 0;
        leftSocket = 0;
        rightSocket = 0;
        frontConnections.Clear();
        backConnections.Clear();
        leftConnections.Clear();
        rightConnections.Clear();
    }

    public void setGridPosition(Vector2Int gridPos)
    {
        gridPosition = gridPos;
    }
}
