using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridA : MonoBehaviour
{
    public bool displayGridGizmos;

    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] grid;

    public TerrainType[] walkableRegions;
    LayerMask walkableMask;
    Dictionary<float, int> walkableRegionsDictionary = new Dictionary<float, int>();

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    public GameObject G_mapGen;
    MapGenerator mapGen;

    private float[,] perlineNoise;


    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        mapGen = G_mapGen.GetComponent<MapGenerator>();
        walkableRegions = mapGen.regions;
        mapGen.GenerateMap();
        perlineNoise = mapGen.noiseMap;

        foreach (TerrainType region in walkableRegions)
        {
            //walkableMask.value += region.height;
            walkableRegionsDictionary.Add(region.height, region.terrainPenalty);
        }


        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for(int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                // Collision check for each point
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                

                int movementPenalt = 0;

                if (walkable)
                {
                    float height = perlineNoise[x, y];
                    float dp = Mathf.Round(height * 10) / 10;

                    try
                    {
                        walkableRegionsDictionary.TryGetValue(dp, out movementPenalt);
                        int value = walkableRegionsDictionary[dp];
                        Debug.Log(value);
                    }
                    catch (KeyNotFoundException) { }
 
                }

                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalt);
            }
        }

    }

    public List<Node> GetNeightbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for(int x = -1; x <=1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if(checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;

    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        // How far along the grid it is in percentage
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        //x,y index
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];

    }
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
            if (grid != null && displayGridGizmos)
            {
                foreach (Node n in grid)
                {
                        
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                }
            }
    }
}
