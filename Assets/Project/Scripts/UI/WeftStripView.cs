using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class WeftStripView : MonoBehaviour
{
  [SerializeField] private WeaveGrid weaveDisplay;
  private CellDrawer _drawer;

  //-------------------------------------------------------------------------
  IEnumerator Start()
  {
    yield return null;

    int colCount = weaveDisplay.ColCount + 1; // 트레들 수 + 컬러피커 1줄
    int rowCount = weaveDisplay.RowCount * 4; // 위사 수
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
    
    // 위치 — 타이업 아래 붙이기
    rt.anchoredPosition = new Vector2(
        tieupRT.anchoredPosition.x + cellSize,
        tieupRT.anchoredPosition.y - tieupRT.sizeDelta.y
    );
  }
  
  //-------------------------------------------------------------------------
  
}