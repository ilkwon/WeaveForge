using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum CellDrawType
{
  Dot,    // 행 위치로 값 표현 (점)
  Number  // 셀 안에 숫자로 값 표현
}

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(RawImage))]
public class CellGridView : MonoBehaviour
{
  [SerializeField] protected int colCount = 0;
  [SerializeField] protected int rowCount = 0;
  [SerializeField] protected int cellSize = 40;
  [SerializeField] protected Font font;

  protected CellDrawer _drawer;
  protected FontRenderer _fontRenderer;
  protected Vector2Int _hoverCell = new Vector2Int(-1, -1);
  protected CellDrawType drawType = CellDrawType.Dot;
  public int ColCount { get => colCount; set => colCount = value; }
  public int RowCount { get => rowCount; set => rowCount = value; }
  public int CellSize { get => cellSize; set => cellSize = value; }

  //---------------------------------------------------------------------------
  protected virtual void OnDestroy()
  {
    if (_drawer != null && _drawer.Texture != null)
    {
      Destroy(_drawer.Texture);
      _drawer = null;
    }
  }
  //---------------------------------------------------------------------------
  protected virtual void Init()
  {
    if (_drawer != null && _drawer.Texture != null)
      Destroy(_drawer.Texture);

    _drawer = new CellDrawer(ColCount, RowCount, CellSize);
    _drawer.CreateTexture();

    if (font != null)
      _fontRenderer = new FontRenderer(_drawer, font);

    _hoverCell = new Vector2Int(-1, -1);

    RawImage rawImage = GetComponent<RawImage>();
    rawImage.texture = _drawer.Texture;
    GetComponent<RectTransform>().sizeDelta =
      new Vector2(ColCount * CellSize, RowCount * CellSize);
  }
  //---------------------------------------------------------------------------
  private Vector2Int _lastPos = new Vector2Int(-1, -1);
  private int _drawCell = 0;
  protected bool _applyFlag = false;
  protected virtual void Update()
  {
    _applyFlag = false;
    RectTransform rt = GetComponent<RectTransform>();
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rt, Mouse.current.position.ReadValue(), null, out Vector2 localMousePos);

    if (localMousePos.x < -rt.sizeDelta.x || localMousePos.x > 0 ||
        localMousePos.y < -rt.sizeDelta.y || localMousePos.y > 0)
      return;

    UpdateHover(localMousePos);

    UpdateMouse(localMousePos);

    if (_applyFlag)
      _drawer.Apply();
  }
  //---------------------------------------------------------------------------
  private void UpdateMouse(Vector2 localMousePos)
  {
    bool firstClick = Mouse.current.leftButton.wasPressedThisFrame && Mouse.current.leftButton.isPressed;
    bool dragging = Mouse.current.leftButton.isPressed && !firstClick;
    if (firstClick &&
      _hoverCell.x >= 0 &&  _hoverCell != _lastPos)
    { 
      _drawCell = GetCellValue(_hoverCell.x, _hoverCell.y) == 1 ? 0 : 1;              
      OnCellClicked(_hoverCell.x, _hoverCell.y);
      _lastPos = _hoverCell;
    }
    else if (dragging && _hoverCell.x >= 0)
    {      
      OnCellDrag(_hoverCell.x, _hoverCell.y, _drawCell);
      _lastPos = _hoverCell;
    }
    // 마우스 버튼이 떼어졌을 때 위치 초기화
    else if (Mouse.current.leftButton.wasReleasedThisFrame)
    {      
        _lastPos = new Vector2Int(-1, -1);
        //Debug.Log("Mouse down at: " + localMousePos);
    }
  }

  //---------------------------------------------------------------------------
  
  private void UpdateHover(Vector2 localPos)
  {
    RectTransform rt = GetComponent<RectTransform>();

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
      _drawer.HighlightCell(curr.x, curr.y);
      //_drawer.Apply();
      _applyFlag = true;
    }
  }

  //---------------------------------------------------------------------------
  protected virtual void RestoreCell(int x, int y)
  {
    if (x < 0 || y < 0) return;
    _drawer.FillCell(x, y, new Color32(255, 255, 255, 255));
  }
  //---------------------------------------------------------------------------
  protected virtual void OnCellClicked(int col, int row) { }
  protected virtual void OnCellDrag(int col, int row, int dragValue) { }
  //---------------------------------------------------------------------------
  protected virtual int GetCellValue(int col, int row) { return 0; }
}
