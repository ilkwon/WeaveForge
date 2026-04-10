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
  public void Show(Action<string> callback)
  {
    onColorSelected = callback;
    gameObject.SetActive(true);
  }

}
