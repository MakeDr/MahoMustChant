// SimulationTickManager.cs
using UnityEngine;

/// <summary>
/// 게임 내 틱 기반 시뮬레이션 단계를 총괄하는 관리자입니다.
/// 자체 타이머로 틱을 발생시키고, 각 시뮬레이션 시스템의 계산 및 적용 단계를 조율합니다.
/// </summary>
public class SimulationTickManager : MonoBehaviour
{
    [Header("필수 참조")]
    [Tooltip("월드 그리드 매니저")]
    public WorldGridManager gridManager;

    [Header("시뮬레이션 시스템 참조")]
    [Tooltip("마나 생성 시스템 컴포넌트")]
    public ManaSource_System manaSourceSystem;
    [Tooltip("마나 확산 시뮬레이션 컴포넌트")]
    public ManaFlow_Simulate manaFlowSimulate;
    // 다른 시뮬레이션 시스템 추가 가능...

    [Header("틱 설정")]
    [Tooltip("초당 실행할 틱(Tick)의 수")]
    [Range(1, 120)] public int ticksPerSecond = 5;
    [Tooltip("시뮬레이션 틱을 일시정지할지 여부")]
    [SerializeField] private bool isPaused = false;

    // 내부 타이머 변수
    private float timer = 0f;
    private float TickInterval => ticksPerSecond > 0 ? 1f / ticksPerSecond : float.MaxValue;

    // --- Unity Methods ---
    private void Update()
    {
        if (isPaused || gridManager == null || gridManager.WorldGrid == null) return;

        timer += Time.deltaTime;
        while (timer >= TickInterval) // 프레임 드랍 시에도 틱 따라잡기
        {
            timer -= TickInterval;
            HandleTick();
        }
    }

    /// <summary>
    /// 한 번의 시뮬레이션 틱을 처리합니다. (계산 단계 -> 적용 단계)
    /// </summary>
    private void HandleTick()
    {
        // --- 1단계: 계산 ---
        manaSourceSystem?.CalculateGenerationChanges(); // public 메서드 호출
        manaFlowSimulate?.CalculateDiffusionChanges(); // public 메서드 호출
        // 다른 시스템 계산 호출...

        // --- 2단계: 적용 ---
        if (gridManager == null || gridManager.WorldGrid == null) return; // 중복 체크지만 안전하게
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

    // --- 외부 제어 메서드 ---
    [ContextMenu("Force Tick")]
    public void ForceTick() { Debug.Log("Forcing simulation tick..."); HandleTick(); }
    public void PauseSimulation() { isPaused = true; Debug.Log("Simulation paused."); }
    public void ResumeSimulation() { isPaused = false; Debug.Log("Simulation resumed."); }
    public void TogglePause() { isPaused = !isPaused; Debug.Log($"Simulation {(isPaused ? "paused" : "resumed")}."); }
}

