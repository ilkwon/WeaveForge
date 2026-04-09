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
  private void OnLoadButton(string weaveName)
  {
    WeaveData data = saveManager.Load(weaveName);
    if (data == null) return;
    weaveGrid.LoadPattern(data);
  }

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
//      item.GetComponent<Button>().onClick.AddListener(() =>
//      {
//        OnLoadButton(captured);
//      });
    }
  }
  //-------------------------------------------------------------------------
}