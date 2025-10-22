using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

public class AStarPathfinding : MonoBehaviour, IPathfinder
{
    private Dictionary<WeightedPosition, float> costSoFar;
    private Dictionary<WeightedPosition, WeightedPosition> cameFrom;
    private PriorityQueue<WeightedPosition, float> frontier;
    private HashSet<WeightedPosition> frontierSet;

    private Dictionary<WeightedPosition, bool> objectGrid;

    private NavmeshGeneration navmesh;
    private void Start()
    {
        navmesh = GetComponent<NavmeshGeneration>();
    }

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

            var neighbors = GetVisitableNeighbors(currentPoint);
            if (neighbors.Count == 0)
            {
                continue;
            }


        }

        //If we find an end point
        if (endPoint != null)
        {

        }

        return null;
    }

    private List<WeightedPosition> GetVisitableNeighbors(WeightedPosition pos)
    {
        List<WeightedPosition> neighbors = new();

        Vector3 northPos = new(pos.Position.x+1, navmesh.NavmeshHeight, pos.Position.z);
        Vector3 eastPos = new(pos.Position.x-1, navmesh.NavmeshHeight, pos.Position.z);
        Vector3 southPos = new(pos.Position.x-1, navmesh.NavmeshHeight, pos.Position.z+1);
        Vector3 westPos = new(pos.Position.x-1, navmesh.NavmeshHeight, pos.Position.z-1);

        var position = ValidatePosition(northPos);
        if (position != null)
        {
            neighbors.Add(position);
        }
        position = ValidatePosition(eastPos);
        if (position != null)
        {
            neighbors.Add(position);
        }
        position = ValidatePosition(southPos);
        if (position != null)
        {
            neighbors.Add(position);
        }
        position = ValidatePosition(westPos);
        if (position != null)
        {
            neighbors.Add(position);
        }

        return neighbors;
    }
    /// <summary>
    /// Validates that a neighbor meets certain criteria 
    /// </summary>
    /// <param name="neighbor"></param>
    /// <returns>A weighted position if valid, otherwise null</returns>
    private WeightedPosition ValidatePosition(Vector3 neighbor)
    {
        //Check that position isn't obstructed(navmesh)
        //Check that position isn't visited
        //Check that position isn't in frontier set
        //Check that position is in bounds

        if (navmesh.navMeshGrid.TryGetValue(neighbor, out WeightedPosition weightedPos))
        {
            if (weightedPos.Weight < 0)
                return null; 

            if (weightedPos.IsVisited)
                return null;

            if (frontierSet.Contains(weightedPos))
                return null;

            if (!IsInBounds(weightedPos.Position))
                return null;

            return weightedPos;
        }
        return null;
    }

    private bool IsInBounds(Vector3 pos)
    {
        if (pos.x > navmesh.MinimumBoundary.x && pos.y > navmesh.MinimumBoundary.x
            && pos.x < navmesh.MaximumBoundary.x && pos.y < navmesh.MaximumBoundary.y)
            return true;
        return false;
    }
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