using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class WarpStripView : MonoBehaviour
{
  [SerializeField] private WeaveGrid weaveDisplay;
  [SerializeField] private Font font;
  private FontRenderer _fontRenderer;

  private CellDrawer _drawer;
  private int cellSize;
  private int repeatX;
  private Texture2D texture;
  private Color[] warpColors;

  //-------------------------------------------------------------------------
  IEnumerator Start()
  {
    yield return null;

    int colCount = weaveDisplay.ColCount * 4;
    int rowCount = weaveDisplay.RowCount + 1; // 종광 수 + 컬러피커 1줄
    int cellSize = weaveDisplay.CellSize;

    _drawer = new CellDrawer(colCount, rowCount, cellSize);
    _drawer.CreateTexture();
    

    GetComponent<RawImage>().texture = _drawer.Texture;
    GetComponent<RawImage>().rectTransform.sizeDelta =
      new Vector2(colCount * cellSize, rowCount * cellSize);

    RawImage rawImage = GetComponent<RawImage>();
    rawImage.texture = _drawer.Texture;

    RectTransform rt = GetComponent<RectTransform>();
    RectTransform tieupRT = weaveDisplay.GetComponent<RectTransform>();

    // 피벗, 앵커 티어업과 동일
    rt.anchorMin = new Vector2(1, 1);
    rt.anchorMax = new Vector2(1, 1);
    rt.pivot = new Vector2(1, 1);

    // 크기
    rt.sizeDelta = new Vector2(colCount * cellSize, rowCount * cellSize);
    
    // 위치 — 타이업 왼쪽에 붙이기
    rt.anchoredPosition = new Vector2(
        tieupRT.anchoredPosition.x - tieupRT.sizeDelta.x,
        tieupRT.anchoredPosition.y + cellSize // 컬러피커 한칸만큼 올려서 붙이기
    );

    _fontRenderer = new FontRenderer(_drawer, font);
    _fontRenderer.PrepareNumbers(1, weaveDisplay.RowCount);
    TestFont();
  }

  private void TestFont()
  {
    // 테스트 — 통경 셀에 1~8 찍기
    for (int i = 0; i < weaveDisplay.RowCount; i++)
    {
      _fontRenderer.DrawNumber(i, i + 1, i + 1, Color.black);
    }
    _drawer.Apply();
  }

  public void Init(int rX, Color[] colors)
  {
    repeatX = rX;
    warpColors = colors;
    int w = (repeatX + 1) * cellSize + 1;
    int h = cellSize + 1;
    texture = new Texture2D(w, h);
    texture.filterMode = FilterMode.Point;
    WarpRepaint();
    texture.Apply();
    GetComponent<RawImage>().texture = texture;
  }

  public void UpdateColor(int index, Color color)
  {
    warpColors[index] = color;
    WarpPaintOne(index + 1, color);
    texture.Apply();
  }

  private void WarpRepaint()
  {
    for (int x = 0; x < texture.width; x++)
      for (int y = 0; y < texture.height; y++)
        texture.SetPixel(x, y, Color.white);
    Color line = new Color(0.7f, 0.7f, 0.7f);
    for (int i = 0; i <= repeatX + 1; i++)
    {
      int x = i * cellSize;
      for (int y = 0; y < texture.height; y++)
        texture.SetPixel(x, y, line);
    }
    for (int y = 0; y < texture.height; y += cellSize)
      for (int x = 0; x < texture.width; x++)
        texture.SetPixel(x, y, line);
    WarpPaintOne(0, new Color(0.75f, 0.75f, 0.75f));
    for (int i = 0; i < repeatX; i++)
      WarpPaintOne(i + 1, warpColors[i]);
  }

  private void WarpPaintOne(int index, Color color)
  {
    int sx = index * cellSize + 1;
    int sy = 1;
    for (int px = sx; px < sx + cellSize - 1; px++)
      for (int py = sy; py < sy + cellSize - 1; py++)
        texture.SetPixel(px, py, color);
  }
}