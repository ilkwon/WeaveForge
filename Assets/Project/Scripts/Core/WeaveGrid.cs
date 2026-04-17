
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Color = UnityEngine.Color;
using System;

[RequireComponent(typeof(RawImage))]
public class WeaveGrid : MonoBehaviour
{
  [SerializeField] private int colCount = 8;
  [SerializeField] private int rowCount = 8;
  [SerializeField] private int cellSize = 40;
  [SerializeField] private WeaveGrid weaveGrid;
  
  private int[,] gridData;
  
  private int _paintValue = 1; // 드래그 방향: 1=칠하기, 0=지우기

  private CellDrawer _drawer;
  //-------------------------------------------------------------------------    
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    gridData = new int[RowCount, ColCount];
    _drawer = new CellDrawer(ColCount, RowCount, CellSize);
    _drawer.CreateTexture();

    GetComponent<RawImage>().texture = _drawer.Texture;
    GetComponent<RawImage>().rectTransform.sizeDelta =
        new Vector2(ColCount * CellSize, RowCount * CellSize);
  }

  //-------------------------------------------------------------------------

  public void LoadPattern(WeaveData data)
  {
    ColCount = data.coiCount;
    RowCount = data.rowCount;
    gridData = new int[RowCount, ColCount];

    for (int y = 0; y < RowCount; y++)
      for (int x = 0; x < ColCount; x++)
        gridData[y, x] = data.cells[y * ColCount + x];

    for (int y = 0; y < RowCount; y++)
      for (int x = 0; x < ColCount; x++)
        if (gridData[y, x] == 1)
          _drawer.FillCell(x, y, Color.black);

    _drawer.Apply();
    GetComponent<RawImage>().texture = _drawer.Texture;
    int displaySize = ColCount * CellSize;
    GetComponent<RawImage>().rectTransform.sizeDelta =
        new Vector2(displaySize, displaySize);
  }

  //-------------------------------------------------------------------------
  // Update is called once per frame
  void Update()
  {
    RectTransform rt = GetComponent<RectTransform>();
    Vector2 localMousePos;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rt, Mouse.current.position.ReadValue(), null, out localMousePos);


    // 마우스가 그리드 영역 밖에 있으면 아무것도 하지 않음.
    if (localMousePos.x < -rt.sizeDelta.x || localMousePos.x > 0 ||
        localMousePos.y < -rt.sizeDelta.y || localMousePos.y > 0)
    {
      //ClearCell(_hoverCell.x, _hoverCell.y);
      return;
    }

    UpdateHoverHighlight();
    UpdatePaint();
  }

  //-------------------------------------------------------------------------
  public void GetData(WeaveData data)
  {
    data.coiCount = ColCount;
    data.rowCount = RowCount;
    data.cells = new int[ColCount * RowCount];
    for (int y = 0; y < RowCount; y++)
      for (int x = 0; x < ColCount; x++)
        data.cells[y * ColCount + x] = gridData[y, x];
  }
  //-------------------------------------------------------------------------
  // 클릭시 셀의 상태를 토글하고 색상을 변경.
  Vector2Int _hoverCell = new Vector2Int(-1, -1);

  public int ColCount { get => colCount; set => colCount = value; }
  public int RowCount { get => rowCount; set => rowCount = value; }
  public int CellSize { get => cellSize; set => cellSize = value; }

  void UpdatePaint()
  {
    if (_hoverCell.x < 0) return;

    if (!Mouse.current.leftButton.isPressed)
    {
      var old = gridData[_hoverCell.y, _hoverCell.x];
      _paintValue = old == 1 ? 0 : 1;
    }
    if (!Mouse.current.leftButton.isPressed) return;

    gridData[_hoverCell.y, _hoverCell.x] = _paintValue;
    Color color = _paintValue == 1 ? Color.black : Color.white;
    _drawer.FillCell(_hoverCell.x, _hoverCell.y, color);

    _drawer.Apply();
  }
  //-------------------------------------------------------------------------
  private void UpdateHoverHighlight()
  {
    Vector2 localPos;
    RectTransform rt = GetComponent<RectTransform>();
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rt, Mouse.current.position.ReadValue(), null, out localPos);

    float adjustedX = localPos.x + rt.sizeDelta.x;
    float adjustedY = localPos.y + rt.sizeDelta.y;

    int cx = (int)(adjustedX / CellSize);
    int cy = (int)(adjustedY / CellSize);
    if (cx < 0 || cx >= ColCount || cy < 0 || cy >= RowCount) return;

    Vector2Int curr = new Vector2Int(cx, cy);
    if (_hoverCell != curr)
    {
      RestoreCell(_hoverCell.x, _hoverCell.y);
      _hoverCell = curr;
      Color32 highlightColor = gridData[curr.y, curr.x] == 1
        ? new Color32(80, 60, 120, 255)   // 보라색 (칠해진 셀)
        : new Color32(217, 217, 217, 255); // 회색 (빈 셀)
      _drawer.FillCell(curr.x, curr.y, highlightColor);
      _drawer.Apply();
    }
  }

  //-------------------------------------------------------------------------
  void RestoreCell(int x, int y)
{
    if (x < 0 || y < 0) return;
    Color32 color = gridData[y, x] == 1
        ? new Color32(0, 0, 0, 255)
        : new Color32(255, 255, 255, 255);
    _drawer.FillCell(x, y, color);
}
  //-------------------------------------------------------------------------
  public void Resize(int w, int h)
  {
    ColCount = w;
    RowCount = h;

    gridData = new int[ColCount, RowCount];
 
    _drawer = new CellDrawer(ColCount, RowCount, CellSize);
    _drawer.CreateTexture();
    
    GetComponent<RawImage>().texture = _drawer.Texture;
    int displaySize = ColCount * CellSize;
    GetComponent<RawImage>().rectTransform.sizeDelta =
      new Vector2(displaySize, displaySize);

    _hoverCell = new Vector2Int(-1, -1);
  }
  //-------------------------------------------------------------------------

}
