using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(RectTransform))]
public class TreadlingView : CellGridView
{
  [SerializeField] private TieupView tieupView;
  private int[] _treadlingData; // 각 위사가 몇 번 트레들인지
  
  //---------------------------------------------------------------------------
  IEnumerator Start()
  {
    yield return null;

    int treadleCount = tieupView.ColCount;
    ColCount = treadleCount + 1; // 트레들 수 + 컬러피커 1열
    RowCount = tieupView.RowCount * 4;
    CellSize = tieupView.CellSize;
    
    tieupView.OnPatternLoaded += Resize;
    
    Init();

    _treadlingData = new int[RowCount];
    for (int i = 0; i < RowCount; i++)
      _treadlingData[i] = -1;    
  }

  private void UpdatePosition()
  {
    
    // 위치 — 타이업 아래, 컬러피커 1열 오른쪽 돌출
    RectTransform rt = GetComponent<RectTransform>();
    RectTransform tieupRT = tieupView.GetComponent<RectTransform>();

    rt.anchorMin = new Vector2(1f, 1f);
    rt.anchorMax = new Vector2(1f, 1f);
    rt.pivot = new Vector2(1f, 1f);
    rt.sizeDelta = new Vector2(ColCount * CellSize, RowCount * CellSize);
    rt.anchoredPosition = new Vector2(
        tieupRT.anchoredPosition.x + CellSize,
        tieupRT.anchoredPosition.y - tieupRT.sizeDelta.y
    );
  }

  //---------------------------------------------------------------------------
  private void Resize()
  {
    ColCount = tieupView.ColCount + 1;
    RowCount = tieupView.RowCount * 4;
    _treadlingData = new int[RowCount];
    for (int i = 0; i < RowCount; i++)
      _treadlingData[i] = -1;

    Init();

    // 위치 — 타이업 왼쪽, 컬러피커 1열 왼쪽으로 돌출
    UpdatePosition();
  }

  //---------------------------------------------------------------------------
  protected override void OnCellClicked(int col, int row)
  {
    int lastCol = ColCount - 1;
    if (col == lastCol) return; // 컬러피커 열은 무시

    int prevCol = _treadlingData[row];

    // 이전 점 지우기
    if (prevCol >= 0)
    {
      _drawer.FillCell(prevCol, row, new Color32(255, 255, 255, 255));
    }

    // 같은 곳 클릭하면 해제
    if (prevCol == col)
    {
      _treadlingData[row] = -1;
    }
    else
    {
      _treadlingData[row] = col;
      _drawer.FillCell(col, row, new Color32(0, 0, 0, 255));
    }

    _drawer.Apply();
  }

  //---------------------------------------------------------------------------
  protected override void RestoreCell(int x, int y)
  {
    if (x < 0 || y < 0 || _treadlingData[0] == -1) return;
    Color32 color = (_treadlingData[y] == x)
        ? new Color32(0, 0, 0, 255)
        : new Color32(255, 255, 255, 255);
    _drawer.FillCell(x, y, color);
  }
  
  //---------------------------------------------------------------------------
  public int GetTreadling(int row)
  {
    return _treadlingData[row];
  }

  //---------------------------------------------------------------------------
}