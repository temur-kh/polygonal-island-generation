using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Evaluation
{
    public static EvaluationResults assessmentResultsToString(MapGraph graph, Config conf)
    {
        float beachCoverage = getPercentageOfBeach(graph);
        float beachScore = triangularFunction(beachCoverage, conf.beachScoreCoefA, conf.beachScoreCoefB);

        float mountainCoverage = getPercentageOfHighMountains(graph);
        float mountainScore = triangularFunction(mountainCoverage, conf.mountainScoreCoefA, conf.mountainScoreCoefB);

        float lowlandCoverage = getPercentageOfLowland(graph);
        float lowlandScore = triangularFunction(lowlandCoverage, conf.lowlandScoreCoefA, conf.lowlandScoreCoefB);

        int jointComponents = countDisjointComponents(graph);

        float maxScore = Mathf.Max(beachScore, Mathf.Max(mountainScore, lowlandScore));

        string result = string.Format(
            "Max Score: {6:F2} Number of Components: {7}\n" +
            "Coastline Beach Coverage: {0:F2} Beach Score: {1:F2}\n" +
            "Mountain Cover Percentage: {2:F2} Mountain Score: {3:F2}\n" +
            "Lowland Cover Percentage: {4:F2} Lowland Score: {5:F2}\n", 
            beachCoverage, beachScore, mountainCoverage, mountainScore, 
            lowlandCoverage, lowlandScore, maxScore, jointComponents);

        AnimationCurve beachFunction = getAnimationCurve(conf.beachScoreCoefA, conf.beachScoreCoefB);
        AnimationCurve mountainFunction = getAnimationCurve(conf.mountainScoreCoefA, conf.mountainScoreCoefB);
        AnimationCurve lowlandFunction = getAnimationCurve(conf.lowlandScoreCoefA, conf.lowlandScoreCoefB);

        EvaluationResults evaluation = new EvaluationResults();
        evaluation.result = result;
        evaluation.beachFunction = beachFunction;
        evaluation.mountainFunction = mountainFunction;
        evaluation.lowlandFunction = lowlandFunction;

        return evaluation;
    }

    private static float triangularFunction(float x, float a, float b)
    {
        if (Mathf.Abs(x - b) < 1.0f / a)
        {
            return 1 - a * Mathf.Abs(x - b);
        } else
        {
            return 0.0f;
        }
    }

    private static AnimationCurve getAnimationCurve(float a, float b)
    {
        Keyframe[] keys = new Keyframe[21];
        int i = 0; 
        for (float x = 0.0f; x < 1.05f; x+=0.05f, i++)
        {
            float y = triangularFunction(x, a, b);
            keys[i] = new Keyframe(x, y);
        }
        return new AnimationCurve(keys);
    }

    public static float getPercentageOfBeach(MapGraph graph)
    {
        int total = 0, beach = 0;
        foreach (var node in graph.nodes)
        {
            if (node.nodeType != NodeType.FreshWater && node.nodeType != NodeType.SaltWater && node.nodeType != NodeType.Error)
            {
                foreach (var neighbor in node.GetNeighborNodes())
                {
                    if (neighbor.nodeType == NodeType.SaltWater)
                    {
                        if (node.nodeType == NodeType.Beach)
                        {
                            beach++;
                        }
                        total++;
                        break;
                    }
                }
            }
        }
        return ((float) beach / (float) total);
    }

    public static float getPercentageOfHighMountains(MapGraph graph)
    {
        int total = 0, mountains = 0;
        foreach (var node in graph.nodes)
        {
            if (node.nodeType != NodeType.SaltWater && node.nodeType != NodeType.Error)
            {
                if (node.GetElevation() > 12f) mountains++;
                total++;
            }
        }
        return ((float) mountains / (float) total);
    }

    public static float getPercentageOfLowland(MapGraph graph)
    {
        int total = 0, lowland = 0;
        foreach (var node in graph.nodes)
        {
            if (node.nodeType != NodeType.SaltWater && node.nodeType != NodeType.FreshWater && node.nodeType != NodeType.Error)
            {
                if (node.GetElevation() <= 3f) lowland++;
                total++;
            }
        }
        return ((float) lowland / (float) total);
    }

    public static int countDisjointComponents(MapGraph graph)
    {
        int count = 0;
        List<Node> nodes = new List<Node>();
        nodes.AddRange(graph.nodes);
        Dictionary<Node, bool> visited = new Dictionary<Node, bool>();
        foreach (var node in nodes)
        {
            visited[node] = false;
        }
        foreach (var node in nodes)
        {
            if (!visited[node] && node.nodeType != NodeType.SaltWater && node.nodeType != NodeType.FreshWater && node.nodeType != NodeType.Error)
            {
                count++;
                dfs(node, visited);
            }
        }
        return count;
    }
    
    private static void dfs(Node node, Dictionary<Node, bool> visited)
    {
        visited[node] = true;
        foreach (var next in node.GetNeighborNodes())
        {
            if (!visited[next] && next.nodeType != NodeType.SaltWater && next.nodeType != NodeType.FreshWater && next.nodeType != NodeType.Error)
            {
                dfs(next, visited);
            }
        }
    }
}