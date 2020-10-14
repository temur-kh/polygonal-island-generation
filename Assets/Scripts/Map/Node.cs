using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Node
{
    private float? _heightDifference;
    private Rect? _boundingRectangle;

    public Vector3 centerPoint;
    // An arbitrary half-edge that borders on this map node (face)
    public NodeHalfEdge startEdge;

    public NodeType nodeType;

    public IEnumerable<NodeHalfEdge> GetEdges()
    {
        yield return startEdge;

        var next = startEdge.next;
        while(next != startEdge)
        {
            yield return next;
            next = next.next;
        }
    }

    public IEnumerable<Vector3> GetCorners()
    {
        yield return startEdge.destination;

        var next = startEdge.next;
        while (next != startEdge)
        {
            yield return next.destination;
            next = next.next;
        }
    }

    public float GetElevation()
    {
        return centerPoint.y;
    }

    public float GetHeightDifference()
    {
        if (!_heightDifference.HasValue)
        {
            var lowestY = centerPoint.y;
            var highestY = centerPoint.y;
            foreach(var corner in GetCorners())
            {
                if (corner.y > highestY) highestY = corner.y;
                if (corner.y < lowestY) lowestY = corner.y;
            }
            _heightDifference = highestY - lowestY;
        }
        return _heightDifference.Value;
    }

    public List<Node> GetNeighborNodes()
    {
        return GetEdges().Where(x => x.opposite != null && x.opposite.node != null).Select(x => x.opposite.node).ToList();
    }
}