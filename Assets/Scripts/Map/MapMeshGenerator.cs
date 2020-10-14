using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public static class MapMeshGenerator
{
    public static Mesh generate(MapGraph mapGraph, int meshSize)
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();

        foreach(var node in mapGraph.nodes)
        {
            vertices.Add(node.centerPoint);
            var centerIndex = vertices.Count - 1;
            var edges = node.GetEdges().ToList();

            int lastIndex = 0;
            int firstIndex = 0;
            for (var i = 0; i < edges.Count(); i++)
            {
                if (i == 0)
                {
                    vertices.Add(edges[i].previous.destination);
                    var i2 = vertices.Count - 1;
                    vertices.Add(edges[i].destination);
                    var i3 = vertices.Count - 1;
                    addTriangle(indices, centerIndex, i2, i3);
                    firstIndex = i2;
                    lastIndex = i3;
                }
                else if (i < edges.Count() -1)
                {
                    vertices.Add(edges[i].destination);
                    var currentIndex = vertices.Count - 1;
                    addTriangle(indices, centerIndex, lastIndex, currentIndex);
                    lastIndex = currentIndex;
                } 
                else
                {
                    addTriangle(indices, centerIndex, lastIndex, firstIndex);
                }
            }
        }

        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / meshSize, vertices[i].z / meshSize);
        }

        var mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = indices.ToArray(),
            uv = uvs
        };

        return mesh;
    }

    private static void addTriangle(List<int> indices, int v1, int v2, int v3)
    {
        indices.Add(v1);
        indices.Add(v2);
        indices.Add(v3);
    }

    
}