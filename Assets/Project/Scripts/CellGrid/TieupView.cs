using UnityEngine;

public class TieupView : CellGridView
{
  private int[,] _gridData;
  private int _painValue = 1;
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
  void Start()
  {
    Init();
  }

  //---------------------------------------------------------------------------
  protected override void OnCellClicked(int col, int row)
  {
    var old = _gridData[row, col];
    _gridData[row, col] = old == 1 ? 0 : 1;
    Color32 color = _gridData[row, col] == 1
        ? new Color32(0, 0, 0, 255)
        : new Color32(255, 255, 255, 255);
    _drawer.FillCell(col, row, color);
    _drawer.Apply();
  }
  //---------------------------------------------------------------------------
  protected override void RestoreCell(int x, int y)
  {
    if (x < 0 || y < 0) return;
    Color32 color = _gridData[y, x] == 1
        ? new Color32(0, 0, 0, 255)
        : new Color32(255, 255, 255, 255);
    _drawer.FillCell(x, y, color);
  }
  //---------------------------------------------------------------------------
  public int GetCell(int col, int row)
  {
    return _gridData[row, col];
  }

  //---------------------------------------------------------------------------
  public void LoadPattern(WeaveData data)
  {
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
  }

}
