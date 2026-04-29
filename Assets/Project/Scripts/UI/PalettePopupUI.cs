using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PalettePopupUI : MonoBehaviour
{
  [SerializeField] private GameObject colorCellPrefab;
  private Action<string> onColorSelected;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    Build();
    gameObject.SetActive(false);
  }

  private void Build()
  {
    foreach (Transform child in transform)
      Destroy(child.gameObject);

    foreach (var useful in ColorPalette.Colors)
    {
      string captured = useful.colorName;
      GameObject cell = Instantiate(colorCellPrefab, transform);
      cell.GetComponent<Image>().color = useful.color;
      cell.GetComponent<Button>().onClick.AddListener(() =>
      {
        onColorSelected?.Invoke(captured);
        gameObject.SetActive(false);
      });
    }
  }

  //----------------------------------------------------------------------
  private bool _jusetOpened = false;  // 팝업이 방금 열렸는지 여부. 열리자마자 닫히는 것을 방지하기 위해 사용
  public void Show(Action<string> callback, Vector2 screenPosition)
  {
    onColorSelected = callback;

    RectTransform rt = GetComponent<RectTransform>();

    float popupWidth = rt.sizeDelta.x;
    float popupHeight = rt.sizeDelta.y;

    float x = screenPosition.x;
    float y = screenPosition.y - popupHeight;

    // 오른쪽 밖으로 나가면 왼쪽으로 당기기
    if (x + popupWidth > Screen.width)
      x = Screen.width - popupWidth;

    // 왼쪽 밖으로 나가면 오른쪽으로 당기기
    if (x < 0)
      x = 0;

    // 아래쪽 밖으로 나가면 위로 올리기
    if (y - popupHeight < 0)
      y = popupHeight;

    // 위쪽 밖으로 나가면 아래로 내리기
    if (y > Screen.height)
      y = Screen.height;

    rt.position = new Vector3(x, y, 0);

    gameObject.SetActive(true);
    _jusetOpened = true;
  }
  //-------------------------------------------------------------------------
  private void Update()
  {
    if (_jusetOpened)
    {
      _jusetOpened = false;
      return; // 팝업이 열리자마자 닫히는 것을 방지
    }

    // 팝업이 열려있을 때는 ESC 키나 팝업 외부 클릭으로 닫기
    if (Keyboard.current.escapeKey.wasPressedThisFrame)
      gameObject.SetActive(false);

    // 팝업 외부 클릭 감지
    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
      Vector2 mousePos = Mouse.current.position.ReadValue();
      if (!RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(), mousePos, null))
      {
        gameObject.SetActive(false);
      }
    }
  }
}
