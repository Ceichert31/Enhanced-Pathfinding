using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(NavmeshGeneration))]
public class AStarPathfinding : MonoBehaviour, IPathfinder
{
    private NavmeshGeneration navmesh;

    private const float DEFAULT_MOVEMENT_COST = 1f;

    private Vector3 currentPosition;
    private void Start()
    {
        navmesh = GetComponent<NavmeshGeneration>();
    }

    public List<Vector3> GetPath(Vector3 startPos, Vector3 target)
    {
        Dictionary<WeightedPosition, float> costSoFar = new();
        Dictionary<Vector3, Vector3> cameFrom = new();
        PriorityQueue<WeightedPosition, float> frontier = new();
        HashSet<Vector3> frontierSet = new();
        HashSet<Vector3> visited = new();

        //Exit early if target is unreachable
        if (ValidatePosition(target, ref visited, ref frontierSet) == null)
        {
            return null;
        }

        //Initialize start position
        WeightedPosition startWeight = new(0.0f, startPos);
        frontier.Enqueue(startWeight, 0);
        costSoFar.Add(startWeight, 0);
        frontierSet.Add(startWeight.Position);

        WeightedPosition endPoint = null;

        //While frontier has elements
        while (frontier.Count > 0)
        {
            var currentPoint = frontier.Dequeue();
            currentPosition = currentPoint.Position;
            visited.Add(currentPoint.Position);

            //If current point is end point
            //(Use vector3.distance to account for any floating point errors)
            if (currentPoint.Position == target)
            {
                endPoint = currentPoint;
                break;
            }

            var neighbors = GetVisitableNeighbors(currentPoint.Position, ref visited, ref frontierSet);
            if (neighbors.Count == 0)
            {
                continue;
            }

            foreach (var neighbor in neighbors) 
            {
                //Calculate new cost of travel
                float newCost = costSoFar[currentPoint] + DEFAULT_MOVEMENT_COST;

                //Check if the neighbor exists already in the frontier and if it does, check its old cost
                if (!frontierSet.Contains(neighbor.Position) || costSoFar[neighbor] > newCost)
                {
                    costSoFar[neighbor] = newCost;

                    float priority = newCost + Heuristic(neighbor.Position, target);

                    //Update came from dictionary
                    cameFrom[neighbor.Position] = currentPoint.Position;
                    //Add neighbor to frontier
                    frontier.Enqueue(neighbor, priority);
                    frontierSet.Add(neighbor.Position);
                }

            }
        }

        //If we find an end point
        //Convert from 2D space to 3D space with navmesh
        if (endPoint != null)
        {
            List<Vector3> path = new();

            path.Add(endPoint.Position);
            cameFrom.TryGetValue(endPoint.Position, out Vector3 current);

            if (current == null)
            {
                return null;
            }

            //Recreate path
            while (current != startPos)
            {
                path.Add(current);

                //If we can't get pos, break the loop
                if (!cameFrom.TryGetValue(current, out current))
                {
                    break;
                } 
            }
            //Reverse path and convert to 3D
            path.Reverse();
            return path;
        }

        return null;
    }

    /// <summary>
    /// Iterate through all connections to a point on the navmesh, check if they are valid
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="visited"></param>
    /// <param name="frontierSet"></param>
    /// <returns></returns>
    private List<WeightedPosition> GetVisitableNeighbors(Vector3 pos, ref HashSet<Vector3> visited, ref HashSet<Vector3> frontierSet)
    {
        List<WeightedPosition> neighbors = new();

        //Ask navmesh what current points we can reach
        var terrainData = navmesh.GetNavmeshValue(new(pos.x, pos.z), pos.y);
        foreach (Vector2 neighbor in terrainData.GetConnections())
        {
            TerrainData neighbor3D = navmesh.GetNavmeshValue(neighbor, pos.y);
            if (neighbor3D == null)
                continue;

            var position = ValidatePosition(neighbor3D.Position, ref visited, ref frontierSet);
            if (position != null)
            {
                neighbors.Add(position);
            }
        }
        return neighbors;
    }
    /// <summary>
    /// Validates that a neighbor meets certain criteria 
    /// </summary>
    /// <param name="neighbor"></param>
    /// <returns>A weighted position if valid, otherwise null</returns>
    private WeightedPosition ValidatePosition(Vector3 neighbor, ref HashSet<Vector3> visited, ref HashSet<Vector3> frontierSet)
    {
        Vector2 neighborKey = new(neighbor.x, neighbor.z);

        //Get the terrain data from navmesh
        var point = navmesh.GetNavmeshValue(neighborKey, neighbor.y);

        if (point == null)
            return null;

        if (!point.IsWalkable)
            return null;

        if (frontierSet.Contains(neighbor))
            return null;

        if (visited.Contains(neighbor))
            return null;

        if (!IsInBounds(neighbor))
            return null;

        return new WeightedPosition(point.MovementCost, neighbor);
    }

    private bool IsInBounds(Vector3 pos)
    {
        if (pos.x > navmesh.MinimumBoundary.x && pos.z > navmesh.MinimumBoundary.y
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