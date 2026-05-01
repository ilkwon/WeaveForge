using System;
using UnityEngine;

/// <summary>
/// 타이업 뷰 : 조직도.
/// </summary>
public class TieupView : CellGridView
{
  public System.Action OnPatternLoaded;
  public System.Action OnTieupChanged;

  public WeaveData CurrentData => _currentData;
  private WeaveData _currentData;
  private int[,] _gridData;

  //---------------------------------------------------------------------------
  protected override void Init()
  {
    base.Init();
    _gridData = new int[RowCount, ColCount];

    RectTransform rt = GetComponent<RectTransform>();
    rt.anchorMin = new Vector2(1f, 1f);
    rt.anchorMax = new Vector2(1f, 1f);
    rt.pivot = new Vector2(1f, 1f);
    rt.anchoredPosition = new Vector2(-40, -40);
  }
  //---------------------------------------------------------------------------
  private void Start()
  {
    Init();
    WeaveDocumentManager.Instance.OnDocumentChanged += LoadPattern;

    // 현재 열려있는 문서가 있을 경우 로드
    if (WeaveDocumentManager.Instance.CurrentWeaveData != null)
      LoadPattern(WeaveDocumentManager.Instance.CurrentWeaveData);
  }
  //--------------------------------------------------------------------------
  protected override void OnDestroy()
  {    
    base.OnDestroy();
    if (WeaveDocumentManager.Instance != null)
      WeaveDocumentManager.Instance.OnDocumentChanged -= LoadPattern;
  }

  //---------------------------------------------------------------------------
  protected override void OnCellClicked(int col, int row)
  {
    var old = _gridData[row, col];
    _gridData[row, col] = old == 1 ? 0 : 1;
    
    // 데이터 저장
    if (_currentData != null)
      _currentData.cells[row * ColCount + col] = _gridData[row, col];
    DrawCell(col, row, _gridData[row, col] == 1);
    //_drawer.Apply();
    _applyFlag = true;
    OnTieupChanged?.Invoke();
  }
  //---------------------------------------------------------------------------
  protected override void OnCellDrag(int col, int row, int dragValue)
  {
    _gridData[row, col] = dragValue;

    // 데이터 저장
    if (_currentData != null)
      _currentData.cells[row * ColCount + col] = _gridData[row, col];

    DrawCell(col, row, dragValue == 1);
    //_drawer.Apply();
    _applyFlag = true;
    OnTieupChanged?.Invoke();
  }
  //---------------------------------------------------------------------------
  private void DrawCell(int col, int row, bool drag)
  {
    Color32 color = drag
        ? new Color32(0, 0, 0, 255)
        : new Color32(255, 255, 255, 255);
    _drawer.FillCell(col, row, color);

  }

  //---------------------------------------------------------------------------
  protected override int GetCellValue(int col, int row)
  {
    return _gridData[row, col];
  }
  //---------------------------------------------------------------------------
  protected override void RestoreCell(int x, int y)
  {
    if (x < 0 || y < 0) return;
    DrawCell(x, y, _gridData[y, x] == 1);
  }
  //---------------------------------------------------------------------------
  public int GetCell(int col, int row)
  {
    return _gridData[row, col];
  }

  //---------------------------------------------------------------------------
  public void LoadPattern(WeaveData data)
  {
    _currentData = data;

    RowCount = data.rowCount;
    ColCount = data.colCount;

    Init();

    for (int row = 0; row < RowCount; row++)
    {
      for (int col = 0; col < ColCount; col++)
      {
        _gridData[row, col] = data.cells[row * ColCount + col];
        Color32 color = _gridData[row, col] == 1
            ? new Color32(0, 0, 0, 255)
            : new Color32(255, 255, 255, 255);
        _drawer.FillCell(col, row, color);
      }
    }

    _drawer.Apply();
    SetWarpColor(data);
    SetWeftColor(data);

    OnPatternLoaded?.Invoke();
  }

  //---------------------------------------------------------------------------
  private void SetWarpColor(WeaveData data)
  {
    int warpCount = ColCount * WeaveDocumentManager.Instance.CurrentWeaveSettings.warpRepeat;
    if (data.warpColorNames == null || data.warpColorNames.Length != warpCount)
    {
      var newColors = new string[warpCount];
      for (int i = 0; i < warpCount; i++)
        newColors[i] = (data.warpColorNames != null && i < data.warpColorNames.Length)
            ? data.warpColorNames[i]
            : string.Empty;
      data.warpColorNames = newColors;
    }

  }
  //---------------------------------------------------------------------------
  private void SetWeftColor(WeaveData data)
  {
    int weftCount = RowCount * WeaveDocumentManager.Instance.CurrentWeaveSettings.weftRepeat;;
    if (data.weftColorNames == null || data.weftColorNames.Length != weftCount)
    {
      var newColors = new string[weftCount];
      for (int i = 0; i < weftCount; i++)
        newColors[i] = (data.weftColorNames != null && i < data.weftColorNames.Length)
            ? data.weftColorNames[i]
            : string.Empty;
      data.weftColorNames = newColors;
    }
  }
  //---------------------------------------------------------------------------

}
