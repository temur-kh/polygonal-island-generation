using System;
using System.Linq;
using UnityEngine;

public class NodeHalfEdge
{
    public Vector3 destination;
    // The next half-edge that shares the same map node (face)
    public NodeHalfEdge next;
    // The previous half-edge that shares the same map node (face)
    public NodeHalfEdge previous;
    // The other half of this half-edge, with a different map node (face)
    public NodeHalfEdge opposite;
    // The map node that this edge borders on
    public Node node;
}