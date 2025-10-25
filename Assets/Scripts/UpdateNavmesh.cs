using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateNavmesh : MonoBehaviour
{
    private NavmeshGeneration navmesh;

    private void Awake()
    {
        navmesh = GetComponent<NavmeshGeneration>();
    }
    public void EnableDebug(BoolEvent ctx)
    {
        navmesh.EnableDebug = ctx.Value;
    }
    public void AddObstacle(VoidEvent ctx)
    {
        navmesh.PlaceRandomObject();
    }
}
