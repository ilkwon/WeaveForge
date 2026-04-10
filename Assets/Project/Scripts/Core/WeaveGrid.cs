
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Color = UnityEngine.Color;

public class WeaveGrid : MonoBehaviour
{
  [SerializeField] private int repeatX = 8;
  [SerializeField] private int repeatY = 8;
  [SerializeField] private int cellSize = 50;
  Texture2D gridTexture;
  private int[,] gridData;
  private int textureSize;
  private int _paintValue = 1; // 드래그 방향: 1=칠하기, 0=지우기
  //-------------------------------------------------------------------------    
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    gridData = new int[repeatY, repeatX];

    textureSize = repeatX * cellSize + 1;
    gridTexture = new Texture2D(textureSize, textureSize)
    {
      filterMode = FilterMode.Point // 픽셀 아트 스타일을 위해 필터 모드 설정
    };

    DrawInitialGrid();
    gridTexture.Apply();
    // RawImage에 적용
    GetComponent<RawImage>().texture = gridTexture;

    // RawImage의 크기를 텍스처 크기에 맞게 조정
    int displaySize = repeatX * cellSize;
    GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(displaySize, displaySize);
  }
  //-------------------------------------------------------------------------
  void DrawInitialGrid()
  {
    // 배경 흰색
    for (int x = 0; x < textureSize; x++)
      for (int y = 0; y < textureSize; y++)
        gridTexture.SetPixel(x, y, Color.white);

    Color lineColor = new Color(0.7f, 0.7f, 0.7f);

    // 세로선
    for (int col = 0; col <= repeatX; col++)
    {
      int x = col * cellSize;
      for (int y = 0; y < textureSize; y++)
        gridTexture.SetPixel(x, y, lineColor);
    }

    // 가로선
    for (int row = 0; row <= repeatY; row++)
    {
      int y = row * cellSize;
      for (int x = 0; x < textureSize; x++)
        gridTexture.SetPixel(x, y, lineColor);
    }
  }
  //-------------------------------------------------------------------------
  public void LoadPattern(WeaveData data)
  {
    repeatX = data.repeatX;
    repeatY = data.repeatY;
    gridData = new int[repeatY, repeatX];

    for (int y = 0; y < repeatY; y++)
      for (int x = 0; x < repeatX; x++)
        gridData[y, x] = data.cells[y * repeatX + x];

    textureSize = repeatX * cellSize + 1;
    gridTexture = new Texture2D(textureSize, textureSize)
    {
      filterMode = FilterMode.Point
    };

    DrawInitialGrid();

    for (int y = 0; y < repeatY; y++)
      for (int x = 0; x < repeatX; x++)
        if (gridData[y, x] == 1)
          FillCell(x, y, Color.black);

    gridTexture.Apply();
    GetComponent<RawImage>().texture = gridTexture;
    int displaySize = repeatX * cellSize;
    GetComponent<RawImage>().rectTransform.sizeDelta =
        new Vector2(displaySize, displaySize);
  }
  //-------------------------------------------------------------------------
  public void Clear()
  {
    for (int y = 0; y < repeatY; y++)
      for (int x = 0; x < repeatX; x++)
        gridData[y, x] = 0;

    DrawInitialGrid();
    gridTexture.Apply();
  }
  //-------------------------------------------------------------------------
  // Update is called once per frame
  void Update()
  {
    RectTransform rt = GetComponent<RectTransform>();
    Vector2 localMousePos;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rt, Mouse.current.position.ReadValue(), null, out localMousePos);

    Vector2 half = rt.sizeDelta / 2f;
    // 마우스가 그리드 영역 밖에 있으면 아무것도 하지 않음.
    if (localMousePos.x < -half.x || localMousePos.x > half.x ||
        localMousePos.y < -half.y || localMousePos.y > half.y)
    {
      //ClearCell(_hoverCell.x, _hoverCell.y);
      return;
    }

    UpdateHoverHighlight();
    UpdatePaint();
  }

  public void GetData(WeaveData data)
  {
    data.repeatX = repeatX;
    data.repeatY = repeatY;
    data.cells = new int[repeatX * repeatY];
    for (int y = 0; y < repeatY; y++)
      for (int x = 0; x < repeatX; x++)
        data.cells[y * repeatX + x] = gridData[y, x];
  }
  //-------------------------------------------------------------------------
  // 클릭시 셀의 상태를 토글하고 색상을 변경.
  Vector2Int _hoverCell = new Vector2Int(-1, -1);
  void UpdatePaint()
  {
    if (_hoverCell.x < 0) return;

    if (!Mouse.current.leftButton.isPressed)
    {
      var old = gridData[_hoverCell.y, _hoverCell.x];
      _paintValue = old == 1 ? 0 : 1;
    }
    if (!Mouse.current.leftButton.isPressed) return;

    gridData[_hoverCell.y, _hoverCell.x] = _paintValue;
    Color color = _paintValue == 1 ? Color.black : Color.white;
    FillCell(_hoverCell.x, _hoverCell.y, color);

    gridTexture.Apply();
  }
  //-------------------------------------------------------------------------
  private void UpdateHoverHighlight()
  {
    Vector2 localPos;
    RectTransform rt = GetComponent<RectTransform>();
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rt, Mouse.current.position.ReadValue(), null, out localPos);
    localPos += rt.sizeDelta / 2f;

    int cx = (int)(localPos.x / cellSize);
    int cy = (int)(localPos.y / cellSize);
    if (cx < 0 || cx >= repeatX || cy < 0 || cy >= repeatY) return;

    Vector2Int curr = new Vector2Int(cx, cy);
    if (_hoverCell != curr)
    {
      ClearCell(_hoverCell.x, _hoverCell.y);
      _hoverCell = curr;
      HighlightCell(curr.x, curr.y);
      gridTexture.Apply();
    }
  }
  //-------------------------------------------------------------------------
  void ClearCell(int x, int y)
  {
    if (x < 0 || y < 0) return;

    Color color = gridData[y, x] == 1 ? Color.black : Color.white;
    FillCell(x, y, color);
  }
  //-------------------------------------------------------------------------
  void HighlightCell(int x, int y)
  {
    FillCell(x, y, new Color(0.85f, 0.85f, 0.85f));
  }
  //-------------------------------------------------------------------------
  // 셀 한칸은  cellSize만큼 픽셀로 채워야 한다. (선 제외)
  void FillCell(int x, int y, Color color)
  {
    int startX = x * cellSize + 1;
    int startY = y * cellSize + 1;

    for (int px = startX; px < startX + cellSize - 1; px++)
      for (int py = startY; py < startY + cellSize - 1; py++)
        gridTexture.SetPixel(px, py, color);
  }
  //-------------------------------------------------------------------------

}
