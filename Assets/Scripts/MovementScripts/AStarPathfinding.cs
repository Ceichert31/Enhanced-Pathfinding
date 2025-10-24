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
        HashSet<Vector3> visited = new();

        //Initialize start position
        WeightedPosition startWeight = new(0.0f, startPos);
        frontier.Enqueue(startWeight, 0);
        costSoFar.Add(startWeight, 0);
        frontierSet.Add(startWeight);

        WeightedPosition endPoint = null;

        //While frontier has elements
        while (frontier.Count > 0)
        {
            var currentPoint = frontier.Dequeue();
            visited.Add(currentPoint.Position);

            //If current point is end point
            if (currentPoint.Position == target)
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
                float newCost = costSoFar[currentPoint] + neighbor.Weight;

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
            cameFrom.TryGetValue(endPoint.Position, out Vector3 current);

            if (current == null)
            {
                return null;
            }

            while (current != startPos)
            {
                path.Add(current);

                if (!cameFrom.TryGetValue(current, out current))
                    return path;
            }
            path.Reverse();

            return path;
        }

        return null;
    }

    private List<WeightedPosition> GetVisitableNeighbors(WeightedPosition pos, ref HashSet<Vector3> visited)
    {
        List<WeightedPosition> neighbors = new();

        Vector2 northPos = new(pos.Position.x+1, pos.Position.z);
        Vector2 eastPos = new(pos.Position.x-1, pos.Position.z);
        Vector2 southPos = new(pos.Position.x, pos.Position.z+1);
        Vector2 westPos = new(pos.Position.x, pos.Position.z-1);

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
    private WeightedPosition ValidatePosition(Vector3 neighbor, ref HashSet<Vector3> visited)
    {
        //Get navmesh data and check is the neighbor is valid
        if (navmesh.navMeshGrid.TryGetValue(new Vector2(neighbor.x, neighbor.z), out TerrainData data))
        {
            if (!data.IsWalkable)
                return null;

            if (visited.Contains(data.Position))
                return null;

            if (!IsInBounds(data.Position))
                return null;

            return new WeightedPosition(data.MovementCost, data.Position);
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