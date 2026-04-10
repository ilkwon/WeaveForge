using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ColorStripUI : MonoBehaviour
{
    [SerializeField] private Transform stripContent;
    [SerializeField] private GameObject colorCellPrefab;
    [SerializeField] private PalettePopupUI palattePopup;

    private List<string> colorNames = new List<string>();
    public List<string> GetColorNames() => colorNames;

    public void Setup(int repeatCount)
  {
    colorNames.Clear();
    for (int i=0; i < repeatCount; i++) {
      colorNames.Add("White");
    }
    Refresh();
  }

  private void Refresh()
  {
    foreach (Transform child in stripContent)
      Destroy(child.gameObject);
    
    for (int i = 0; i<colorNames.Count; i++)
    {
      int captured = i;
      GameObject cell = Instantiate(colorCellPrefab, stripContent);
      cell.GetComponent<Image>().color = ColorPalette.GetColor(colorNames[i]);
      cell.GetComponent<Button>().onClick.AddListener(() =>
      {
        OnCellClick(captured);
      });
    }
  }

  private void OnCellClick(int index)
  {
    palattePopup.Show((colorName) =>
    {
      colorNames[index] = colorName;
      Refresh();
    });
  }
}