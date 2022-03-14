using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateVoiceMap(int mapWidth, int mapHeight, float scale)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                float sampleX = i / scale;
                float sampleY = j /scale;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[i, j] = perlinValue;
            }
        }

        return noiseMap;
    }
}
