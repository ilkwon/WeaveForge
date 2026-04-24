using System;
using UnityEngine;
using UnityEngine.UI;

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
  }

}
