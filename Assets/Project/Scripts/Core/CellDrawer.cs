using UnityEngine;

public class CellDrawer
{
  public Texture2D Texture { get; private set; }
  public int ColCount { get; private set; }
  public int RowCount { get; private set; }
  public int CellSize { get; private set; }

  private Color32[] _pixelBuffer;
  //--------------------------------------------------------------------------
  public CellDrawer(int colCount, int rowCount, int cellSize)
  {
    ColCount = colCount;
    RowCount = rowCount;
    CellSize = cellSize;
  }
  //--------------------------------------------------------------------------
  public void CreateTexture()
  {
    int width = ColCount * CellSize + 1;
    int height = RowCount * CellSize + 1;
    Texture = new Texture2D(width, height)
    {
      filterMode = FilterMode.Point
    };
    _pixelBuffer = new Color32[width * height];
    DrawGrid();
  }
  //--------------------------------------------------------------------------
  public void DrawGrid()
  {
    int width = Texture.width;
    int height = Texture.height;

    Color32 white = new Color32(255, 255, 255, 255);
    Color32 lineColor = new Color32(179, 179, 179, 255);

    for (int i = 0; i < _pixelBuffer.Length; i++)
      _pixelBuffer[i] = white;

    for (int col = 0; col <= ColCount; col++)
    {
      int x = col * CellSize;
      for (int y = 0; y < height; y++)
        _pixelBuffer[y * width + x] = lineColor;
    }

    for (int row = 0; row <= RowCount; row++)
    {
      int y = row * CellSize;
      for (int x = 0; x < width; x++)
        _pixelBuffer[y * width + x] = lineColor;
    }

    Apply();
  }
  //--------------------------------------------------------------------------
  public void FillCell(int col, int row, Color32 color)
  {
    int width = Texture.width;
    int startX = col * CellSize + 1;
    int startY = row * CellSize + 1;

    for (int y = startY; y < startY + CellSize - 1; y++)
      for (int x = startX; x < startX + CellSize - 1; x++)
        _pixelBuffer[y * width + x] = color;
  }
  //--------------------------------------------------------------------------
  public void HighlightCell(int col, int row)
  {
    FillCell(col, row, new Color32(217, 217, 217, 255));
  }
  //--------------------------------------------------------------------------
  public void Apply()
  {
    Texture.SetPixels32(_pixelBuffer);
    Texture.Apply();
  }

  public void ClearCell(int col, int row, Color32 color)
  {
    if (col < 0 || row < 0) return;
    FillCell(col, row, color);
  }

  public void SetPixel(int x, int y, Color32 color)
  {
    int width = Texture.width;
        //if (x < 0 || x >= width || y < 0 || y >= Texture.height) return;
    _pixelBuffer[y * width + x] = color;
  }
}