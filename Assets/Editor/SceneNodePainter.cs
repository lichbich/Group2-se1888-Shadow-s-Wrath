#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Pathfinding;
using System.Collections.Generic;
/// <summary>
/// Scene tool to toggle / paint A* nodes in the Scene view.
/// Open via Window -> A* Node Painter
/// Added: save picked points to a ScriptableObject and resolve nearest node indices.
/// </summary>

public class SceneNodePainter : EditorWindow
{
    bool enabledInScene = true;
    float pickRadius = 0.1f;   // visual radius for click
    float brushRadius = 0.5f;  // area painting radius (world units)
    bool showHandles = true;

    // Keep last pick(s) for UI review
    List<Vector3> pickedPoints = new List<Vector3>();

    // Asset target for saving picks
    AstarNodePickAsset saveAsset;

    // UI option: don't forcibly disable scan on startup any more
    bool disableAutoRescan = false;

    [MenuItem("Window/A* Node Painter")]
    public static void OpenWindow()
    {
        GetWindow<SceneNodePainter>("A* Node Painter");
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;

        // Do not force-disable scanOnStartup any more.
        // If user wants to disable or restore it they can use the UI below.
        if (saveAsset == null)
        {
            saveAsset = FindFirstAssetOfType<AstarNodePickAsset>();
        }
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("A* Node Painter", EditorStyles.boldLabel);
        enabledInScene = EditorGUILayout.Toggle("Enable Scene Clicks", enabledInScene);
        showHandles = EditorGUILayout.Toggle("Show Handles", showHandles);
        pickRadius = EditorGUILayout.FloatField("Pick Radius", pickRadius);
        brushRadius = EditorGUILayout.FloatField("Brush Radius", brushRadius);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Left click: toggle nearest node.\nCtrl+Click: set area walkable.\nShift+Click: set area unwalkable.", MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Picks")) pickedPoints.Clear();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Scan-on-load controls (restored)
        EditorGUILayout.LabelField("Auto-Rescan Controls", EditorStyles.boldLabel);
        disableAutoRescan = EditorGUILayout.Toggle("Disable Auto Rescan On Startup", disableAutoRescan);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply Auto-Rescan Setting"))
        {
            if (AstarPath.active != null)
            {
                AstarPath.active.scanOnStartup = !disableAutoRescan;
                EditorUtility.DisplayDialog("A* Node Painter", $"AstarPath.active.scanOnStartup set to {AstarPath.active.scanOnStartup}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("A* Node Painter", "AstarPath.active not found in scene. Add an AstarPath component and try again.", "OK");
            }
        }
        if (GUILayout.Button("Restore Scan On Startup"))
        {
            if (AstarPath.active != null)
            {
                AstarPath.active.scanOnStartup = true;
                disableAutoRescan = false;
                EditorUtility.DisplayDialog("A* Node Painter", "AstarPath.active.scanOnStartup set to true", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("A* Node Painter", "AstarPath.active not found in scene. Add an AstarPath component and try again.", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Save / Resolve UI
        EditorGUILayout.LabelField("Save / Export", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        saveAsset = (AstarNodePickAsset)EditorGUILayout.ObjectField("Target Asset", saveAsset, typeof(AstarNodePickAsset), false);
        if (GUILayout.Button("Auto Assign", GUILayout.Width(80)))
        {
            saveAsset = FindFirstAssetOfType<AstarNodePickAsset>();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Picks As Asset"))
        {
            SavePicksToAsset();
        }
        if (GUILayout.Button("Create New Asset & Save"))
        {
            var path = EditorUtility.SaveFilePanelInProject("Create NodePickAsset", "NodePickAsset", "asset", "Choose where to create the NodePickAsset");
            if (!string.IsNullOrEmpty(path))
            {
                var asset = ScriptableObject.CreateInstance<AstarNodePickAsset>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                saveAsset = asset;
                SavePicksToAsset();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Resolve Picks -> Node Indices"))
        {
            ResolvePicksToNodeIndices();
        }
        if (GUILayout.Button("Clear Saved Indices"))
        {
            if (saveAsset != null)
            {
                saveAsset.nodeIndices = new int[0];
                EditorUtility.SetDirty(saveAsset);
                AssetDatabase.SaveAssets();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Notes:\n- It's safest to store world positions and resolve to nearest nodes at runtime.\n- Node indices can be saved for quick lookup but may become invalid if graphs are rebuilt.", MessageType.Info);

        EditorGUILayout.Space();
        if (GUILayout.Button("Scan Graph (AstarPath.active.Scan)"))
        {
            if (AstarPath.active != null)
            {
                AstarPath.active.Scan();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Picked Points: " + pickedPoints.Count);
        // show a small list of last picks
        for (int i = Mathf.Max(0, pickedPoints.Count - 10); i < pickedPoints.Count; i++)
        {
            EditorGUILayout.Vector3Field($"[{i}]", pickedPoints[i]);
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!enabledInScene) return;

        Event e = Event.current;

        // Only handle left mouse button down (not dragging UI)
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            // For 2D scenes with orthographic camera, get world point directly and project to Z=0
            Vector3 worldPoint = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            worldPoint.z = 0f; // Assuming 2D graph in XY plane at Z=0

            // Visual feedback
            if (showHandles)
            {
                Handles.color = Color.yellow;
                Handles.DrawSolidDisc(worldPoint, Vector3.forward, Mathf.Max(0.05f, pickRadius));
            }

            // Ctrl = make walkable area
            if (e.control)
            {
                ApplyAreaWalkability(worldPoint, brushRadius, true);
            }
            // Shift = make unwalkable area
            else if (e.shift)
            {
                ApplyAreaWalkability(worldPoint, brushRadius, false);
            }
            // Default = toggle nearest node
            else
            {
                ToggleNearestNode(worldPoint);
            }

            // record pick
            RegisterPickedPoint(worldPoint);

            // consume event so selection doesn't change
            e.Use();
            // repaint to update graph visualization
            SceneView.RepaintAll();
        }
    }

    void ToggleNearestNode(Vector3 worldPoint)
    {
        if (AstarPath.active == null) return;

        // Use work item to modify nodes thread-safely
        Vector3 p = worldPoint;

        // Use the void Action overload (do not return a value)
        AstarPath.active.AddWorkItem(() =>
        {
            var nn = AstarPath.active.GetNearest(p);
            var node = nn.node;
            if (node != null)
            {
                node.Walkable = !node.Walkable;
                node.SetConnectivityDirty();

                // Update the graph around the node to ensure walkability changes take effect
                var bounds = new Bounds((Vector3)node.position, Vector3.one * 0.1f); // Small bounds around the node
                AstarPath.active.UpdateGraphs(bounds);
            }
            else
            {
                Debug.Log($"No node found at position {p}. Ensure the graph is scanned and nodes exist near this point.");
            }
        });
    }

    void ApplyAreaWalkability(Vector3 center, float radius, bool walkable)
    {
        if (AstarPath.active == null) return;

        // GraphUpdateObject is the efficient way to mark an area walkable/unwalkable
        var bounds = new Bounds(center, Vector3.one * Mathf.Max(0.001f, radius * 2f));

        var guo = new GraphUpdateObject(bounds)
        {
            updatePhysics = false,
            // correct properties: modifyWalkability + setWalkability
            modifyWalkability = true,
            setWalkability = walkable
        };

        // Queue the update (no full scan required)
        AstarPath.active.UpdateGraphs(guo);
    }

    void RegisterPickedPoint(Vector3 p)
    {
        pickedPoints.Add(p);
    }

    private bool IsGraphEmpty(NavGraph graph)
    {
        bool hasNode = false;
        graph.GetNodes(_ => { hasNode = true; return false; });
        return !hasNode;
    }

    // --- Save / Resolve helpers ---

    void SavePicksToAsset()
    {
        if (saveAsset == null)
        {
            EditorUtility.DisplayDialog("No target asset", "Please assign or create a NodePickAsset to save picks into.", "OK");
            return;
        }

        saveAsset.points = pickedPoints.ToArray();
        // Do not overwrite indices unless they already exist; let user resolve explicitly
        if (saveAsset.nodeIndices == null) saveAsset.nodeIndices = new int[0];
        EditorUtility.SetDirty(saveAsset);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"Saved {pickedPoints.Count} picks to {AssetDatabase.GetAssetPath(saveAsset)}");
    }

    void ResolvePicksToNodeIndices()
    {
        if (saveAsset == null)
        {
            EditorUtility.DisplayDialog("No target asset", "Please assign a NodePickAsset first.", "OK");
            return;
        }

        if (AstarPath.active == null)
        {
            EditorUtility.DisplayDialog("No AstarPath", "AstarPath.active is null. Ensure the A* Path object is present and initialized.", "OK");
            return;
        }

        if (saveAsset.points == null || saveAsset.points.Length == 0)
        {
            EditorUtility.DisplayDialog("No saved points", "The asset has no saved points. Save picks first.", "OK");
            return;
        }

        var indices = new int[saveAsset.points.Length];
        for (int i = 0; i < saveAsset.points.Length; i++)
        {
            var nn = AstarPath.active.GetNearest(saveAsset.points[i]);
            var n = nn.node;
            indices[i] = n != null ? n.NodeIndex : -1;
        }

        saveAsset.nodeIndices = indices;
        EditorUtility.SetDirty(saveAsset);
        AssetDatabase.SaveAssets();
        Debug.Log($"Resolved {indices.Length} picks to node indices and saved into {AssetDatabase.GetAssetPath(saveAsset)}");
    }

    private T FindFirstAssetOfType<T>() where T : UnityEngine.Object
    {
        // Editor-only API - safe because this file is under #if UNITY_EDITOR
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        if (guids.Length == 0) return null;
        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
#endif