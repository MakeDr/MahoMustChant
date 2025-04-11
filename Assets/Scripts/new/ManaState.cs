// ManaState.cs
using UnityEngine;

/// <summary>
/// Ÿ�� �Ǵ� ��ü�� ���� ���¸� �����ϴ� Ŭ����.
/// ƽ ��� �ùķ��̼ǿ��� �������� ���� ��ȭ�� ���� Pending Delta ��Ŀ������ ����մϴ�.
/// </summary>
[System.Serializable]
public class ManaState
{
    [Tooltip("���� ���� ������. ���� �������ٴ� AddPendingChange/ApplyPendingChanges ��� ����.")]
    [SerializeField] private float _currentMana; // �ܺ� ���� ���� ����

    [Tooltip("���� ������ ���Ѱ�")]
    [SerializeField] private float _minMana = 0f;

    [Tooltip("���� ������ ���Ѱ�")]
    [SerializeField] private float _maxMana = 100f; // �ʿ信 ���� float.MaxValue ��� ����

    // ƽ ���� �߻��� ���� ��ȭ���� �����ϴ� ���� ����
    private float _pendingDelta = 0f;

    // --- Public Accessors ---

    /// <summary>
    /// ���� ���� ������ (�б� ����).
    /// AI ���� �����ϴ� �����ε� ���� �� ������, �����̳� ���͸��� ������� ���� ���� ���Դϴ�.
    /// </summary>
    public float CurrentMana => _currentMana;
    public float MinMana => _minMana;
    public float MaxMana => _maxMana;

    // --- Simulation Methods ---

    /// <summary>
    /// ���� ƽ���� �߻��� ���� ��ȭ��(delta)�� ���� ���ۿ� �����մϴ�.
    /// ���̳ʽ� ���� �����մϴ� (��: Ȯ������ �Ҵ� ���).
    /// </summary>
    /// <param name="delta">�̹� ƽ������ ���� ��ȭ��</param>
    public void AddPendingChange(float delta)
    {
        _pendingDelta += delta;
    }

    /// <summary>
    /// ���� ƽ ���� ������ ��� Pending ��ȭ���� ���� CurrentMana�� �����ϰ� ���۸� �����մϴ�.
    /// �ݵ�� ��� ���(����, Ȯ�� ��)�� ���� ��, ƽ�� ������ �ܰ迡�� �׸��� ��ü�� ���� ȣ��Ǿ�� �մϴ�.
    /// </summary>
    public void ApplyPendingChanges()
    {
        // ��ȭ�� ���� �� Clamp ���� ó��
        _currentMana = Mathf.Clamp(_currentMana + _pendingDelta, _minMana, _maxMana);
        _pendingDelta = 0f; // ���� ����
    }

    // --- Utility Methods ---

    /// <summary>
    /// ���� ���¸� Ư�� ������ ��� �����մϴ�. (�ùķ��̼� �ʱ�ȭ � ���)
    /// Pending Delta�� �ʱ�ȭ�˴ϴ�.
    /// </summary>
    /// <param name="value">������ ���� ��</param>
    public void InitializeMana(float value)
    {
        _currentMana = Mathf.Clamp(value, _minMana, _maxMana);
        _pendingDelta = 0f;
    }

    /// <summary>
    /// ���� ���°� ���� ����ִ��� Ȯ���մϴ�.
    /// </summary>
    public bool IsDormant(float threshold = 0.1f) => _currentMana < threshold;

    /// <summary>
    /// ���� ���°� �ִ�ġ�� �ʰ��ߴ��� Ȯ���մϴ�. (Clamp�� ���� �Ϲ������� �߻����� ����)
    /// </summary>
    public bool IsOvercharged => _currentMana >= _maxMana; // ��ȣ ���� ���
}