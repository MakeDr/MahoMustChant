// ManaState.cs
using UnityEngine;

/// <summary>
/// 타일 또는 개체의 마나 상태를 관리하는 클래스.
/// 틱 기반 시뮬레이션에서 안정적인 상태 변화를 위해 Pending Delta 메커니즘을 사용합니다.
/// </summary>
[System.Serializable]
public class ManaState
{
    [Tooltip("현재 마나 보유량. 직접 수정보다는 AddPendingChange/ApplyPendingChanges 사용 권장.")]
    [SerializeField] private float _currentMana; // 외부 직접 접근 제한

    [Tooltip("마나 보유량 하한값")]
    [SerializeField] private float _minMana = 0f;

    [Tooltip("마나 보유량 상한값")]
    [SerializeField] private float _maxMana = 100f; // 필요에 따라 float.MaxValue 사용 가능

    // 틱 동안 발생한 마나 변화량을 누적하는 내부 버퍼
    private float _pendingDelta = 0f;

    // --- Public Accessors ---

    /// <summary>
    /// 현재 마나 보유량 (읽기 전용).
    /// AI 등이 인지하는 값으로도 사용될 수 있으나, 지연이나 필터링은 적용되지 않은 현재 값입니다.
    /// </summary>
    public float CurrentMana => _currentMana;
    public float MinMana => _minMana;
    public float MaxMana => _maxMana;

    // --- Simulation Methods ---

    /// <summary>
    /// 현재 틱에서 발생한 마나 변화량(delta)을 내부 버퍼에 누적합니다.
    /// 마이너스 값도 가능합니다 (예: 확산으로 잃는 경우).
    /// </summary>
    /// <param name="delta">이번 틱에서의 마나 변화량</param>
    public void AddPendingChange(float delta)
    {
        _pendingDelta += delta;
    }

    /// <summary>
    /// 현재 틱 동안 누적된 모든 Pending 변화량을 실제 CurrentMana에 적용하고 버퍼를 리셋합니다.
    /// 반드시 모든 계산(생성, 확산 등)이 끝난 후, 틱의 마지막 단계에서 그리드 전체에 대해 호출되어야 합니다.
    /// </summary>
    public void ApplyPendingChanges()
    {
        // 변화량 적용 및 Clamp 동시 처리
        _currentMana = Mathf.Clamp(_currentMana + _pendingDelta, _minMana, _maxMana);
        _pendingDelta = 0f; // 버퍼 리셋
    }

    // --- Utility Methods ---

    /// <summary>
    /// 마나 상태를 특정 값으로 즉시 설정합니다. (시뮬레이션 초기화 등에 사용)
    /// Pending Delta는 초기화됩니다.
    /// </summary>
    /// <param name="value">설정할 마나 값</param>
    public void InitializeMana(float value)
    {
        _currentMana = Mathf.Clamp(value, _minMana, _maxMana);
        _pendingDelta = 0f;
    }

    /// <summary>
    /// 마나 상태가 거의 비어있는지 확인합니다.
    /// </summary>
    public bool IsDormant(float threshold = 0.1f) => _currentMana < threshold;

    /// <summary>
    /// 마나 상태가 최대치를 초과했는지 확인합니다. (Clamp로 인해 일반적으로 발생하지 않음)
    /// </summary>
    public bool IsOvercharged => _currentMana >= _maxMana; // 등호 포함 고려
}