// SimulationTickManager.cs
using UnityEngine;

/// <summary>
/// ���� �� ƽ ��� �ùķ��̼� �ܰ踦 �Ѱ��ϴ� �������Դϴ�.
/// ��ü Ÿ�̸ӷ� ƽ�� �߻���Ű��, �� �ùķ��̼� �ý����� ��� �� ���� �ܰ踦 �����մϴ�.
/// </summary>
public class SimulationTickManager : MonoBehaviour
{
    [Header("�ʼ� ����")]
    [Tooltip("���� �׸��� �Ŵ���")]
    public WorldGridManager gridManager;

    [Header("�ùķ��̼� �ý��� ����")]
    [Tooltip("���� ���� �ý��� ������Ʈ")]
    public ManaSource_System manaSourceSystem;
    [Tooltip("���� Ȯ�� �ùķ��̼� ������Ʈ")]
    public ManaFlow_Simulate manaFlowSimulate;
    // �ٸ� �ùķ��̼� �ý��� �߰� ����...

    [Header("ƽ ����")]
    [Tooltip("�ʴ� ������ ƽ(Tick)�� ��")]
    [Range(1, 120)] public int ticksPerSecond = 5;
    [Tooltip("�ùķ��̼� ƽ�� �Ͻ��������� ����")]
    [SerializeField] private bool isPaused = false;

    // ���� Ÿ�̸� ����
    private float timer = 0f;
    private float TickInterval => ticksPerSecond > 0 ? 1f / ticksPerSecond : float.MaxValue;

    // --- Unity Methods ---
    private void Update()
    {
        if (isPaused || gridManager == null || gridManager.WorldGrid == null) return;

        timer += Time.deltaTime;
        while (timer >= TickInterval) // ������ ��� �ÿ��� ƽ �������
        {
            timer -= TickInterval;
            HandleTick();
        }
    }

    /// <summary>
    /// �� ���� �ùķ��̼� ƽ�� ó���մϴ�. (��� �ܰ� -> ���� �ܰ�)
    /// </summary>
    private void HandleTick()
    {
        // --- 1�ܰ�: ��� ---
        manaSourceSystem?.CalculateGenerationChanges(); // public �޼��� ȣ��
        manaFlowSimulate?.CalculateDiffusionChanges(); // public �޼��� ȣ��
        // �ٸ� �ý��� ��� ȣ��...

        // --- 2�ܰ�: ���� ---
        if (gridManager == null || gridManager.WorldGrid == null) return; // �ߺ� üũ���� �����ϰ�
        var grid = gridManager.WorldGrid;
        var size = gridManager.GridSize;
        for (int ix = 0; ix < size.x; ix++)
        {
            for (int iy = 0; iy < size.y; iy++)
            {
                grid[ix, iy]?.Mana?.ApplyPendingChanges();
            }
        }
    }

    // --- �ܺ� ���� �޼��� ---
    [ContextMenu("Force Tick")]
    public void ForceTick() { Debug.Log("Forcing simulation tick..."); HandleTick(); }
    public void PauseSimulation() { isPaused = true; Debug.Log("Simulation paused."); }
    public void ResumeSimulation() { isPaused = false; Debug.Log("Simulation resumed."); }
    public void TogglePause() { isPaused = !isPaused; Debug.Log($"Simulation {(isPaused ? "paused" : "resumed")}."); }
}

