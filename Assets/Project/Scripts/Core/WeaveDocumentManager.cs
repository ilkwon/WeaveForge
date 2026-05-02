using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class WeaveDocumentManager : Singleton<WeaveDocumentManager>
{
  [SerializeField] private TieupView tieupView;

  public WeaveData CurrentWeaveData { get; private set; }
  
  [ShowInInspector, ReadOnly]
  public WeaveSettings CurrentWeaveSettings { get; private set;  }
  public Action<WeaveData> OnDocumentChanged; // 문서가 변경될 때마다 호출되는 이벤트
  public Action<WeaveSettings> OnSettingsChanged; // 설정이 변경될 때마다 호출되는 이벤트

  protected override void Awake()
  {
    base.Awake();
    // 초기화 작업이 필요한 경우 여기에 추가

    CurrentWeaveSettings = SaveDataWeaveConfig.Load().weaveSettings ?? new WeaveSettings();
  }
  //-------------------------------------------------------------------------
  private void Start()
  {
    tieupView.OnTieupChanged += SaveDocument;
    
  }
  //-------------------------------------------------------------------------
  private void OnDestroy()
  {   
    tieupView.OnTieupChanged -= SaveDocument;
  }
  //-------------------------------------------------------------------------
  public void OpenDocument(string code)
  {
    var data = WeaveSaveManager.Instance.Load(code);
    if (data == null) return;
    CurrentWeaveData = data;
    OnDocumentChanged?.Invoke(CurrentWeaveData);
  }

  //-------------------------------------------------------------------------
  public void NewDocument()
  {    
    var settings = CurrentWeaveSettings ?? new WeaveSettings();
    var data = new WeaveData()
    {
      weaveName = "새 패턴",
      weaveCode = GenerateCode(new WeaveData() { colCount = settings.colCount, rowCount = settings.rowCount }),
      colCount = settings.colCount,
      rowCount = settings.rowCount,
      cells = new int[settings.colCount * settings.rowCount],
      warpColorNames = new string[settings.colCount * settings.warpRepeat],
      weftColorNames = new string[settings.colCount * settings.weftRepeat],
      warpThickness = new float[settings.colCount],
      weftThickness = new float[settings.rowCount],
      savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
    };
    WeaveSaveManager.Instance.Save(data, isNew: true);
    CurrentWeaveData = data;
    
    OnDocumentChanged?.Invoke(CurrentWeaveData);
  }
  //-------------------------------------------------------------------------
  public void SaveDocument()
  {
    if (CurrentWeaveData == null) return;
    if (string.IsNullOrEmpty(CurrentWeaveData.weaveCode))
      CurrentWeaveData.weaveCode = GenerateCode(CurrentWeaveData);
    WeaveSaveManager.Instance.Save(CurrentWeaveData, isNew: false);
  }

  //-------------------------------------------------------------------------
  public void DeleteDocument(string code)
  {
    // 현재 열려있는 문서가 삭제되는 경우를 체크
    bool isDeletingCurrent = CurrentWeaveData != null && CurrentWeaveData.weaveCode == code;
    WeaveSaveManager.Instance.Delete(code);

    if (!isDeletingCurrent) return;

    var list = WeaveSaveManager.Instance.GetList(); // 리스트 새로고침
    if (list.Count > 0)
    {
      int last = list.Count - 1; // 가장 최근에 저장된 문서 선택
      var newCode = list[last]["Code"].ToString();
      OpenDocument(newCode); // 최근문서 열기
    }
    else
    {
      NewDocument(); // 새 문서 생성
    }
  }

  //-------------------------------------------------------------------------  
  private string GenerateCode(WeaveData data)
  {
    string type = (data.colCount > 64 || data.rowCount > 64) ? "JQ" : "DB";
    string date = System.DateTime.Now.ToString("yyMMdd");

    var list = WeaveSaveManager.Instance.GetList();
    
    int maxSeq = 0;
    foreach (var item in list)
    {
      if (!item["Code"].StartsWith(type + "-" + date)) continue;  // 오늘 날짜의 패턴이 아니면 무시
      var prefix = item["Code"].ToString();
      int lastDash = prefix.LastIndexOf('-');
      string numberPart = lastDash >= 0 ? prefix.Substring(lastDash + 1) : "000";
      int num = int.TryParse(numberPart, out int n) ? n : 0;
      if (num > maxSeq) // 가장 큰 번호 찾기
        maxSeq = num;        
    }

    string code = $"{type}-{date}-{(maxSeq + 1):D3}";
    Debug.Log($"GenerateCode : {code}");
    return code;
  }
  //-------------------------------------------------------------------------
  public void RenameDocument(string code, string newName)
  {
    if (CurrentWeaveData != null && CurrentWeaveData.weaveCode == code)
    {
      CurrentWeaveData.weaveName = newName;
      WeaveSaveManager.Instance.Rename(code, newName);      
    }
  }
  //-------------------------------------------------------------------------
  public void ApplySettings(WeaveSettings settings)
  {
    CurrentWeaveSettings = settings ?? new WeaveSettings();
    OnSettingsChanged?.Invoke(CurrentWeaveSettings);
  }
}
