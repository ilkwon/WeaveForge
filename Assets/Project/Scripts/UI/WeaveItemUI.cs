using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaveItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text noText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text dateText;

    public void Setup(int no, WeaveData data)
    {
        noText.text   = no.ToString();
        nameText.text = data.weaveName;
        codeText.text = data.weaveCode;
        dateText.text = data.savedAt;
    }
}