using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionData : MonoBehaviour
{
    public List<GameObject> mapTilePrefabs;

    //lists of all possible sets of connections
    public List<int> standardSet;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < mapTilePrefabs.Count; i++)
        {
            standardSet.Add(mapTilePrefabs[i].GetComponent<MapTile>().tileID);
            standardSet.Sort((tile1, tile2) => tile1.CompareTo(tile2)); //sort in ascending order
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
