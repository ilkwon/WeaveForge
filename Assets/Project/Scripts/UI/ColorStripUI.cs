using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;

public class ColorStripUI : MonoBehaviour
{
  //-------------------------------------------------------------------------
  [SerializeField] private Transform stripContent;
  [SerializeField] private GameObject colorCellPrefab;
  [SerializeField] private PalettePopupUI palattePopup;
  //-------------------------------------------------------------------------
  private List<string> colorNames = new List<string>();
  private bool isWarp;

  public List<string> GetColorNames() => colorNames;
  //-------------------------------------------------------------------------
  public void Setup(int repeatCount, bool isWarp)
  {
    this.isWarp = isWarp;
    colorNames.Clear();
    string[] loaded = isWarp 
      ? ColorSettings.LoadWarpColors(repeatCount)
      : ColorSettings.LoadWeftColors(repeatCount);

    for (int i = 0; i < repeatCount; i++) {
      if (i < loaded.Length)
        colorNames.Add(loaded[i]);
      else
        colorNames.Add("White");
    }

    Refresh();
  }
  //-------------------------------------------------------------------------
  private void Refresh()
  {
    foreach (Transform child in stripContent)
      Destroy(child.gameObject);

    for (int i = 0; i < colorNames.Count; i++)
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
  //-------------------------------------------------------------------------
  private void OnCellClick(int index)
  {
    palattePopup.Show((colorName) =>
    {
      colorNames[index] = colorName;
      Refresh();
      
      if (isWarp)    
        ColorSettings.SaveWarpColors(colorNames.ToArray());
      else
        ColorSettings.SaveWeftColors(colorNames.ToArray());      
    }, Mouse.current.position.ReadValue());
  }

  //-------------------------------------------------------------------------
}