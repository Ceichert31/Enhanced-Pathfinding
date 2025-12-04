using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionData : MonoBehaviour
{
    public List<GameObject> mapTilePrefabs;
    public List<int> standardSet;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < mapTilePrefabs.Count; i++)
        {
            standardSet.Add(mapTilePrefabs[i].GetComponent<MapTile>().tileID);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
