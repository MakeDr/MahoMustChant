#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// 마나 흐름 시뮬레이터용 디버그 시각화 에디터
/// </summary>
[CustomEditor(typeof(ManaFlow_Simulate))]
public class ManaFlow_SimulateEditor : Editor
{
    private static bool showGizmos = true;
    private static bool showManaLabels = false;
    private static float gizmoManaCap = 10f;
    private static float gizmoAlpha = 0.3f; // 추가된 투명도 제어

    private ManaFlow_Simulate sim;

    private void OnEnable()
    {
        sim = (ManaFlow_Simulate)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gizmo Settings", EditorStyles.boldLabel);

        showGizmos = EditorGUILayout.Toggle("Show Mana Gizmos", showGizmos);
        if (showGizmos)
        {
            EditorGUI.indentLevel++;
            showManaLabels = EditorGUILayout.Toggle("Show Mana Labels", showManaLabels);
            gizmoManaCap = EditorGUILayout.Slider("Gizmo Mana Cap", gizmoManaCap, 1f, 100f);
            gizmoAlpha = EditorGUILayout.Slider("Gizmo Alpha", gizmoAlpha, 0.05f, 1f); // ✨ 추가된 슬라이더
            EditorGUI.indentLevel--;
        }

        if (GUI.changed) SceneView.RepaintAll();
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    private static void DrawGizmosForSimulate(ManaFlow_Simulate sim, GizmoType gizmoType)
    {
        if (!showGizmos || sim == null || sim.gridManager == null) return;

        var grid = sim.gridManager.WorldGrid;
        var tilemap = sim.gridManager.groundTilemap;
        if (grid == null || tilemap == null) return;

        BoundsInt bounds = tilemap.cellBounds;
        Vector2Int gridOffset = new Vector2Int(bounds.xMin, bounds.yMin);
        Vector2Int gridSize = new Vector2Int(bounds.size.x, bounds.size.y);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                var tile = grid[x, y];
                if (tile == null || tile.Mana == null) continue;

                float mana = tile.Mana.CurrentMana;
                Color cellColor = GetManaDebugColor(mana, gizmoManaCap, gizmoAlpha); // ✨ 알파 포함 호출

                Vector3 worldPos = tilemap.GetCellCenterWorld(new Vector3Int(x + gridOffset.x, y + gridOffset.y, 0));
                Vector3 size = Vector3.one * 0.9f;

                Handles.color = cellColor;
                Handles.DrawSolidRectangleWithOutline(
                    GetRectangleVertices(worldPos, size, Quaternion.identity),
                    cellColor,
                    Color.black * 0.3f
                );

                if (showManaLabels)
                {
                    Vector3 offset = new Vector3(-0.25f, 0f, 0f);
                    Handles.Label(worldPos + offset, mana.ToString("F1"));
                }
            }
        }
    }

    /// <summary>
    /// 빨강 -> 보라 -> 민트 보간 + 알파 조절
    /// </summary>
    private static Color GetManaDebugColor(float mana, float cap, float alpha)
    {
        float t = Mathf.Clamp01(mana / cap);

        Color color;
        if (t < 0.5f)
        {
            float localT = t / 0.5f;
            color = Color.Lerp(
                new Color(1f, 0f, 0f),    // 빨강
                new Color(0.5f, 0f, 1f),  // 보라
                localT
            );
        }
        else
        {
            float localT = (t - 0.5f) / 0.5f;
            color = Color.Lerp(
                new Color(0.5f, 0f, 1f),  // 보라
                new Color(0f, 1f, 1f),    // 민트
                localT
            );
        }

        color.a = alpha; // 사용자 설정 알파 반영
        return color;
    }

    private static Vector3[] GetRectangleVertices(Vector3 center, Vector3 size, Quaternion rotation)
    {
        Vector3 half = size * 0.5f;
        return new Vector3[]
        {
            center + rotation * new Vector3(-half.x, -half.y, 0),
            center + rotation * new Vector3( half.x, -half.y, 0),
            center + rotation * new Vector3( half.x,  half.y, 0),
            center + rotation * new Vector3(-half.x,  half.y, 0),
        };
    }
}
#endif
