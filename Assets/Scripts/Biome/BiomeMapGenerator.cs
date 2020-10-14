using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BiomeMapGenerator
{
    public static void generate(MapGraph graph, Config conf)
    {
        setNodesWithElevationAndMoisture(graph, conf.moisture);
        setLowNodesToWater(graph, 0.2f);

        fillOcean(graph);
        setBeaches(graph);

        averageCenterPoints(graph);
    }

    private static void setNodesWithElevationAndMoisture(MapGraph graph, float moisture)
    {
        foreach (var node in graph.nodes)
        {
            if (node.GetElevation() > 22f && moisture >= 0.5f)
            {
                node.nodeType = NodeType.Snow;
            }
            else if (node.GetElevation() > 22f && moisture >= 0.33f)
            {
                node.nodeType = NodeType.Tundra;
            }
            else if (node.GetElevation() > 22f && moisture >= 0.16f)
            {
                node.nodeType = NodeType.Bare;
            }
            else if (node.GetElevation() > 22f)
            {
                node.nodeType = NodeType.Scorched;
            }
            else if (node.GetElevation() > 12f && moisture >= 0.66f)
            {
                node.nodeType = NodeType.Taiga;
            }
            else if (node.GetElevation() > 12f && moisture >= 0.33f)
            {
                node.nodeType = NodeType.Shrubland;
            }
            else if (node.GetElevation() > 12f)
            {
                node.nodeType = NodeType.TemperateDesert;
            }
            else if (node.GetElevation() > 3f && moisture >= 0.83f)
            {
                node.nodeType = NodeType.TemperateRainForest;
            }
            else if (node.GetElevation() > 3f && moisture >= 0.5f)
            {
                node.nodeType = NodeType.TemperateDeciduousForest;
            }
            else if (node.GetElevation() > 3f && moisture >= 0.16f)
            {
                node.nodeType = NodeType.Grassland;
            }
            else if (node.GetElevation() > 3f)
            {
                node.nodeType = NodeType.TemperateDesert;
            }
            else if (moisture >= 0.66f)
            {
                node.nodeType = NodeType.TropicalRainForest;
            }
            else if (moisture >= 0.33f)
            {
                node.nodeType = NodeType.TropicalSeasonalForest;
            }
            else if (moisture >= 0.16f)
            {
                node.nodeType = NodeType.Grassland;
            }
            else
            {
                node.nodeType = NodeType.SubtropicalDesert;
            }
            
        }
    }

    private static void averageCenterPoints(MapGraph graph)
    {
        foreach (var node in graph.nodes)
        {
            node.centerPoint = new Vector3(node.centerPoint.x, node.GetCorners().Average(x => x.y), node.centerPoint.z);
        }
    }

    private static void fillOcean(MapGraph graph)
    {
        var startNode = graph.nodes.FirstOrDefault(x => x.nodeType == NodeType.FreshWater);
        floodFill(startNode, NodeType.FreshWater, NodeType.SaltWater);
    }

    private static void floodFill(Node node, NodeType targetType, NodeType replacementType)
    {
        if (targetType == replacementType) return;
        if (node.nodeType != targetType) return;
        node.nodeType = replacementType;
        foreach (var neighbor in node.GetNeighborNodes())
        {
            floodFill(neighbor, targetType, replacementType);
        }
    }

    private static void setBeaches(MapGraph graph)
    {
        foreach (var node in graph.nodes)
        {
            if (node.nodeType != NodeType.FreshWater && node.nodeType != NodeType.SaltWater && node.nodeType != NodeType.Error)
            {
                foreach (var neighbor in node.GetNeighborNodes())
                {
                    if (neighbor.nodeType == NodeType.SaltWater)
                    {
                        if (node.GetHeightDifference() < 0.8f)
                        {
                            node.nodeType = NodeType.Beach;
                        }
                        break;
                    }
                }
            }
        }
    }

    private static void setLowNodesToWater(MapGraph graph, float cutoff)
    {
        foreach (var node in graph.nodes)
        {
            if (node.centerPoint.y <= cutoff)
            {
                var allZero = true;
                foreach (var edge in node.GetEdges())
                {
                    if (edge.destination.y > cutoff)
                    {
                        allZero = false;
                        break;
                    }
                }
                if (allZero && node.nodeType != NodeType.Error) node.nodeType = NodeType.FreshWater;
            }
        }
    }
}
