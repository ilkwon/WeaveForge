using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// 트레딩 뷰 : 경사 배열.
/// </summary>
public class ThreadingView : CellGridView
{
  [SerializeField] private PalettePopupUI palettePopup;
  [SerializeField] private TieupView tieupView;
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
       _warpColors[i] = Color.white;
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
    if (row != RowCount - 1) return; 

    // 컬러피커 구현.
    palettePopup.Show((colorName) =>
    {      
      _warpColors[col] = ColorPalette.GetColor(colorName);
      _drawer.FillCell(col, RowCount - 1, _warpColors[col]);
      _drawer.Apply();

      var data = tieupView.CurrentData;
      if (data != null && data.warpColorNames != null && col < data.warpColorNames.Length)
      {
        data.warpColorNames[col] = colorName;
        WeaveSaveManager.Instance.Save(data, false);
      }
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
      _warpColors[i] = ColorPalette.GetColor(colorName);
      _drawer.FillCell(i, RowCount - 1, _warpColors[i]);     
    }
    _drawer.Apply();
  }
  //---------------------------------------------------------------------------
}