using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class WarpStripView : MonoBehaviour
{
    [SerializeField] private WeaveGrid weaveDisplay;
    private int cellSize;
    private int repeatX;
    private Texture2D texture;
    private Color[] warpColors;

    IEnumerator Start()
    {
        yield return null;
        cellSize = weaveDisplay.CellSize;
        repeatX  = weaveDisplay.RepeatX;
        RectTransform wdRt = weaveDisplay.GetComponent<RectTransform>();
        RectTransform rt   = GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(
            wdRt.anchoredPosition.x + wdRt.sizeDelta.x,
            wdRt.anchoredPosition.y
        );
        rt.sizeDelta = new Vector2((repeatX + 1) * cellSize, cellSize);
        warpColors = new Color[repeatX];
        for (int i = 0; i < repeatX; i++)
            warpColors[i] = Color.white;
        Init(repeatX, warpColors);
    }

    public void Init(int rX, Color[] colors)
    {
        repeatX    = rX;
        warpColors = colors;
        int w = (repeatX + 1) * cellSize + 1;
        int h = cellSize + 1;
        texture            = new Texture2D(w, h);
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