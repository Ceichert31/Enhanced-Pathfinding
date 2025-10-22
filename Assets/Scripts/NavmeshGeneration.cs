using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavmeshGeneration : MonoBehaviour
{
    public Dictionary<Vector3, WeightedPosition> navMeshGrid = new();

    [SerializeField]
    private Vector2 minBound;
    [SerializeField]
    private Vector2 maxBound;
    [SerializeField]
    private LayerMask obstacleLayer;

    private const float NAVMESH_HEIGHT = 100.0f;

    public float NavmeshHeight { get { return NAVMESH_HEIGHT; } }
    public Vector2 MinimumBoundary => minBound;
    public Vector2 MaximumBoundary => maxBound;
    public void Awake()
    {
        for (int i = (int)minBound.x; i < maxBound.x; ++i)
        {
            for (int j = (int)minBound.y; j < maxBound.y; ++j)
            {
                Vector3 currentPosition = new(i, NAVMESH_HEIGHT, j);

                //Check for obstacle layer
                if (Physics.Raycast(currentPosition, Vector3.down, out RaycastHit hitInfo, NAVMESH_HEIGHT, obstacleLayer))
                {
                    //Add a negative weight for an impassible object
                    navMeshGrid.Add(currentPosition, new WeightedPosition(-1, currentPosition));
                }
                else
                {
                    float weight = hitInfo.point.y;

                    if (weight <= 0)
                        weight = 1;

                    //Set cost as the height of the point of contact
                    navMeshGrid.Add(currentPosition, new WeightedPosition(weight, currentPosition));
                }
            }
        }
    }
}
