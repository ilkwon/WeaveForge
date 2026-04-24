using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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
  //---------------------------------------------------------------------------
  [SerializeField] TieupView tieupView;
  [SerializeField] ThreadingView threadingView;
  [SerializeField] TreadlingView treadlingView;
  //---------------------------------------------------------------------------
  IEnumerator Start()
  {
    yield return null;

    CellSize = tieupView.CellSize;

    tieupView.OnPatternLoaded += Resize;

  }

  //---------------------------------------------------------------------------
  private void Resize()
  {
    ColCount = tieupView.ColCount * 4;
    RowCount = tieupView.RowCount * 4;
    CellSize = tieupView.CellSize;

    Init();
    UpdatePosition();

    StartCoroutine(RecalculateNextFrame());
  }
  
  //---------------------------------------------------------------------------
  private IEnumerator RecalculateNextFrame()
  {
    yield return null; // 다음 프레임까지 대기
    Recalculate();
  }

  //---------------------------------------------------------------------------
  private void UpdatePosition()
  {
    RectTransform rt = GetComponent<RectTransform>();
    RectTransform tieupRT = tieupView.GetComponent<RectTransform>();

    rt.anchorMin = new Vector2(1f, 1f);
    rt.anchorMax = new Vector2(1f, 1f);
    rt.pivot = new Vector2(1f, 1f);
    rt.sizeDelta = new Vector2(ColCount * CellSize, RowCount * CellSize);
    // 
    rt.anchoredPosition = new Vector2(
        tieupRT.anchoredPosition.x - tieupRT.sizeDelta.x,
        tieupRT.anchoredPosition.y - tieupRT.sizeDelta.y
    );
  }

  //---------------------------------------------------------------------------
  private void Recalculate()
  {
    var warpCount = threadingView.ColCount; // 경사 헤더 컬럼 수
    var weftCount = treadlingView.RowCount; // 위사 헤더 행 수

    for (int x = 0; x < warpCount; x++)
    {
      for (int y = 0; y < weftCount; y++)
      {
        int shaft = threadingView.GetThreading(x);    // x열의 종광 번호
        int treadle = treadlingView.GetTreadling(y);  // y행의 트레들 번호

        if (shaft < 1 || shaft > tieupView.RowCount) { _drawer.FillCell(x, y, Color.white); continue; }
        if (treadle < 1 || treadle > tieupView.ColCount) { _drawer.FillCell(x, y, Color.white); continue; }

        var result = tieupView.GetCell(treadle - 1, shaft - 1); // 타이up은 0-based 인덱스
        Color color = result == 1 ? Color.black : Color.white; // 타이업이 1이면 검정, 아니면 흰색
        _drawer.FillCell(x, y, color);
      }
    }
    _drawer.Apply();
  }

  //---------------------------------------------------------------------------
  private void Update()
  {
    RectTransform rt = GetComponent<RectTransform>();
    Vector2 localMousePos;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rt, Mouse.current.position.ReadValue(), null, out localMousePos);

    if (localMousePos.x < -rt.sizeDelta.x || localMousePos.x > 0 ||
        localMousePos.y < -rt.sizeDelta.y || localMousePos.y > 0)
      return;

    int cx = (int)((localMousePos.x + rt.sizeDelta.x) / CellSize);
    int cy = (int)((localMousePos.y + rt.sizeDelta.y) / CellSize);

    if (Mouse.current.leftButton.wasPressedThisFrame)
      OnCellClicked(cx, cy);
  }
}