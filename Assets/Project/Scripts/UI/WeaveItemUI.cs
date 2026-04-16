using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaveItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text labelNo;
    [SerializeField] private TMP_Text labelName;
    [SerializeField] private TMP_Text labelCode;
    [SerializeField] private TMP_Text labelDate;
    public Button buttonSelect;
    public void Setup(int no, WeaveData data)
    {
        labelNo.text   = no.ToString();
        labelName.text = data.weaveName;
        labelCode.text = data.weaveCode;
        labelDate.text = data.savedAt;
        //buttonSelect.onClick.AddListener(() => WeaveUI.OnLoad(data));
    }
}