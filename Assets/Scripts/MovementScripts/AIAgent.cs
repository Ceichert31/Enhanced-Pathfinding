using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgent : MonoBehaviour
{
    //Handle target and agent related stats:
    //like speed, and whatnot 

    [SerializeField]
    private IPathfinder pathfindingAlgorithm;

    [SerializeField]
    private Transform target;

    [Header("Agent Settings")]

    [SerializeField]
    private float agentSpeed = 1f;

    private void Start()
    {
        pathfindingAlgorithm = GetComponent<IPathfinder>();

        if (pathfindingAlgorithm == null)
        {
            throw new NullReferenceException("Please add a pathfinding algorithm to the AI Agent!");
        }
    }


    private void Update()
    {
        //Get path to target
        var path = pathfindingAlgorithm.GetPath(target.position);

        if (path == null) return;

        transform.position = Vector3.MoveTowards(transform.position, path[0], agentSpeed * Time.deltaTime);
    }

}

public interface IPathfinder
{
    List<Vector3> GetPath(Vector3 target);
}
