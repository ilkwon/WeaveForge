using System;
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
  protected virtual void Update()
  {
    RectTransform rt = GetComponent<RectTransform>();
    Vector2 localMousePos;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rt, Mouse.current.position.ReadValue(), null, out localMousePos);

    if (localMousePos.x < -rt.sizeDelta.x || localMousePos.x > 0 ||
        localMousePos.y < -rt.sizeDelta.y || localMousePos.y > 0)
      return;

    UpdateHover();

    if (Mouse.current.leftButton.wasPressedThisFrame && _hoverCell.x >= 0)
      OnCellClicked(_hoverCell.x, _hoverCell.y);
  }

  //---------------------------------------------------------------------------
  private void UpdateHover()
  {
    RectTransform rt = GetComponent<RectTransform>();
    Vector2 localPos;
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
      _drawer.HighlightCell(curr.x, curr.y);
      _drawer.Apply();
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
  //---------------------------------------------------------------------------

}
