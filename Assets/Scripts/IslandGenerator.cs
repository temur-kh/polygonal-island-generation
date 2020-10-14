using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IslandGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int seed = 42;

    [Header("Mesh Settings")]
    public int meshSize = 200;

    [Header("Voronoi Generation")]
    public int pointSpacing = 3;
    public int relaxationIterations = 1;

    [Header("Elevation")]
    public float scale = 50;
    public int octaves = 5;
    [Range(0, 1)]
    public float persistance = .43f;
    public float lacunarity = 2;
    public Vector2 offset = new Vector2(0, 0);
    public AnimationCurve heightCurve;
    public AnimationCurve falloffCurve;
    public float heightMultiplier = 60;

    [Header("Moisture")]
    [Range(0, 1)]
    public float moisture = 1.0f;

    [Header("Texture Settings")]
    public int textureSize = 2048;

    [Header("Evaluation")]
    public float beachScoreCoefA = 3.0f;
    public float beachScoreCoefB = 0.85f;
    public AnimationCurve beachFunction;
    public float mountainScoreCoefA = 5.0f;
    public float mountainScoreCoefB = 0.15f;
    public AnimationCurve mountainFunction;
    public float lowlandScoreCoefA = 3.0f;
    public float lowlandScoreCoefB = 0.6f;
    public AnimationCurve lowlandFunction;

    [Header("Outputs")]
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public void Start()
    {
        Config conf = getConfig();
        generateMap(conf);
    }

    void OnValidate()
    {
        Start();
    }

    public Config getConfig()
    {
        Config conf = new Config();
        conf.seed = seed;
        conf.meshSize = meshSize;
        conf.pointSpacing = pointSpacing;
        conf.relaxationIterations = relaxationIterations;
        conf.scale = scale;
        conf.octaves = octaves;
        conf.persistance = persistance;
        conf.lacunarity = 2;
        conf.offset = offset;
        conf.heightCurve = heightCurve;
        conf.falloffCurve = falloffCurve;
        conf.heightMultiplier = heightMultiplier;
        conf.moisture = moisture;
        conf.textureSize = textureSize;
        conf.beachScoreCoefA = beachScoreCoefA;
        conf.beachScoreCoefB = beachScoreCoefB;
        conf.mountainScoreCoefA = mountainScoreCoefA;
        conf.mountainScoreCoefB = mountainScoreCoefB;
        conf.lowlandScoreCoefA = lowlandScoreCoefA;
        conf.lowlandScoreCoefB = lowlandScoreCoefB;
        return conf;
    }

	public void generateMap(Config conf)
    {
        Random.InitState(conf.seed);

        var points = PointsGenerator.getPoints(conf);
        var voronoi = new Delaunay.Voronoi(points, null, new Rect(0, 0, conf.meshSize, conf.meshSize), conf.relaxationIterations);

        var elevationMap = ElevationMapGenerator.generate(conf);

        var mapGraph = new MapGraph(voronoi, elevationMap);

        BiomeMapGenerator.generate(mapGraph, conf);

        updateMesh(MapMeshGenerator.generate(mapGraph, meshSize));
        updateTexture(MapTextureGenerator.generate(mapGraph, conf));

        EvaluationResults evaluation = Evaluation.assessmentResultsToString(mapGraph, conf);
        beachFunction = evaluation.beachFunction;
        mountainFunction = evaluation.mountainFunction;
        lowlandFunction = evaluation.lowlandFunction;
        Debug.Log(evaluation.result);
    }

    private void updateMesh(Mesh mesh)
    {
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private void updateTexture(Texture2D texture)
    {
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
