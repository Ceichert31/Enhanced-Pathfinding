using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class AStarPathfinding : MonoBehaviour, IPathfinder
{
    private Dictionary<WeightedPosition, float> costSoFar;
    private Dictionary<WeightedPosition, WeightedPosition> cameFrom;
    private PriorityQueue<WeightedPosition, float> frontier;
    private HashSet<WeightedPosition> frontierSet;

    public List<Vector3> GetPath(Vector3 startPos, Vector3 target)
    {
        //Initialize start position
        WeightedPosition startPosition = new WeightedPosition(0.0f, startPos);
        startPosition.IsVisited = true;
        frontier.Enqueue(startPosition, startPosition.Weight);
        costSoFar.Add(startPosition, startPosition.Weight);


        return null;
    }
}

public struct WeightedPosition
{
    public WeightedPosition(float weight, Vector3 pos)
    {
        Weight = weight;
        Position = pos;
        IsVisited = false;
    }

    public float Weight;
    public Vector3 Position;
    public bool IsVisited;
}