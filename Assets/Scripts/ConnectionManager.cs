using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{

    public int tileSize; //dimension of a tile, given that the tile is square
    public int mapSize; //dimension of the map, for a map of size (mapsize X mapsize)

    //data manager for lists of connection types
    [SerializeField] ConnectionData connectionData;

    public bool run;

    public GameObject current = null;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /* //debug scenario
        if (run)
        {
            //make a way to even chack for the first 'check' square
            List<GameObject> list = check(current);
            if (list.Count > 0)
            {
                GameObject newPlace = Instantiate(list[Random.Range(0, list.Count)]);
                newPlace.transform.position = transform.position;//this has to be set to the goal pos
            }
            run = false;
        }
        */
    }

    List<int> check(MapTile checkedObject)
    {
        List<int> possibilities = new List<int>();
        if (checkedObject.GetSockets(1) == 0)//checks up
        {
            if (checkedObject.GetComponent<MapTile>().GetConnections(1) == 0)
            {
                for (int i = 0; i < connectionData.mapTilePrefabs.Count; i++)
                {
                    if (connectionData.mapTilePrefabs[i].GetComponent<MapTile>().GetSockets(3) == 0)
                    {
                        chooseFrom.Add(connectionData.mapTilePrefabs[i]);
                    }
                }
            }
        }
        if (checkedObject.GetComponent<MapTile>().GetSockets(2) == 0)//checks right
        {
            if (checkedObject.GetComponent<MapTile>().GetConnections(2) == 0)
            {
                for (int i = 0; i < connectionData.mapTilePrefabs.Count; i++)
                {
                    if (connectionData.mapTilePrefabs[i].GetComponent<MapTile>().GetSockets(4) == 0)
                    {
                        chooseFrom.Add(connectionData.mapTilePrefabs[i]);
                    }
                }
            }
        }
        if (checkedObject.GetComponent<MapTile>().GetSockets(3) == 0)//checks down
        {
            if (checkedObject.GetComponent<MapTile>().GetConnections(3) == 0)
            {
                for (int i = 0; i < connectionData.mapTilePrefabs.Count; i++)
                {
                    if (connectionData.mapTilePrefabs[i].GetComponent<MapTile>().GetSockets(1) == 0)
                    {
                        chooseFrom.Add(connectionData.mapTilePrefabs[i]);
                    }
                }
            }
        }
        if (checkedObject.GetComponent<MapTile>().GetSockets(4) == 0)//checks left
        {
            if (checkedObject.GetComponent<MapTile>().GetConnections(4) == 0)
            {
                for (int i = 0; i < connectionData.mapTilePrefabs.Count; i++)
                {
                    if (connectionData.mapTilePrefabs[i].GetComponent<MapTile>().GetSockets(2) == 0)
                    {
                        chooseFrom.Add(connectionData.mapTilePrefabs[i]);
                    }
                }
            }
        }
        return chooseFrom;//returns a list of possible works. there may be an issue where it will double up on like a corner to keep in mind.
    }
}
