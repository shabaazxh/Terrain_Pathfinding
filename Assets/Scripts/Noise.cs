using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacuranity, Vector2 offset)
    {

        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float[,] noiseMap = new float[mapWidth, mapHeight];

        Vector2 offsets;

        // We want to be able to generate a repeatable map: Seed can help us do this
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Get half dimensions in order to prevent zoom into the top right corner and instead zoom into centre
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1; // Higher the frequency -> More further apart sample points, height values change more rapidly
                float currentHeight = 0;

                // Octaves
                for(int i = 0; i < octaves; i++)
                {
                    // from where do we want to sample our height map from?
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // Put in range -1:1
                    currentHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacuranity;
                }

                // Keep track of highest and lowest height in order to normalize
                if(currentHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = currentHeight;
                }
                else if(currentHeight < minNoiseHeight)
                {
                    minNoiseHeight = currentHeight;
                }

                noiseMap[x, y] = currentHeight;
            }
        }

        // Normalize noise map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]); // return between 0-1 = normalized
            }
        }

       return noiseMap;
    }
}
