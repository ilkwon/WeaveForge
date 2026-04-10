using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WeaveUI : MonoBehaviour
{
  [SerializeField] private WeaveGrid weaveGrid;
  [SerializeField] private WeaveSaveManager saveManager;
  [SerializeField] private TMP_InputField nameInputField;
  [SerializeField] private Transform listContent;
  [SerializeField] private GameObject listItemPrefab;
  [SerializeField] private ColorStripUI colorStripWarp;
  [SerializeField] private ColorStripUI colorStripWeft;
  //-------------------------------------------------------------------------
  private void Start()
  {
    colorStripWarp.Setup(8);
    colorStripWeft.Setup(8);
    
    RefreshList();
  }

  //-------------------------------------------------------------------------
  public void OnSaveButton()
  {
    string patternName = nameInputField.text.Trim();
    if (string.IsNullOrEmpty(patternName)) return;

    WeaveData data = new WeaveData();
    weaveGrid.GetData(data);
    data.weaveName = patternName;
    saveManager.Save(data);
    RefreshList();
  }

  //-------------------------------------------------------------------------
  public void OnNewButton()
  {
    weaveGrid.Clear();
    nameInputField.text = "";
  }

  private void OnLoadButton(string weaveName)
  {
    WeaveData data = saveManager.Load(weaveName);
    if (data == null) return;
    weaveGrid.LoadPattern(data);
    nameInputField.text = data.weaveName;
  }

  //-------------------------------------------------------------------------
  public void RefreshList()
  {
    foreach (Transform child in listContent)
      Destroy(child.gameObject);

    List<string> list = saveManager.GetList();
    for (int i = 0; i < list.Count; i++)
    {
      WeaveData data = saveManager.Load(list[i]);
      GameObject item = Instantiate(listItemPrefab, listContent);
      item.GetComponent<WeaveItemUI>().Setup(i + 1, data);
      string captured = list[i];
      item.GetComponent<WeaveItemUI>().buttonSelect.onClick.AddListener(() =>
      {
        OnLoadButton(captured);
      });
    }
  }
  //-------------------------------------------------------------------------
}