using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionData : MonoBehaviour
{
    //SOCKET LIST !!! -----------
    //
    // 0 = NULL - USED FOR UNCOLLAPSED TILES
    // 1 = AIR - USED FOR AIR SPACE SOCKETS
    // 2 = WALL - USED FOR FULLY BLOCKED SOCKETS
    //
    // --------------------------
    public List<GameObject> mapTilePrefabs;

    //lists of all possible sets of connections
    public List<int> standardSet;
    //arrays of tile IDs per socket state - needs new array per new socket state
    //up sets 
    public List<int> upSet1;
    public List<int> upSet2;
    //down sets
    public List<int> downSet1;
    public List<int> downSet2;
    //left sets
    public List<int> leftSet1;
    public List<int> leftSet2;
    //right sets
    public List<int> rightSet1;
    public List<int> rightSet2;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < mapTilePrefabs.Count; i++)
        {
            MapTile currentPrefab = mapTilePrefabs[i].GetComponent<MapTile>();
            //creating set filled with every ID
            standardSet.Add(currentPrefab.tileID);
            standardSet.Sort((tile1, tile2) => tile1.CompareTo(tile2)); //sort in ascending order

            //populate up sets
            if (currentPrefab.GetSockets(1) == 1)
            {
                upSet1.Add(currentPrefab.tileID);
            }
            if (currentPrefab.GetSockets(1) == 2)
            {
                upSet2.Add(currentPrefab.tileID);
            }
            
            //populate down sets
            if (currentPrefab.GetSockets(2) == 1)
            {
                downSet1.Add(currentPrefab.tileID);
            }
            if (currentPrefab.GetSockets(2) == 2)
            {
                downSet2.Add(currentPrefab.tileID);
            }
            
            //populate left sets
            if (currentPrefab.GetSockets(3) == 1)
            {
                leftSet1.Add(currentPrefab.tileID);
            }
            if (currentPrefab.GetSockets(3) == 2)
            {
                leftSet2.Add(currentPrefab.tileID);
            }
            
            //populate right sets
            if (currentPrefab.GetSockets(4) == 1)
            {
                rightSet1.Add(currentPrefab.tileID);
            }
            if(currentPrefab.GetSockets(4) == 2)
            {
                rightSet2.Add(currentPrefab.tileID);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
