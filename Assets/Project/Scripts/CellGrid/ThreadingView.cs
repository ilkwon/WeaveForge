using UnityEngine;
using System.Collections;

public class ThreadingView : CellGridView
{
    [SerializeField] private TieupView tieupView;
    private int[] _threadingData; // 각 경사가 몇 번 종광인지

    IEnumerator Start()
    {
        yield return null;

        int shaftCount = tieupView.RowCount;
        ColCount = tieupView.ColCount * 4;
        RowCount = shaftCount + 1; // 종광 수 + 컬러피커 1줄
        CellSize = tieupView.CellSize;

        Init();

        _threadingData = new int[ColCount];
        for (int i = 0; i < ColCount; i++)
            _threadingData[i] = -1; // 미지정

        // 위치 — 타이업 왼쪽, 컬러피커 1줄 위로 돌출
        RectTransform rt = GetComponent<RectTransform>();
        RectTransform tieupRT = tieupView.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(ColCount * CellSize, RowCount * CellSize);
        rt.anchoredPosition = new Vector2(
            tieupRT.anchoredPosition.x - tieupRT.sizeDelta.x,
            tieupRT.anchoredPosition.y + CellSize
        );
    }

    protected override void OnCellClicked(int col, int row)
    {
        if (row == 0) return; // 컬러피커 행은 무시 (추후 처리)

        int shaftRow = row; // 1부터 시작 = 종광 영역
        int prevRow = _threadingData[col];

        // 이전 점 지우기
        if (prevRow >= 0)
        {
            _drawer.FillCell(col, prevRow, new Color32(255, 255, 255, 255));
        }

        // 같은 곳 클릭하면 해제
        if (prevRow == shaftRow)
        {
            _threadingData[col] = -1;
        }
        else
        {
            _threadingData[col] = shaftRow;
            _drawer.FillCell(col, shaftRow, new Color32(0, 0, 0, 255));
        }

        _drawer.Apply();
    }

    protected override void RestoreCell(int x, int y)
    {
        if (x < 0 || y < 0) return;
        Color32 color = (_threadingData[x] == y)
            ? new Color32(0, 0, 0, 255)
            : new Color32(255, 255, 255, 255);
        _drawer.FillCell(x, y, color);
    }

    public int GetThreading(int col)
    {
        return _threadingData[col];
    }
}