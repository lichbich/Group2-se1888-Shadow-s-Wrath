using System;
using System.Reflection;
using UnityEngine;
using Pathfinding;

/// <summary>
/// Apply a saved NodePick asset to the A* graph:
/// - resolves saved world positions to nearest nodes at runtime/editor time
/// - sets node.Walkable (or other action)
/// - updates the graph in the area of the picks
/// Attach to any GameObject and assign your NodePick asset (any ScriptableObject that contains a public Vector3[] points).
/// Use the context menu "Apply Picks Now" in the inspector or enable ApplyOnStart.
/// </summary>
[ExecuteAlways]
public class NodePickApplier : MonoBehaviour
{
    [Tooltip("Assign the NodePick asset here. The asset can be any ScriptableObject that exposes a Vector3[] named 'points' (public field or property).")]
    public UnityEngine.Object picksAsset;

    public bool applyOnStart = false;
    public bool setWalkable = false; // the action to apply to resolved nodes
    public float updateMargin = 0.25f; // extra padding when updating graph bounds

    void Start()
    {
        if (Application.isPlaying && applyOnStart)
        {
            ApplyPicks();
        }
    }

    [ContextMenu("Apply Picks Now")]
    public void ApplyPicks()
    {
        if (picksAsset == null)
        {
            Debug.LogWarning("NodePickApplier: no picks asset assigned.");
            return;
        }

        var pts = GetPointsFromAsset(picksAsset);
        if (pts == null || pts.Length == 0)
        {
            Debug.LogWarning("NodePickApplier: could not read points from the assigned asset or asset contains no points.");
            return;
        }

        if (AstarPath.active == null)
        {
            Debug.LogWarning("NodePickApplier: AstarPath.active is null. Make sure an AstarPath component exists in the scene.");
            return;
        }

        // Capture points locally for use inside the work item
        Vector3[] localPoints = (Vector3[])pts.Clone();

        // Run the edit on the pathfinding work item to be thread-safe
        AstarPath.active.AddWorkItem(() =>
        {
            Bounds bounds = new Bounds((Vector3)localPoints[0], Vector3.zero);
            for (int i = 0; i < localPoints.Length; i++)
            {
                var nn = AstarPath.active.GetNearest(localPoints[i]);
                var node = nn.node;
                if (node == null) continue;

                // Example action: set walkability
                node.Walkable = setWalkable;
                node.SetConnectivityDirty();

                // Expand bounds to cover node position
                bounds.Encapsulate((Vector3)node.position);
            }

            // Apply a small margin and update graphs in that area so the change takes effect
            bounds.Expand(updateMargin * 2f);
            AstarPath.active.UpdateGraphs(bounds);
        });

        Debug.Log($"NodePickApplier: requested apply of {localPoints.Length} picks (setWalkable={setWalkable}).");
    }

    // Reflection helper - tries to extract a Vector3[] from a field or property called "points"
    Vector3[] GetPointsFromAsset(UnityEngine.Object asset)
    {
        if (asset == null) return null;
        var type = asset.GetType();

        // Try public field "points"
        var field = type.GetField("points", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && field.FieldType == typeof(Vector3[]))
        {
            return field.GetValue(asset) as Vector3[];
        }

        // Try public property "points"
        var prop = type.GetProperty("points", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null && prop.PropertyType == typeof(Vector3[]))
        {
            try
            {
                return prop.GetValue(asset, null) as Vector3[];
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"NodePickApplier: failed to read 'points' property via reflection: {ex.Message}");
                return null;
            }
        }

#if UNITY_EDITOR
        // Editor-only fallback: use SerializedObject to read points if reflection fails and we're in the editor.
        // This helps if the field is private/serialized but not exposed as a direct field/property.
        var so = new UnityEditor.SerializedObject(asset);
        var sp = so.FindProperty("points");
        if (sp != null && sp.isArray)
        {
            var list = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < sp.arraySize; i++)
            {
                var elem = sp.GetArrayElementAtIndex(i);
                if (elem != null)
                {
                    // SerializedProperty of Vector3 is not directly convertible; use vector3Value
                    list.Add(elem.vector3Value);
                }
            }
            return list.ToArray();
        }
#endif

        Debug.LogWarning($"NodePickApplier: assigned asset ({type.FullName}) does not expose a Vector3[] named 'points' (field or property).");
        return null;
    }
}