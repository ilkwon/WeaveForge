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
  public void RefreshList()
  {
    return;
    // 기존 리스트 아이템 제거
    foreach (Transform child in listContent)
      Destroy(child.gameObject);

    // 저장된 패턴 리스트 불러오기
    List<string> patternNames = saveManager.GetList();
    foreach (string name in patternNames)
    {
      GameObject item = Instantiate(listItemPrefab, listContent);
      item.GetComponentInChildren<TMP_Text>().text = name;
      Button button = item.GetComponent<Button>();
      button.onClick.AddListener(() => OnLoadButton(name));
    }
  }

  //-------------------------------------------------------------------------
  private void OnLoadButton(string patternName)
  {

  }
}