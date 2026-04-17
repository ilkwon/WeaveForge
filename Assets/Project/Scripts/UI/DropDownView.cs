using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class DrawdownView : MonoBehaviour
{
    [SerializeField] private WeaveGrid weaveDisplay;
    private CellDrawer _drawer;

    IEnumerator Start()
    {
        yield return null;

        int colCount = weaveDisplay.ColCount * 4; // 트레들 수 * 4 (반복 수)
        int rowCount = weaveDisplay.RowCount * 4; // 트레들 수 * 4 (반복 수)
        int cellSize = weaveDisplay.CellSize;

        _drawer = new CellDrawer(colCount, rowCount, cellSize);
        _drawer.CreateTexture();

        RawImage rawImage = GetComponent<RawImage>();
        rawImage.texture = _drawer.Texture;

        RectTransform rt = GetComponent<RectTransform>();
        RectTransform tieupRT = weaveDisplay.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);

        rt.sizeDelta = new Vector2(colCount * cellSize, rowCount * cellSize);

        // 위치 — 경사 헤더 아래, 위사 헤더 왼쪽
        rt.anchoredPosition = new Vector2(
            tieupRT.anchoredPosition.x - tieupRT.sizeDelta.x,
            tieupRT.anchoredPosition.y - tieupRT.sizeDelta.y
        );
    }
}