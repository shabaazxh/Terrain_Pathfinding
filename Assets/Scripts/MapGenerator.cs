using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode { NoiseMap, ColourMap, Mesh, SpawnAssets };
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public bool autoUpdate;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacuranity;

    public int seed;
    public Vector2 offset;

    public TerrainType[] regions;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public Asset[] AssetsToSpawn;

    public float[,] noiseMap;
    private MapDisplay display;

    [Range(0, 100)]
    public float rayLength = 100f;


    private List<Vector3> spawnedLocations = new List<Vector3>();

    public void GenerateMap()
    {
        // Get noise map 
        noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacuranity, offset);

        Color[] colourMap = new Color[mapWidth * mapHeight];

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];

                // Which region does this current height fall in?
                for(int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        display = FindObjectOfType<MapDisplay>();


        // Which draw mode to utilise 
        if(drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));

        } else if(drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
        else if(drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
        else if(drawMode == DrawMode.SpawnAssets)
        {
           SpawnAsset();
        }

    }

    float EuclideanDistance(Vector3 p1, Vector3 p2)
    {
        return Mathf.Sqrt(
            Mathf.Pow((p2.x - p1.x), 2) +
            Mathf.Pow((p2.y - p1.y), 2) +
            Mathf.Pow((p2.z - p1.z), 2));
    }

    // Spawn assets that should appear across the world
    public void SpawnWorldAssets()
    {
        var lastNoiseHeight = 0f;
        Vector3[] verts = display.meshFilter.sharedMesh.vertices;
        var mesh_scale = 1;
        int range = 20;
        for(int p = 0; p < AssetsToSpawn.Length; p++)
        {
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 worldPt = transform.TransformPoint(verts[i]);
                var noiseHeight = worldPt.y;
                //Debug.Log(noiseHeight);
                if (Mathf.Abs(lastNoiseHeight - noiseHeight) < 25)
                {
                    if (noiseHeight > 1)
                    {
                        if (Random.Range(1, range) == 1)
                        {
                            var spawnAbove = noiseHeight * 1;
                            Vector3 pos = new Vector3(verts[i].x * mesh_scale, spawnAbove, verts[i].z * mesh_scale);
                            var obj = Instantiate(AssetsToSpawn[p].AssetPrefab, pos, Quaternion.identity);
                            obj.transform.SetParent(this.transform);
                            range += 10;
                        }
                    }
                }
                lastNoiseHeight = noiseHeight;
            }
        } 
    }

    private void AnimateMesh()
    {
        Vector3[] verts = display.meshFilter.sharedMesh.vertices;
        float waveSpeed = 1f;
        float waveHeight = 5f;
        for(int i = 0; i < verts.Length; i++)
        {
            Vector3 worldPt = transform.TransformPoint(verts[i]);
            var noiseHeight = worldPt.y;
            //Debug.Log(noiseHeight);
            if (noiseHeight <= 0.28f)
            {
                float px = (verts[i].x * noiseScale) + (Time.timeSinceLevelLoad * waveSpeed);
                float pz = (verts[i].z * noiseScale) + (Time.timeSinceLevelLoad * waveSpeed);

                verts[i].y = (Mathf.PerlinNoise(px, pz) - 0.5f) * waveHeight;
            }
        }

        display.meshFilter.sharedMesh.vertices = verts;
    }

    // Spawn at rocks region layer
    public void SpawnAsset()
    {
        Vector3[] verts = display.meshFilter.sharedMesh.vertices;
        for (int p = 0; p < AssetsToSpawn.Length; p++)
        {
            for (int i = 0; i < (verts.Length / 4) / 2; i++)
            {

                Vector3 worldPt = transform.TransformPoint(verts[i]);
                var noiseHeight = worldPt.y;

                // Randomise change of this object spawning to prevent spawn on every vertex
                if (Random.Range(1, 100) == 1)
                {
                    if (noiseHeight >= 4.0f && noiseHeight <= 6.0f)
                    {
                        // Debug.Log(noiseHeight);
                        Instantiate(AssetsToSpawn[0].AssetPrefab, worldPt, Quaternion.identity);
                    }

                    if (noiseHeight >= 1.05f && noiseHeight <= 2.2f)
                    {
                        Instantiate(AssetsToSpawn[1].AssetPrefab, worldPt, Quaternion.identity);
                    }

                    


                    /*                    if (noiseHeight >= 0.18f && noiseHeight <= 1.0f)
                                        {
                                            Instantiate(AssetsToSpawn[2].AssetPrefab, worldPt, Quaternion.identity);
                                        }*/

                }
            }
        }
    }

    // Clamp properties 
    void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }

        if(mapHeight < 1)
        {
            mapHeight = 1;
        }

        if(lacuranity < 1)
        {
            lacuranity = 1;
        }

        if(octaves < 0)
        {
            octaves = 0;
        }

    }


    private void Update()
    {
        //AnimateMesh();
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
    public int terrainPenalty;
}

[System.Serializable]
public struct Asset
{
    public string name;
    public GameObject AssetPrefab;
    public int AmountToSpawn;
    public float findHeight;
}
