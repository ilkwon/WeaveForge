using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class WeaveItemContextMenu : MonoBehaviour
{
  [SerializeField] private Button buttonDelete;
  private Action _onDelete;

  public void Show(Vector2 screenPos, Action onDelete)
  {
    gameObject.SetActive(true);    
    transform.position = screenPos;
    _onDelete = onDelete;
    buttonDelete.onClick.RemoveAllListeners();  
    buttonDelete.onClick.AddListener(() =>
    {
      _onDelete?.Invoke();
      Hide();
    });    
  }

  //---------------------------------------------------------------------------  
  private void Hide()
  {
    gameObject.SetActive(false);
  }

  //---------------------------------------------------------------------------
  private void Update()
  {
    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
      // 클릭이 메뉴 외부에서 발생했는지 확인
      if (!RectTransformUtility.RectangleContainsScreenPoint(
        GetComponent<RectTransform>(), Mouse.current.position.ReadValue(), null))
      {
        Hide();
      }
    }
  }
}
