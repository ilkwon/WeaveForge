using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
/// <summary>
/// 직물조직 뷰 : 타이업, 트레딩, 트레들링 뷰를 종합하여 직물 조직도를 보여준다.
/// 타이업, 트레딩, 트레들링 뷰를 참조하여 그린다. 타이업, 트레딩, 트레들링 
/// 뷰의 패턴이 변경되면 그에 맞춰 다시 그린다.
/// </summary>
//---------------------------------------------------------------------------
public class DrawdownView : CellGridView
{

  [SerializeField] TieupView tieupView;

  [SerializeField] ThreadingView threadingView;

  [SerializeField] TreadlingView treadlingView;
  IEnumerator Start()
  {
    yield return null;

    CellSize = tieupView.CellSize;

    tieupView.OnPatternLoaded += Resize;

  }

  private void Resize()
  {
    ColCount = tieupView.ColCount * 4;
    RowCount = tieupView.RowCount * 4;
    CellSize = tieupView.CellSize;

    Init();
    UpdatePosition();
    
  }

  private void UpdatePosition()
  {
    RectTransform rt = GetComponent<RectTransform>();
    RectTransform tieupRT = tieupView.GetComponent<RectTransform>();

    rt.anchorMin = new Vector2(1f, 1f);
    rt.anchorMax = new Vector2(1f, 1f);
    rt.pivot = new Vector2(1f, 1f);
    rt.sizeDelta = new Vector2(ColCount * CellSize, RowCount * CellSize);
    rt.anchoredPosition = new Vector2(
        tieupRT.anchoredPosition.x - tieupRT.sizeDelta.x,
        tieupRT.anchoredPosition.y - tieupRT.sizeDelta.y
    );
  }
}