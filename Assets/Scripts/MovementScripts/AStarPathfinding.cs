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

    private Dictionary<WeightedPosition, bool> objectGrid;

    public List<Vector3> GetPath(Vector3 startPos, Vector3 target)
    {
        //Initialize start position
        WeightedPosition startPosition = new(0.0f, startPos)
        {
            IsVisited = true
        };
        frontier.Enqueue(startPosition, startPosition.Weight);
        costSoFar.Add(startPosition, startPosition.Weight);
        frontierSet.Add(startPosition);

        WeightedPosition endPoint = null;

        //While frontier has elements
        while (frontier.Count > 0)
        {
            var currentPoint = frontier.Dequeue();
            currentPoint.IsVisited = true;

            //If current point is end point
            if (currentPoint.Position == target)
            {
                endPoint = currentPoint;
                break;
            }
        }

        //If we find an end point
        if (endPoint != null)
        {

        }

        return null;
    }

  /*  private List<WeightedPosition> GetVisitableNeighbors(WeightedPosition pos)
    {
        List<WeightedPosition> neighbors = new();

        WeightedPosition northPos = new(1.0f, new Vector3(pos.Position.x, pos.Position.y + 1, pos.Position.z));
        WeightedPosition eastPos = new(1.0f, new Vector3(pos.Position.x - 1, pos.Position.y, pos.Position.z));
    }*/
}

public class WeightedPosition
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