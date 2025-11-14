using UnityEngine;
using Pathfinding;

[CreateAssetMenu(menuName = "A* / Node Pick Asset", fileName = "NodePickAsset")]
public class AstarNodePickAsset : ScriptableObject
{
    public Vector3[] points;
    public int[] nodeIndices;

    // Simple runtime helper - resolves saved world positions to nearest nodes
    public GraphNode[] ResolveNodesAtRuntime()
    {
        if (AstarPath.active == null || points == null) return new GraphNode[0];
        var nodes = new GraphNode[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            var nn = AstarPath.active.GetNearest(points[i]);
            nodes[i] = nn.node;
        }
        return nodes;
    }
}