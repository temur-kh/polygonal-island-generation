using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// References: 
// https://github.com/SebLague/Procedural-Landmass-Generation
// https://www.youtube.com/playlist?list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3
public class ElevationMapGenerator
{
    public static ElevationMap generate(Config conf)
    {
        Vector2 center = Vector2.zero;
        float[,] values = generateNoiseMap(conf.meshSize, conf.meshSize, center, conf);

        float[,] falloff = new float[0, 0];
        falloff = generateFalloffMap(conf.meshSize, conf.falloffCurve);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < conf.meshSize; i++)
        {
            for (int j = 0; j < conf.meshSize; j++)
            {
                values[i, j] -= falloff[i, j];
                values[i, j] *= conf.heightCurve.Evaluate(values[i, j]) * conf.heightMultiplier;
                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        return new ElevationMap(values, minValue, maxValue);
    }

    static float[,] generateNoiseMap(int width, int height, Vector2 center, Config conf)
    {
        float[,] noiseMap = new float[width, height];

        System.Random prng = new System.Random(conf.seed);
        Vector2[] octaveOffsets = new Vector2[conf.octaves];

        for (int i = 0; i < conf.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + conf.offset.x + center.x;
            float offsetY = prng.Next(-100000, 100000) - conf.offset.y - center.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < conf.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / conf.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / conf.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= conf.persistance;
                    frequency *= conf.lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    static float[,] generateFalloffMap(int size, AnimationCurve falloffCurve)
    {
        float[,] map = new float[size, size];

        var center = size / 2f;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var distance = Vector2.Distance(new Vector2(i, j), new Vector2(center, center)) / size / 0.5f;
                map[i, j] = falloffCurve.Evaluate(Mathf.Clamp01(distance));
            }
        }

        return map;
    }
}

public struct ElevationMap
{
    public float[,] values;
    public float minValue;
    public float maxValue;

    public ElevationMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}
