﻿// ManaFlow_Manager.cs
using UnityEngine;
using System.Collections.Generic;

public class ManaFlow_Manager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 20;
    public int height = 20;

    [Header("Simulation Settings")]
    [Range(0f, 1f)] // 확산 계수는 보통 0.5 이하 (안정성 고려)
    public float diffusionRate = 0.02f;

    [Header("Source Settings")]
    public List<ManaWater_Source> manaWaterSources = new List<ManaWater_Source>();

    [Header("Visualization (Gizmo)")]
    public float maxManaForFullColor = 30f; // Gizmo 색상 계산에 사용

    // 그리드 데이터는 외부에서 직접 수정하지 못하도록 private 처리
    private Mana_Cell[,] manaGrid;

    // 외부(특히 Editor 스크립트)에서 그리드 정보를 읽기 위한 public 접근자
    public Mana_Cell[,] GetManaGrid() => manaGrid;
    public int GridWidth => width;
    public int GridHeight => height;

    // --- Unity Methods ---

    void Awake()
    {
        InitializeGrid();
    }

    void FixedUpdate() // 물리/시뮬레이션은 FixedUpdate에서 처리하는 것이 좋음
    {
        // 1. 각 마나 소스에서 마나 생성
        GenerateManaFromSources(Time.fixedDeltaTime);

        // 2. 마나 확산 시뮬레이션
        SimulateManaFlow(Time.fixedDeltaTime);
    }

    // --- Initialization ---

    void InitializeGrid()
    {
        manaGrid = new Mana_Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                manaGrid[x, y] = new Mana_Cell(new Vector2Int(x, y));

                // 테스트용으로 특정 영역을 Blocked 타입으로 설정
                if (x >= 5 && x <= 10 && y >= 5 && y <= 10)
                {
                    manaGrid[x, y].SetType(Mana_Cell.CellType.Blocked);
                }
            }
        }
    }

    // --- Simulation Steps ---

    void GenerateManaFromSources(float deltaTime)
    {
        foreach (var source in manaWaterSources)
        {
            // 각 소스의 Generate 메서드 호출
            source.Generate(manaGrid, deltaTime);
        }
    }

    void SimulateManaFlow(float deltaTime)
    {
        // 1. 다음 상태를 저장할 임시 배열 생성
        float[,] nextManaPower = new float[width, height];

        // 2. 현재 그리드 상태를 기반으로 마나 흐름 계산
        CalculateManaFlow(deltaTime, nextManaPower);

        // 3. 계산된 결과를 실제 그리드에 적용
        ApplyManaFlow(nextManaPower);
    }

    /// <summary>
    /// 현재 그리드 상태를 기반으로 마나 흐름을 계산합니다.
    /// </summary>
    /// <param name="deltaTime">시간 간격</param>
    /// <param name="nextManaPower">다음 상태를 저장할 배열</param>
    void CalculateManaFlow(float deltaTime, float[,] nextManaPower)
    {
        Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Mana_Cell currentCell = manaGrid[x, y];

                // Blocked 타입의 셀은 마나 흐름을 차단
                if (currentCell.Type == Mana_Cell.CellType.Blocked)
                    continue;

                float currentPower = currentCell.ManaPower;
                float totalFlowOut = 0f;

                foreach (var dir in directions)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;

                    if (IsValidCell(nx, ny))
                    {
                        Mana_Cell neighborCell = manaGrid[nx, ny];

                        // Blocked 타입의 이웃 셀로는 마나가 흐르지 않음
                        if (neighborCell.Type == Mana_Cell.CellType.Blocked)
                            continue;

                        float neighborPower = neighborCell.ManaPower;
                        float flow = Mathf.Max(0f, (currentPower - neighborPower) * diffusionRate * 10f * deltaTime);
                        flow = Mathf.Min(flow, currentPower - totalFlowOut);
                        totalFlowOut += flow;
                        nextManaPower[nx, ny] += flow;
                    }
                }

                nextManaPower[x, y] += currentPower - totalFlowOut;
            }
        }
    }

    /// <summary>
    /// 두 셀 간의 마나 흐름을 계산합니다.
    /// </summary>
    /// <param name="currentPower">현재 셀의 마나 파워</param>
    /// <param name="neighborPower">이웃 셀의 마나 파워</param>
    /// <param name="deltaTime">시간 간격</param>
    /// <returns>흐르는 마나량</returns>
    float CalculateFlow(float currentPower, float neighborPower, float deltaTime)
    {
        // 확산 비율과 시간 간격을 기반으로 흐름 계산
        return Mathf.Max(0f, (currentPower - neighborPower) * diffusionRate * deltaTime);
    }

    /// <summary>
    /// 계산된 마나 흐름을 실제 그리드에 적용합니다.
    /// </summary>
    /// <param name="nextManaPower">다음 상태를 저장한 배열</param>
    void ApplyManaFlow(float[,] nextManaPower)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Mana_Cell의 SetMana 메서드를 통해 값 설정 (내부적으로 클램핑됨)
                manaGrid[x, y].SetMana(nextManaPower[x, y]);
            }
        }
    }

    /// <summary>
    /// 주어진 좌표가 유효한 셀인지 확인합니다.
    /// </summary>
    /// <param name="x">셀의 X 좌표</param>
    /// <param name="y">셀의 Y 좌표</param>
    /// <returns>유효한 셀인지 여부</returns>
    bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    public void Generate(Mana_Cell[,] manaGrid, float deltaTime)
    {
        foreach (var source in manaWaterSources)
        {
            foreach (var cell in manaGrid)
            {
                if (cell.Type == Mana_Cell.CellType.Blocked)
                    continue; // Blocked 타입의 셀에는 마나를 추가하지 않음

                // 각 소스의 regenRate를 사용하여 마나를 추가
                cell.AddMana(source.regenRate * deltaTime);
            }
        }
    }


    // OnDrawGizmos 메서드는 제거됨 (Editor 스크립트로 이동)
}