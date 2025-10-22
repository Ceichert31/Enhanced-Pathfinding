using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavmeshGeneration : MonoBehaviour
{
    public Dictionary<Vector3, WeightedPosition> navMeshGrid;

    [SerializeField]
    private Vector2 minBound;
    [SerializeField]
    private Vector2 maxBound;
    [SerializeField]
    private LayerMask obstacleLayer;

    const float NAVMESH_HEIGHT = 100.0f;

    private void Awake()
    {
        for (int i = (int)minBound.x; i < maxBound.x; ++i)
        {
            for (int j = (int)minBound.y; j < maxBound.y; ++j)
            {
                //Check for obstacle layer
                if (Physics.Raycast(new Vector3(i, NAVMESH_HEIGHT, j), Vector3.down, NAVMESH_HEIGHT, obstacleLayer))
                {

                }
            }
        }
    }
}
