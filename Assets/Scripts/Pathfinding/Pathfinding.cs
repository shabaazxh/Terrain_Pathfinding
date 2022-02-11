using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{
    PathRequestManager requestManager;
    GridA grid;

    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<GridA>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
       StartCoroutine(FindPath(startPos, targetPos));
    }


    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSucess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSucess = true;

                    break;
                }

                // Loop neighbouring nodes

                foreach (Node neightbour in grid.GetNeightbours(currentNode))
                {
                    if (!neightbour.walkable || closedSet.Contains(neightbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeightbour = currentNode.gCost + GetDistance(currentNode, neightbour) + neightbour.movementPenalty;

                    if (newMovementCostToNeightbour < neightbour.gCost || !openSet.Contains(neightbour))
                    {
                        neightbour.gCost = newMovementCostToNeightbour;
                        neightbour.hCost = GetDistance(neightbour, targetNode);
                        neightbour.parent = currentNode;

                        if (!openSet.Contains(neightbour))
                            openSet.Add(neightbour);
                        else
                            openSet.UpdateItem(neightbour);
                    }

                }


            }
        }

        yield return null;
        if (pathSucess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }

        requestManager.FinishedProcessingPath(waypoints, pathSucess);


    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
       Vector3[] waypoints = SimplifyPath(path);

        Array.Reverse(waypoints);

        return waypoints;

    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }

            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);

        return 14 * dstX + 10 * (dstY - dstX);
    }

}
