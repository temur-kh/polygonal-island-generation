using UnityEngine;
using UnityEditor;
using Delaunay;
using System.Collections.Generic;
using System.Linq;
using System;
using Delaunay.Geo;

public class MapGraph
{
    public HashSet<Vector3> points;
    public HashSet<Node> nodes;
    public List<NodeHalfEdge> edges;

    private ElevationMap elevationMap;

    public MapGraph(Voronoi voronoi, ElevationMap elevationMap)
    {
        this.elevationMap = elevationMap;
        generateGraph(voronoi);
    }

    private void generateGraph(Voronoi voronoi)
    {
        points = new HashSet<Vector3>();
        nodes = new HashSet<Node>();
        edges = new List<NodeHalfEdge>();

        var edgesByStartPosition = new Dictionary<Vector3, List<NodeHalfEdge>>();
        var siteEdges = new Dictionary<Vector2, List<LineSegment>>();

        foreach (var edge in voronoi.Edges())
        {
            if (edge.visible)
            {
                var p1 = edge.clippedEnds[Delaunay.LR.Side.LEFT];
                var p2 = edge.clippedEnds[Delaunay.LR.Side.RIGHT];
                var segment = new LineSegment(p1, p2);

                if (Vector2.Distance(p1.Value, p2.Value) < 0.001f)
                {
                    continue;
                }

                if (edge.leftSite != null)
                {
                    if (!siteEdges.ContainsKey(edge.leftSite.Coord)) siteEdges.Add(edge.leftSite.Coord, new List<LineSegment>());
                    siteEdges[edge.leftSite.Coord].Add(segment);
                }
                if (edge.rightSite != null)
                {
                    if (!siteEdges.ContainsKey(edge.rightSite.Coord)) siteEdges.Add(edge.rightSite.Coord, new List<LineSegment>());
                    siteEdges[edge.rightSite.Coord].Add(segment);
                }
            }
        }

        foreach (var site in voronoi.SiteCoords())
        {
            var boundries = getBoundriesForSite(siteEdges, site);
            var center = getPointWithElevation(site);
            var currentNode = new Node { centerPoint = center };
            nodes.Add(currentNode);

            NodeHalfEdge firstEdge = null;
            NodeHalfEdge previousEdge = null;

            for (var i = 0; i < boundries.Count; i++)
            {
                var edge = boundries[i];

                var start = getPointWithElevation(edge.p0.Value);
                var end = getPointWithElevation(edge.p1.Value);
                if (start == end) continue;

                previousEdge = addEdge(edgesByStartPosition, previousEdge, start, end, currentNode);
                if (firstEdge == null) firstEdge = previousEdge;
                if (currentNode.startEdge == null) currentNode.startEdge = previousEdge;

                // figure out if the two edges meet, and if not then insert some more edges to close the polygon
                if (i < boundries.Count - 1)
                {
                    start = getPointWithElevation(edge.p1.Value);
                    end = getPointWithElevation(boundries[i + 1].p0.Value);
                }
                else if (i == boundries.Count - 1)
                {
                    start = getPointWithElevation(edge.p1.Value);
                    end = getPointWithElevation(boundries[0].p0.Value);
                }
                if (start != end)
                    previousEdge = addEdge(edgesByStartPosition, previousEdge, previousEdge.destination, end, currentNode);
            }
            // Connect up the end of the loop
            previousEdge.next = firstEdge;
            firstEdge.previous = previousEdge;
        }

        connectOpposites(edgesByStartPosition);
    }

    private List<LineSegment> getBoundriesForSite(Dictionary<Vector2, List<LineSegment>> siteEdges, Vector2 site)
    {
        var boundries = siteEdges[site];

        // Flip boundries clockwise
        for (int i = 0; i < boundries.Count; i++)
        {
            var firstVector = boundries[i].p0.Value - site;
            var secondVector = boundries[i].p1.Value - site;
            var angle = Vector2.SignedAngle(firstVector, secondVector);

            if (angle > 0) boundries[i] = new LineSegment(boundries[i].p1, boundries[i].p0);
            else boundries[i] = new LineSegment(boundries[i].p0, boundries[i].p1);
        }

        // Sort boundaries clockwise
        boundries.Sort((line1, line2) =>
        {
            var firstVector = line1.p0.Value - site;
            var secondVector = line2.p0.Value - site;
            var angle = Vector2.SignedAngle(firstVector, secondVector);

            return angle > 0 ? 1 : (angle < 0 ? -1 : 0);
        });

        return boundries;
    }

    private void connectOpposites(Dictionary<Vector3, List<NodeHalfEdge>> edgesByStartPosition)
    {
        foreach (var edge in edges)
        {
            if (edge.opposite == null)
            {
                var startEdgePosition = edge.previous.destination;
                var endEdgePosition = edge.destination;

                if (edgesByStartPosition.ContainsKey(endEdgePosition))
                {
                    foreach (var item in edgesByStartPosition[endEdgePosition])
                    {
                        if (startEdgePosition == item.destination)
                        {
                            edge.opposite = item;
                            item.opposite = edge;
                            break;
                        }
                    }
                }
            }
        }
    }

    private NodeHalfEdge addEdge(Dictionary<Vector3, List<NodeHalfEdge>> edgesByStartPosition, NodeHalfEdge previous, Vector3 start, Vector3 end, Node node)
    {
        var currentEdge = new NodeHalfEdge { node = node };

        if (!points.Contains(start)) points.Add(start);
        if (!points.Contains(end)) points.Add(end);

        currentEdge.destination = end;

        if (!edgesByStartPosition.ContainsKey(start)) edgesByStartPosition.Add(start, new List<NodeHalfEdge>());
        edgesByStartPosition[start].Add(currentEdge);

        edges.Add(currentEdge);

        if (previous != null)
        {
            previous.next = currentEdge;
            currentEdge.previous = previous;
        }
        return currentEdge;
    }

    private Vector3 getPointWithElevation(Vector2 vector)
    {
        Vector3 point = new Vector3(Mathf.Round(vector.x * 1000f) / 1000f, 0, Mathf.Round(vector.y * 1000f) / 1000f);
        var x = Mathf.FloorToInt(point.x);
        var y = Mathf.FloorToInt(point.z);
        if (x >= 0 && y >= 0 && x < elevationMap.values.GetLength(0) && y < elevationMap.values.GetLength(1))
        {
            point.y = elevationMap.values[x, y];
        }
        return point;
    }
}