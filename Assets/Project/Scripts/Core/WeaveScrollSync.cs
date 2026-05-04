using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// 드로다운 스크롤 처리
///   휠       → 세로 스크롤 (드로다운 + 트레들링 동기화)
///   Shift+휠 → 가로 스크롤 (드로다운만, 통경 고정)
/// </summary>
public class WeaveScrollSync : MonoBehaviour
{
  [SerializeField] private ScrollRect  scrollRect;
  [SerializeField] private RectTransform weftStripRT;  // 트레들링 - 세로 동기화
  [SerializeField] private float sensitivity = 40f;
  [SerializeField] private TieupView tieupView;
  [SerializeField] private int zoomStep = 4;
  [SerializeField] private int zoomMin  = 10;
  [SerializeField] private int zoomMax  = 80;
  [SerializeField] private int defaultCellSize = 40;
  //-------------------------------------------------------------------------
  private void Start()
  {
    if (scrollRect == null) return;
    scrollRect.horizontal = false;
    scrollRect.vertical   = false;
  }

  //-------------------------------------------------------------------------
  public void ResetSync() { }  // 패턴 변경 시 호출 (현재 상태 유지)

  //-------------------------------------------------------------------------
  private void Update()
  {
    if (scrollRect == null) return;

    bool ctrlHeld = Keyboard.current.leftCtrlKey.isPressed ||
                   Keyboard.current.rightCtrlKey.isPressed;
    if (ctrlHeld) { HandleZoom(); return; }

    float input = ReadScrollInput(out bool isHorizontal); 
    if (Mathf.Approximately(input, 0f)) return; // 입력 없음

    if (isHorizontal) ScrollHorizontal(input);
    else              ScrollVertical(input);
  }
  //-------------------------------------------------------------------------
  private void HandleZoom()
  {
    // Ctrl+0 → 기본 크기로 리셋
    if (Keyboard.current.digit0Key.wasPressedThisFrame)
    {
      tieupView.CellSize = defaultCellSize;
      tieupView.FirePatternLoaded();
      return;
    }

    float scroll = Mouse.current.scroll.ReadValue().y;
    if (Mathf.Approximately(scroll, 0f)) return;

    int newSize = Mathf.Clamp(tieupView.CellSize + (int)(Mathf.Sign(scroll) * zoomStep),
      zoomMin, zoomMax
    ); // 10 단위로 조절
    if (newSize == tieupView.CellSize) return; // 변경 없음

    tieupView.CellSize = newSize;
    tieupView.FirePatternLoaded();    
  }

  //-------------------------------------------------------------------------
  /// <summary>
  /// 스크롤 방향 반환 (-1 or +1), isHorizontal = Shift 눌림 여부
  /// macOS: Shift+휠 → OS가 x축으로 변환
  /// </summary>
  private float ReadScrollInput(out bool isHorizontal)
  {
    var scroll = Mouse.current.scroll.ReadValue();

    isHorizontal = Keyboard.current.leftShiftKey.isPressed ||
                   Keyboard.current.rightShiftKey.isPressed;

    float raw = isHorizontal
        ? (Mathf.Approximately(scroll.x, 0f) ? scroll.y : scroll.x)
        : scroll.y;

    return Mathf.Sign(raw) * (Mathf.Approximately(raw, 0f) ? 0f : 1f);
  }

  //-------------------------------------------------------------------------
  // 스크롤 처리
  //-------------------------------------------------------------------------

  private void ScrollHorizontal(float direction)
  {
    var content  = scrollRect.content;
    var viewport = scrollRect.viewport;

    float max = Mathf.Max(0, content.sizeDelta.x - viewport.rect.width);
    float newX = Mathf.Clamp(content.anchoredPosition.x + direction * sensitivity, 0, max);

    content.anchoredPosition = new Vector2(newX, content.anchoredPosition.y);
  }

  private void ScrollVertical(float direction)
  {
    var content  = scrollRect.content;
    var viewport = scrollRect.viewport;

    float max     = Mathf.Max(0, content.sizeDelta.y - viewport.rect.height);
    float prevY   = content.anchoredPosition.y;
    float newY    = Mathf.Clamp(prevY + direction * sensitivity, 0, max);

    content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);

    // 트레들링 동기화
    float deltaY = newY - prevY;
    if (weftStripRT != null)
      weftStripRT.anchoredPosition += new Vector2(0, deltaY);
  }
}