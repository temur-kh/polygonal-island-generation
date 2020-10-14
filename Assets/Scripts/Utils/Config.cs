using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Config
{
    public int seed;

    public int meshSize;

    public int pointSpacing;
    public int relaxationIterations;

    public float scale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public Vector2 offset;
    public AnimationCurve heightCurve;
    public AnimationCurve falloffCurve;
    public float heightMultiplier;

    public float moisture;

    public int textureSize;

    public float beachScoreCoefA;
    public float beachScoreCoefB;
    public float mountainScoreCoefA;
    public float mountainScoreCoefB;
    public float lowlandScoreCoefA;
    public float lowlandScoreCoefB;
}