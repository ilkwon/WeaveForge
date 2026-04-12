using UnityEngine;

using TMPro;
using System.CodeDom.Compiler;
using System;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class WeaveUI : MonoBehaviour
{
  [SerializeField] private WeaveGrid weaveGrid;

  [SerializeField] private TMP_InputField nameInputField;
  [SerializeField] private Transform listContent;
  [SerializeField] private GameObject listItemPrefab;
  [SerializeField] private ColorStripUI colorStripWarp;
  [SerializeField] private ColorStripUI colorStripWeft;
  [SerializeField] private TMP_InputField texboxUnitWidth;
  [SerializeField] private TMP_InputField textboxUnitHeight;
  private string currentCode = "";

  //-------------------------------------------------------------------------

  private void Start()
  {
    colorStripWarp.Setup(8, true);
    colorStripWeft.Setup(8, false);
    
    texboxUnitWidth.text = "8";
    textboxUnitHeight.text = "8";
    
    // 탭 연결.
    texboxUnitWidth.onSubmit.AddListener(_ => textboxUnitHeight.Select());

    RefreshList();
  }

  //-------------------------------------------------------------------------
  private void TabKeyNextCursorUnitSize()
  {
      if (Keyboard.current.tabKey.wasPressedThisFrame)
      {
        if (texboxUnitWidth.isFocused)
          textboxUnitHeight.Select();
        else if (textboxUnitHeight.isFocused)
          texboxUnitWidth.Select();
      }
  }

  //-------------------------------------------------------------------------
  public void OnSaveButton()
  {
    string patternName = nameInputField.text.Trim();
    if (string.IsNullOrEmpty(patternName)) return;

    WeaveData data = new WeaveData();
    weaveGrid.GetData(data);

    int x = int.Parse(texboxUnitWidth.text);
    int y = int.Parse(textboxUnitHeight.text);
    

    // 크기가 바뀐 경우만 Resize
    if (x != data.repeatX || y != data.repeatY)
      weaveGrid.Resize(x, y);

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
    int w = int.Parse(texboxUnitWidth.text);
    int h = int.Parse(textboxUnitHeight.text);
    
    weaveGrid.Resize(w, h);

    colorStripWarp.Setup(w, true);  // 경사 세로줄 w 방향
    colorStripWeft.Setup(h, false);  // 위사 가로줄 h 방향

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

    texboxUnitWidth.text = data.repeatX.ToString();
    textboxUnitHeight.text = data.repeatY.ToString();

    colorStripWarp.Setup(data.repeatX, true);
    colorStripWeft.Setup(data.repeatY, false);
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
  private void Update()
  {
    TabKeyNextCursorUnitSize();
  }
}