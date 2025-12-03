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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
