// ManaFlow_ManagerEditor.cs (Place in an 'Editor' folder)
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
// using System.Collections.Generic; // ���� �� �ϹǷ� �ʿ� ������
// using System.Linq; // ���� �� �ϹǷ� �ʿ� ������

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

    // Gizmo �׸��� �޼��� (���� ���� ����, �� ��ġ�� ������)
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

        // �׸��� �� ��ȸ�ϸ� �ٷ� �׸��� (���� ����)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Mana_Cell cell = grid[x, y];
                if (cell == null) continue;

                Vector3 localPos = new Vector3(x - (width - 1) * 0.5f, y - (height - 1) * 0.5f, 0f);
                Vector3 worldPos = managerTransform.TransformPoint(localPos); // ���� ���� �߽� ��ǥ
                Quaternion rotation = managerTransform.rotation;
                Vector3 size = Vector3.one * 0.9f;

                Color cellColor;
                if (cell.ManaPower <= 0.01f) { cellColor = emptyColor; }
                else
                {
                    float lerpFactor = Mathf.Clamp01(cell.ManaPower / mgr.maxManaForFullColor);
                    cellColor = Color.Lerp(minManaColor, maxManaColor, lerpFactor);
                }

                // �簢�� �׸���
                Handles.color = cellColor;
                Handles.DrawSolidRectangleWithOutline(
                    GetRectangleVertices(worldPos, size, rotation),
                    cellColor,
                    Color.black * 0.5f
                );

                // �� �׸��� (�ɼ� ���� ���� ��)
                if (showManaLabels && SceneView.currentDrawingSceneView != null)
                {
                    Handles.color = Color.white;
                    // --- �ٽ� ����: �� ��ġ�� ������ ���� worldPos(�� �߾�)�� ���� ---
                    // �������� 0.5 ���ָ�ŭ �̵� (���� ����)
                    Vector3 offset = new Vector3(-0.25f, 0, 0);
                    Handles.Label(worldPos + offset, cell.ManaPower.ToString("F1"));
                    // --- ������� ---
                }
            }
        }
    }


    // �簢�� ������ ��� ���� (������ ����)
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