using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WeaveItemUI : MonoBehaviour, IPointerClickHandler
{
  
  [SerializeField] private TMP_Text labelNo;
  [SerializeField] private TMP_InputField labelName;
  [SerializeField] private TMP_Text labelCode;
  [SerializeField] private TMP_Text labelDate;
  
  private Action _onDelete;
  public Button buttonSelect;
  private static WeaveItemContextMenu _contextMenu;

  //---------------------------------------------------------------------------
  private void Start()
  {
    if (_contextMenu == null)
      _contextMenu = FindAnyObjectByType<WeaveItemContextMenu>(FindObjectsInactive.Include);    
  }

  //---------------------------------------------------------------------------
  public void Setup(int no, WeaveData data, Action onDelete)
  {
    labelNo.text = no.ToString();
    labelName.text = data.weaveName;
    labelCode.text = data.weaveCode;
    labelDate.text = data.savedAt;
    
    _onDelete = onDelete;
    var code = data.weaveCode; // 캡처
    labelName.onEndEdit.RemoveAllListeners();
    labelName.onEndEdit.AddListener(newName => {
      WeaveDocumentManager.Instance.RenameDocument(code, newName);
    });
  }

  //---------------------------------------------------------------------------
  public void OnPointerClick(PointerEventData eventData)
  {
    Debug.Log("WeaveItemUI OnPointerClick: " + eventData.button);
    if (eventData.button == PointerEventData.InputButton.Right)
    {
      Vector2 currPos = Mouse.current.position.ReadValue();
      Debug.Log("Right click at: " + currPos);
      _contextMenu.Show(currPos, () => _onDelete?.Invoke());
    }
  }

  //---------------------------------------------------------------------------
  private void Update()
  {
    if (Mouse.current.rightButton.wasPressedThisFrame)
    {
      // 클릭이 메뉴 외부에서 발생했는지 확인
      Vector2 currPos = Mouse.current.position.ReadValue();
      if (_contextMenu != null){
        var rt = GetComponent<RectTransform>();
        if (RectTransformUtility.RectangleContainsScreenPoint(rt, currPos, null))
          _contextMenu.Show(currPos, () => _onDelete?.Invoke());
      } 
      else
      {
        Debug.LogWarning("WeaveItemUI Update: ContextMenu reference is null."); 
      }
    }
  }
  //---------------------------------------------------------------------------
}