// ManaFlow_ManagerEditor.cs (Place in an 'Editor' folder)
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
// using System.Collections.Generic; // 정렬 안 하므로 필요 없어짐
// using System.Linq; // 정렬 안 하므로 필요 없어짐

[CustomEditor(typeof(ManaFlow_Manager))]
public class ManaFlow_ManagerEditor : Editor
{
    private ManaFlow_Manager manager;
    private static bool showGridGizmos = true;
    private static bool showManaLabels = true;

    private void OnEnable()
    {
        manager = (ManaFlow_Manager)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gizmo Settings", EditorStyles.boldLabel);
        showGridGizmos = EditorGUILayout.Toggle("Show Grid Gizmos", showGridGizmos);
        if (showGridGizmos)
        {
            EditorGUI.indentLevel++;
            showManaLabels = EditorGUILayout.Toggle("Show Mana Labels", showManaLabels);
            EditorGUI.indentLevel--;
        }
        if (GUI.changed) { SceneView.RepaintAll(); }
    }

    // Gizmo 그리기 메서드 (정렬 로직 없이, 라벨 위치만 수정됨)
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void DrawGizmosForManaFlowManager(ManaFlow_Manager mgr, GizmoType gizmoType)
    {
        if (!showGridGizmos || mgr == null) return;

        Mana_Cell[,] grid = mgr.GetManaGrid();
        if (grid == null) return;

        int width = mgr.GridWidth;
        int height = mgr.GridHeight;
        Transform managerTransform = mgr.transform;

        Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        Color minManaColor = new Color(0.5f, 0f, 1f, 0.7f);
        Color maxManaColor = new Color(0f, 1f, 0.8f, 0.7f);

        // 그리드 셀 순회하며 바로 그리기 (정렬 없음)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Mana_Cell cell = grid[x, y];
                if (cell == null) continue;

                Vector3 localPos = new Vector3(x - (width - 1) * 0.5f, y - (height - 1) * 0.5f, 0f);
                Vector3 worldPos = managerTransform.TransformPoint(localPos); // 셀의 월드 중심 좌표
                Quaternion rotation = managerTransform.rotation;
                Vector3 size = Vector3.one * 0.9f;

                Color cellColor;
                if (cell.ManaPower <= 0.01f) { cellColor = emptyColor; }
                else
                {
                    float lerpFactor = Mathf.Clamp01(cell.ManaPower / mgr.maxManaForFullColor);
                    cellColor = Color.Lerp(minManaColor, maxManaColor, lerpFactor);
                }

                // 사각형 그리기
                Handles.color = cellColor;
                Handles.DrawSolidRectangleWithOutline(
                    GetRectangleVertices(worldPos, size, rotation),
                    cellColor,
                    Color.black * 0.5f
                );

                // 라벨 그리기 (옵션 켜져 있을 시)
                if (showManaLabels && SceneView.currentDrawingSceneView != null)
                {
                    Handles.color = Color.white;
                    // --- 핵심 수정: 라벨 위치를 오프셋 없이 worldPos(셀 중앙)로 설정 ---
                    // 왼쪽으로 0.5 유닛만큼 이동 (조정 가능)
                    Vector3 offset = new Vector3(-0.25f, 0, 0);
                    Handles.Label(worldPos + offset, cell.ManaPower.ToString("F1"));
                    // --- 여기까지 ---
                }
            }
        }
    }


    // 사각형 꼭짓점 계산 헬퍼 (이전과 동일)
    private static Vector3[] GetRectangleVertices(Vector3 center, Vector3 size, Quaternion rotation)
    {
        Vector3 halfSize = size * 0.5f;
        Vector3 p1 = new Vector3(-halfSize.x, -halfSize.y, 0);
        Vector3 p2 = new Vector3(halfSize.x, -halfSize.y, 0);
        Vector3 p3 = new Vector3(halfSize.x, halfSize.y, 0);
        Vector3 p4 = new Vector3(-halfSize.x, halfSize.y, 0);
        Vector3[] vertices = new Vector3[4] {
            center + rotation * p1, center + rotation * p2, center + rotation * p3, center + rotation * p4
        };
        return vertices;
    }
}
#endif // UNITY_EDITOR