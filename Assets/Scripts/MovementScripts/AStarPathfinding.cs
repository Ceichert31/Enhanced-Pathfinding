using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

public class AStarPathfinding : MonoBehaviour, IPathfinder
{
    private NavmeshGeneration navmesh;

    private void Start()
    {
        navmesh = GetComponent<NavmeshGeneration>();
    }

    public List<Vector3> GetPath(Vector3 startPos, Vector3 target)
    {
        Dictionary<WeightedPosition, float> costSoFar = new();
        Dictionary<Vector3, Vector3> cameFrom = new();
        PriorityQueue<WeightedPosition, float> frontier = new();
        HashSet<WeightedPosition> frontierSet = new();
        Dictionary<WeightedPosition, bool> visited = new();

        Vector3 startPosition = new(startPos.x, navmesh.NavmeshHeight, startPos.z);
        Vector3 endPosition = new(target.x, navmesh.NavmeshHeight, target.z);

        //Initialize start position
        WeightedPosition startWeight = new(0.0f, startPosition);
        frontier.Enqueue(startWeight, 0);
        costSoFar.Add(startWeight, 0);
        frontierSet.Add(startWeight);

        WeightedPosition endPoint = null;

        //While frontier has elements
        while (frontier.Count > 0)
        {
            var currentPoint = frontier.Dequeue();
            visited.Add(currentPoint, true);

            //If current point is end point
            if (currentPoint.Position == endPosition)
            {
                endPoint = currentPoint;
                break;
            }

            var neighbors = GetVisitableNeighbors(currentPoint, ref visited);
            if (neighbors.Count == 0)
            {
                continue;
            }

            foreach (var neighbor in neighbors) 
            {
                //Calculate new cost of travel
                float newCost = costSoFar[currentPoint] + 1;

                //Check if the neighbor exists already in the frontier and if it does, check its old cost
                if (!frontierSet.Contains(neighbor) || costSoFar[neighbor] > newCost)
                {
                    costSoFar[neighbor] = newCost;

                    float priority = newCost + Heuristic(neighbor.Position, target);

                    //Update came from dictionary
                    cameFrom[neighbor.Position] = currentPoint.Position;
                    //Add neighbor to frontier
                    frontier.Enqueue(neighbor, priority);
                    frontierSet.Add(neighbor);
                }

            }
        }

        //If we find an end point
        if (endPoint != null)
        {
            List<Vector3> path = new();

            path.Add(endPoint.Position);
            Vector3 current = cameFrom[endPoint.Position];

            while (current != startPosition)
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Reverse();

            return path;
        }

        return null;
    }

    private List<WeightedPosition> GetVisitableNeighbors(WeightedPosition pos, ref Dictionary<WeightedPosition, bool> visited)
    {
        List<WeightedPosition> neighbors = new();

        Vector3 northPos = new(pos.Position.x+1, navmesh.NavmeshHeight, pos.Position.z);
        Vector3 eastPos = new(pos.Position.x-1, navmesh.NavmeshHeight, pos.Position.z);
        Vector3 southPos = new(pos.Position.x, navmesh.NavmeshHeight, pos.Position.z+1);
        Vector3 westPos = new(pos.Position.x, navmesh.NavmeshHeight, pos.Position.z-1);

        var position = ValidatePosition(northPos, ref visited);
        if (position != null)
        {
            neighbors.Add(position);
        }
        position = ValidatePosition(eastPos, ref visited);
        if (position != null)
        {
            neighbors.Add(position);
        }
        position = ValidatePosition(southPos, ref visited);
        if (position != null)
        {
            neighbors.Add(position);
        }
        position = ValidatePosition(westPos, ref visited);
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
    private WeightedPosition ValidatePosition(Vector3 neighbor, ref Dictionary<WeightedPosition, bool> visited)
    {
        //Check that position isn't obstructed(navmesh)
        //Check that position isn't visited
        //Check that position isn't in frontier set
        //Check that position is in bounds

        if (navmesh.navMeshGrid.TryGetValue(neighbor, out WeightedPosition weightedPos))
        {
            if (weightedPos.Weight < 0)
                return null;

            if (visited.ContainsKey(weightedPos))
                return null;

            if (!IsInBounds(weightedPos.Position))
                return null;

            return weightedPos;
        }
        return null;
    }

    private bool IsInBounds(Vector3 pos)
    {
        if (pos.x > navmesh.MinimumBoundary.x && pos.z > navmesh.MinimumBoundary.x
            && pos.x < navmesh.MaximumBoundary.x && pos.z < navmesh.MaximumBoundary.y)
            return true;
        return false;
    }

    public float Heuristic(Vector3 start, Vector3 end)
    {
        return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.z - end.z);
    }
}

public class WeightedPosition
{
    public WeightedPosition(float weight, Vector3 pos)
    {
        Weight = weight;
        Position = pos;
    }

    public float Weight;
    public Vector3 Position;
}