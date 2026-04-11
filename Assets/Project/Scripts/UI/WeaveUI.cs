using UnityEngine;

using TMPro;
using System.CodeDom.Compiler;
using System;

public class WeaveUI : MonoBehaviour
{
  [SerializeField] private WeaveGrid weaveGrid;

  [SerializeField] private TMP_InputField nameInputField;
  [SerializeField] private Transform listContent;
  [SerializeField] private GameObject listItemPrefab;
  [SerializeField] private ColorStripUI colorStripWarp;
  [SerializeField] private ColorStripUI colorStripWeft;
  private string currentCode = "";

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
    bool isNew = string.IsNullOrEmpty(currentCode);
    if (isNew)
      currentCode = GenerateCode(data);
    
    Debug.Log($"currentCode : {currentCode}");
    Debug.Log($"data.weaveCode : {data.weaveCode}");

    data.weaveCode = currentCode;
    WeaveSaveManager.Instance.Save(data, isNew);
    RefreshList();
  }
  //-------------------------------------------------------------------------
  private string GenerateCode(WeaveData data)
  {
    string type = (data.repeatX > 64 || data.repeatY > 64) ? "JQ" : "DB";
    string date = System.DateTime.Now.ToString("yyMMdd");

    var list = WeaveSaveManager.Instance.GetList();
    int count = 0;
    foreach (var item in list)
    {
      if (item["Code"].StartsWith(type + "-" + date))
        count++;
    }
    string code = $"{type}-{date}-{(count + 1):D3}";
    Debug.Log($"GenerateCode : {code}");
    return code;
  }

  //-------------------------------------------------------------------------
  public void OnNewButton()
  {
    weaveGrid.Clear();
    nameInputField.text = "";
    currentCode = "";
  }

  //-------------------------------------------------------------------------
  private void OnLoadButton(string weaveName)
  {
    WeaveData data = WeaveSaveManager.Instance.Load(weaveName);
    if (data == null) return;
    weaveGrid.LoadPattern(data);
    nameInputField.text = data.weaveName;

    currentCode = data.weaveCode;
  }

  //-------------------------------------------------------------------------
  public void RefreshList()
  {
    foreach (Transform child in listContent)
      Destroy(child.gameObject);

    var list = WeaveSaveManager.Instance.GetList();
    for (int i = 0; i < list.Count; i++)
    {
      string code = list[i]["Code"];
      string name = list[i]["Name"];
      string saveAt = list[i]["SavedAt"];
      WeaveData data = new WeaveData();
      data.weaveName = name;
      data.weaveCode = code;
      data.savedAt = saveAt;
      GameObject item = Instantiate(listItemPrefab, listContent);
      item.GetComponent<WeaveItemUI>().Setup(i + 1, data);
      string captured = code;
      item.GetComponent<WeaveItemUI>().buttonSelect.onClick.AddListener(() =>
      {
        OnLoadButton(captured);
      });
    }
  }
  //-------------------------------------------------------------------------
}