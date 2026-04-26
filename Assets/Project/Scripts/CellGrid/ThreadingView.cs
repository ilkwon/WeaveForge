using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

/// <summary>
/// 트레딩 뷰 : 경사 배열.
/// </summary>
public class ThreadingView : CellGridView
{  
  [SerializeField] private PalettePopupUI palettePopup;
  [SerializeField] private TieupView tieupView;
  public System.Action OnThreadingChanged;
  public System.Action OnColorChanged;
  private int[] _threadingData; // 각 경사가 몇 번 종광인지
  private Color[] _warpColors;

  //---------------------------------------------------------------------------
  IEnumerator Start()
  {
    yield return null;

    int shaftCount = tieupView.RowCount;
    ColCount = tieupView.ColCount * 4;
    RowCount = shaftCount + 1; // 종광 수 + 컬러피커 1줄
    CellSize = tieupView.CellSize;

    tieupView.OnPatternLoaded += Resize;

    _threadingData = new int[ColCount];
    for (int i = 0; i < ColCount; i++)
      _threadingData[i] = -1; // 미지정

    Refresh();
  }
  //---------------------------------------------------------------------------
  private void InitColors()
  {
    _warpColors = new Color[ColCount];
    for (int i = 0; i < ColCount; i++)
      _warpColors[i] = ColorPalette.Unset; // 미지정
  }

  //---------------------------------------------------------------------------
  private void GenerateStraightDraw()
  {
    var shaftCount = tieupView.RowCount;
    for (int i = 0; i < ColCount; i++)
    {
      // 단순히 종광 수로 나눈 나머지로 초기값 설정 
      // (직조 패턴에 따라 달라질 수 있음)
      _threadingData[i] = ((ColCount - 1 - i) % shaftCount) + 1;
    }

    _fontRenderer.PrepareNumbers(1, shaftCount);  // 
    for (int i = 0; i < ColCount; i++)
    {
      _fontRenderer.DrawNumber(_threadingData[i], i, _threadingData[i] - 1, Color.black);
    }

    _drawer.Apply();
  }
  //---------------------------------------------------------------------------
  private void UpdatePosition()
  {
    RectTransform rt = GetComponent<RectTransform>();
    RectTransform tieupRT = tieupView.GetComponent<RectTransform>();

    rt.anchorMin = new Vector2(1f, 1f);
    rt.anchorMax = new Vector2(1f, 1f);
    rt.pivot = new Vector2(1f, 1f);
    rt.sizeDelta = new Vector2(ColCount * CellSize, RowCount * CellSize);
    rt.anchoredPosition = new Vector2(
        tieupRT.anchoredPosition.x - tieupRT.sizeDelta.x,
        tieupRT.anchoredPosition.y + CellSize
    );
  }

  //---------------------------------------------------------------------------
  private void Resize()
  {
    ColCount = tieupView.ColCount * 4;
    RowCount = tieupView.RowCount + 1;

    _threadingData = new int[ColCount];
    for (int i = 0; i < ColCount; i++)
      _threadingData[i] = -1; // 미지정

    Refresh();
  }
  //---------------------------------------------------------------------------
  private void Refresh()
  {
    Init();

    // 위치 — 타이업 왼쪽, 컬러피커 1줄 위로 돌출
    UpdatePosition();
    InitColors();
    GenerateStraightDraw();
    LoadColors(tieupView.CurrentData);
  }

  //---------------------------------------------------------------------------
  protected override void OnCellClicked(int col, int row)
  {
    if (row == RowCount - 1)
    {
      PopupPalette(col, row);      
      return;
    }

    // 종광 영역
    var prevShaft = _threadingData[col];
    // 이전 숫자 지우기
    if (prevShaft >= 1)
        _drawer.FillCell(col, prevShaft - 1, Color.white);
    
    if (prevShaft == row + 1)
    {
      // 같은 셀 클릭 -> 해제
      _threadingData[col] = -1;
    }
    else
    {
      // 다른 셀 클릭 -> 업데이트
      int threadingNum = row + 1;
      _threadingData[col] = threadingNum;
      _fontRenderer.DrawNumber(threadingNum, col, row, Color.black);    
    }

    _drawer.Apply();
    OnThreadingChanged?.Invoke();
  }

  //---------------------------------------------------------------------------
  private void PopupPalette(int col, int row)
  {
      // 컬러피커 구현.
      palettePopup.Show((colorName) =>
      {
        _warpColors[col] = ColorPalette.GetColor(colorName);
        Debug.Log($"Selected color for col {col}: {colorName} -> {_warpColors[col]}");
        for (int i = 0; i < ColCount; i++)
        {
          if (_warpColors[i] == ColorPalette.Unset) // 미지정 컬러는 선택한 컬러로 초기화
          {
              _warpColors[i] = _warpColors[col];              
          }
          _drawer.FillCell(i, RowCount - 1, (Color32)_warpColors[i]);
        }
        
        _drawer.Apply();

        var data = tieupView.CurrentData;
        if (data != null && data.warpColorNames != null && col < data.warpColorNames.Length)
        {
          data.warpColorNames[col] = colorName;
          WeaveSaveManager.Instance.Save(data, false);                    
        }
        OnColorChanged?.Invoke();
      }, Mouse.current.position.ReadValue());
  }

  //---------------------------------------------------------------------------
  protected override void RestoreCell(int x, int y)
  {
    if (x < 0 || y < 0 || _threadingData == null) return;

    if (drawType == CellDrawType.Dot)
    {
      Color32 color = (_threadingData[x] == y)
          ? new Color32(0, 0, 0, 255)
          : new Color32(255, 255, 255, 255);
      _drawer.FillCell(x, y - 1, color);
    }
    else
    {
      _drawer.FillCell(x, y, new Color32(255, 255, 255, 255));
      if (_threadingData[x] == y && y > 0)
        _fontRenderer.DrawNumber(_threadingData[x], x, y - 1, Color.black);
    }
  }
  //---------------------------------------------------------------------------
  public int GetThreading(int col)
  {
    return _threadingData[col];
  }
  //---------------------------------------------------------------------------
  protected override void Update()
  {
    if (palettePopup.gameObject.activeSelf)
      return;
    
    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
      RectTransform rt = GetComponent<RectTransform>();
      Vector2 localMousePos;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
          rt, Mouse.current.position.ReadValue(), null, out localMousePos);

      if (localMousePos.x < -rt.sizeDelta.x || localMousePos.x > 0 ||
          localMousePos.y < -rt.sizeDelta.y || localMousePos.y > 0)
        return;

      RectTransformUtility.ScreenPointToLocalPointInRectangle(
          rt, Mouse.current.position.ReadValue(), null, out Vector2 localPos);

      float adjustedX = localPos.x + rt.sizeDelta.x;
      float adjustedY = localPos.y + rt.sizeDelta.y;

      int cx = (int)(adjustedX / CellSize);
      int cy = (int)(adjustedY / CellSize);
      if (cx >= 0 && cx < ColCount && cy >= 0 && cy < RowCount)
        OnCellClicked(cx, cy);
    }
  }
  //---------------------------------------------------------------------------
  private void LoadColors(WeaveData data)
  {
    if (data == null) return;
    if (data.warpColorNames == null || data.warpColorNames.Length == 0) return;

    for (int i = 0; i < ColCount; i++)
    {
      var colorName = data.warpColorNames[i];
      Debug.Log($"LoadColors: col={i}, colorName={colorName}");
      //_warpColors[i] = string.IsNullOrEmpty(colorName) ? ColorPalette.Unset : ColorPalette.GetColor(colorName);
      _warpColors[i] = (string.IsNullOrEmpty(colorName) || colorName == "White") ? ColorPalette.Unset : ColorPalette.GetColor(colorName);
      _drawer.FillCell(i, RowCount - 1, (Color32)_warpColors[i]);
    }
    _drawer.Apply();
  }
  //---------------------------------------------------------------------------
  public Color WarpColor(int col)
  {
    if (_warpColors == null || col < 0 || col >= _warpColors.Length)
      return Color.black;
    if (_warpColors[col] == ColorPalette.Unset)  // 미지정 컬러는 검정으로 반환
      return Color.gray;  //
    return _warpColors[col];
  }
  //---------------------------------------------------------------------------
}