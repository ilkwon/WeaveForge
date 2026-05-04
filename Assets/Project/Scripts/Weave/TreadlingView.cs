using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

[RequireComponent(typeof(RectTransform))]
/// <summary>
/// 트레들링 뷰 : 위사 배열. (타이업 column count + 컬러피커 1)
/// </summary>
public class TreadlingView : CellGridView
{
  [SerializeField] private PalettePopupUI palettePopup;
  [SerializeField] private TieupView tieupView;
  [SerializeField] private RectTransform containerRT;
  public System.Action OnTreadlingChanged;
  public System.Action OnColorChanged;
  private int[] _treadlingData; // 각 위사가 몇 번 트레들인지
  private Color[] _weftColors;

  //---------------------------------------------------------------------------
  IEnumerator Start()
  {
    yield return null;

    //int treadleCount = tieupView.ColCount;
    ColCount = tieupView.ColCount + 1; // 트레들 수 + 컬러피커 1열
    RowCount = tieupView.RowCount * WeaveDocumentManager.Instance.CurrentWeaveSettings.weftRepeat; // 종광 수 * 반복 수
    CellSize = tieupView.CellSize;

    tieupView.OnPatternLoaded += Resize;

    _treadlingData = new int[RowCount];
    for (int i = 0; i < RowCount; i++)
      _treadlingData[i] = -1;

    Refresh();
  }

  //---------------------------------------------------------------------------
  private void Refresh()
  {
    Init();
    UpdatePosition();
    InitColors();
    GenerateStraightDraw();
    LoadColors(tieupView.CurrentData);
  }
  //---------------------------------------------------------------------------
  private void InitColors()
  {
    _weftColors = new Color[RowCount];
    for (int i = 0; i < RowCount; i++)
      _weftColors[i] = ColorPalette.Unset; // 미지정
  }

  //---------------------------------------------------------------------------
  private void GenerateStraightDraw()
  {
    var treadleCount = tieupView.ColCount;  // 트레들 수
    for (int i = 0; i < RowCount; i++)
    {
      int treadleNum = ((RowCount - 1 - i) % treadleCount) + 1; // 단순히 트레들 수로 나눈 나머지로 초기값 설정 (직조 패턴에 따라 달라질 수 있음)
      _treadlingData[i] = treadleNum;
    }

    _fontRenderer.PrepareNumbers(1, treadleCount);  // 트레들 수에 맞춰 숫자 준비
    for (int i = 0; i < RowCount; i++)
    {
      int treadleNum = _treadlingData[i];
      _fontRenderer.DrawNumber(treadleNum, treadleNum - 1, i, Color.black); // 트레들 번호에 맞춰 숫자 그리기 (컬러피커 열이 오른쪽 맨끝이다.)
    }
    _drawer.Apply();
  }


  //---------------------------------------------------------------------------
  private void UpdatePosition()
  {

    // 위치 — 타이업 아래, 컬러피커 1열 오른쪽 돌출
    RectTransform rt = GetComponent<RectTransform>();
    RectTransform tieupRT = tieupView.GetComponent<RectTransform>();

    rt.anchorMin = new Vector2(1f, 1f);
    rt.anchorMax = new Vector2(1f, 1f);
    rt.pivot = new Vector2(1f, 1f);
    rt.sizeDelta = new Vector2(ColCount * CellSize, RowCount * CellSize);
    rt.anchoredPosition = Vector2.zero;
    if (containerRT != null)
    {
      containerRT.anchorMin = containerRT.anchorMax = new Vector2(1f, 1f);
      containerRT.pivot = new Vector2(1f, 1f);
      containerRT.sizeDelta = rt.sizeDelta;
      containerRT.anchoredPosition = new Vector2(
          tieupRT.anchoredPosition.x + CellSize,
          tieupRT.anchoredPosition.y - tieupRT.sizeDelta.y
      );
    }
  }

  //---------------------------------------------------------------------------
  private void Resize()
  {
    ColCount = tieupView.ColCount + 1;
    RowCount = tieupView.RowCount * WeaveDocumentManager.Instance.CurrentWeaveSettings.weftRepeat;

    _treadlingData = new int[RowCount];
    for (int i = 0; i < RowCount; i++)
      _treadlingData[i] = -1;

    Refresh();
  }

  //---------------------------------------------------------------------------
  protected override void OnCellClicked(int col, int row)
  {
    if (col == ColCount - 1)
    {
      PopupPalette(col, row);
      return;
    }

    // 숫자를 고치려면 이전 셀 초기화.
    var prevTreadle = _treadlingData[row];
    if (prevTreadle >= 1)
      _drawer.FillCell(prevTreadle - 1, row, new Color32(255, 255, 255, 255)); // 이전 트레들 번호 위치 초기화

    // 같은 셀 클릭시 -> 해제
    if (prevTreadle == col + 1)
      _treadlingData[row] = -1;
    else
    {
      int treadleNum = col + 1;
      _treadlingData[row] = treadleNum; // 선택한 셀의 트레들 번호 저장
      _fontRenderer.DrawNumber(treadleNum, col, row, Color.black); // 선택한 셀에 숫자 그리기  
    }
    _drawer.Apply();
    OnTreadlingChanged?.Invoke();
  }

  //---------------------------------------------------------------------------
  private void PopupPalette(int col, int row)
  {
    // 컬러피커 열 클릭 시 팔레트 팝업 열기
    palettePopup.Show((colorName) =>
    {
      _weftColors[row] = ColorPalette.GetColor(colorName);

      for (int i = 0; i < RowCount; i++)
      {
        if (_weftColors[i] == ColorPalette.Unset)
          _weftColors[i] = _weftColors[row];

        _drawer.FillCell(ColCount - 1, i, (Color32)_weftColors[i]);
      }

      _drawer.Apply();

      // 현재 패턴 데이터에 색상 정보 저장
      var data = tieupView.CurrentData;
      if (data != null && data.weftColorNames != null && row < data.weftColorNames.Length)
      {
        for (int i = 0; i < RowCount && i < data.weftColorNames.Length; i++)
        {
          if (string.IsNullOrEmpty(data.weftColorNames[i]) || data.weftColorNames[i] == "White")
            data.weftColorNames[i] = colorName; // 미지정 컬러는 선택한 컬러로 초기화
        }

        data.weftColorNames[row] = colorName;
        WeaveSaveManager.Instance.Save(data, false);
      }
      OnColorChanged?.Invoke();
    }, Mouse.current.position.ReadValue());
  }

  //---------------------------------------------------------------------------
  protected override void RestoreCell(int x, int y)
  {
    if (x < 0 || y < 0 || _treadlingData == null) return;

    if (drawType == CellDrawType.Dot)
    {
      Color32 color = (_treadlingData[y] == x)
          ? new Color32(0, 0, 0, 255)
          : new Color32(255, 255, 255, 255);
      _drawer.FillCell(x, y - 1, color);
    }
    else
    {
      _drawer.FillCell(x, y, new Color32(255, 255, 255, 255));
      if (_treadlingData[y] == x && x > 0)
        _fontRenderer.DrawNumber(_treadlingData[y], x - 1, y, Color.black);
    }
  }

  //---------------------------------------------------------------------------
  public int GetTreadling(int row)
  {
    return _treadlingData[row];
  }
  //---------------------------------------------------------------------------
  protected override void Update()
  {
    // 팝업 열려있으면 클릭 무시)
    if (palettePopup.gameObject.activeSelf)
      return;

    // 마우스 클릭 위치가 그리드 영역 내에 있는지 확인
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
    if (data.weftColorNames == null || data.weftColorNames.Length == 0) return;

    for (int i = 0; i < RowCount; i++)
    {
      var colorName = data.weftColorNames[i];
      //Debug.Log($"LoadColors: row={i}, colorName={colorName}");

      _weftColors[i] = (string.IsNullOrEmpty(colorName) || colorName == "White") ? ColorPalette.Unset : ColorPalette.GetColor(colorName);
      _drawer.FillCell(ColCount - 1, i, (Color32)_weftColors[i]);
    }
    _drawer.Apply();
  }
  //---------------------------------------------------------------------------
  public Color WeftColor(int row)
  {
    if (_weftColors == null || row < 0 || row >= _weftColors.Length)
      return Color.black;
    if (_weftColors[row] == ColorPalette.Unset)
      return Color.white;  // 미지정 위사 컬러는 흰색으로 반환.
    return _weftColors[row];
  }
  //---------------------------------------------------------------------------
}