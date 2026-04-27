using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PatternListUI : MonoBehaviour
{
  //[SerializeField] private TieupView tieupView;
  [SerializeField] private Transform listContent;
  [SerializeField] private GameObject listItemPrefab;
  [SerializeField] private GameObject scrollView;
  [SerializeField] private TMP_Text toggleButtonLabel;
  // 필드 추가
  [SerializeField] private RectTransform panelRT;
  private float _expandedWidth = 600f;
  private float _collapsedWidth = 30f;
  private bool _isExpanded = true;

  //-------------------------------------------------------------------------
  void Start()
  {
    RefreshList();
  }

  //-------------------------------------------------------------------------
  private void RefreshList()
  {
    ClearList();
    //scrollView.GetComponent<UnityEngine.UI.ScrollRect>().movementType
    //  = ScrollRect.MovementType.Clamped;
    var list = WeaveSaveManager.Instance.GetList();
    for (int i = 0; i < list.Count; i++)
    {
      string code = list[i]["Code"];
      string name = list[i]["Name"];
      string savedAt = list[i]["SavedAt"];

      SpawnItem(i + 1, code, name, savedAt);
    }
  }

  //-------------------------------------------------------------------------
  private void SpawnItem(int no, string code, string name, string savedAt)
  {
    WeaveData data = new WeaveData();
    data.weaveCode = code;
    data.weaveName = name;
    data.savedAt = savedAt;

    GameObject item = Instantiate(listItemPrefab, listContent);
    item.GetComponent<WeaveItemUI>().Setup(no, data, () => {
          
      WeaveDocumentManager.Instance.DeleteDocument(code);
      RefreshList(); 
    });

    string captured = code;
    //Debug.Log($"[PatternListUI] OnSelect called: {code}");
    item.GetComponent<WeaveItemUI>().buttonSelect.onClick.AddListener(() =>
    {
      OnSelect(captured);
    });
  }

  //-------------------------------------------------------------------------
  private void OnSelect(string code)
  {
    WeaveDocumentManager.Instance.OpenDocument(code);    
  }

  //-------------------------------------------------------------------------
  private void ClearList()
  {
    foreach (Transform child in listContent)
      Destroy(child.gameObject);
  }

  //-------------------------------------------------------------------------
  public void TogglePanel()
  {
    _isExpanded = !_isExpanded;
    scrollView.SetActive(_isExpanded);
    panelRT.sizeDelta = new Vector2(
      _isExpanded ? _expandedWidth : _collapsedWidth,
      panelRT.sizeDelta.y);

    toggleButtonLabel.text = _isExpanded ? "◀" : "▶";
  }
  //-------------------------------------------------------------------------
  public void NewDocument()
  {
    WeaveDocumentManager.Instance.NewDocument();
    
    RefreshList();
  }
  //-------------------------------------------------------------------------
}
