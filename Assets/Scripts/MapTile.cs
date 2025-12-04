using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    //front, back, left, right sockets for sorting
    public int frontSocket;
    public int backSocket;
    public int leftSocket;
    public int rightSocket;
    //front, back, left, right list of connections
    public List<int> frontConnections;
    public List<int> backConnections;
    public List<int> leftConnections;
    public List<int> rightConnections;
    public int tileID; //used for organization within connectionData instead of storing prefabs
    // Start is called before the first frame update
    public List<int> tilePossibilities;
    bool collapsed = false;
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
                return rightSocket;
            case 3:
                return backSocket;
            case 4:
                return leftSocket;
        }
        return 0;
    }

    public int GetConnections(int choice)
    {
        switch (choice)
        {
            case 1:
                return frontConnections.Count;
            case 2:
                return rightConnections.Count;
            case 3:
                return backConnections.Count;
            case 4:
                return leftConnections.Count;
        }
        return 0;
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
}
