using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Deconim.DBConn;

public class WeaveSaveManager : Singleton<WeaveSaveManager>
{
  private string SaveDir =>
    Path.Combine(Application.persistentDataPath, "WeaveSaves");
  //-------------------------------------------------------------------------
  protected override void Awake()
  {
    base.Awake();

    if (!Directory.Exists(SaveDir))
      Directory.CreateDirectory(SaveDir);
  }
  
  //-------------------------------------------------------------------------
  private string ToNowDateString()
  {
    return DateTime.Now.ToString("yy-MM-dd HH:mm");
  }
  
  //-------------------------------------------------------------------------
  public void SaveJson(WeaveData data)
  {
    data.savedAt = ToNowDateString();
    string json = JsonUtility.ToJson(data, true);
    string path = Path.Combine(SaveDir, data.weaveName + ".json");
    File.WriteAllText(path, json);
    Debug.Log($"Weave pattern '{data.weaveName}' saved at {path}");
  }
  //-------------------------------------------------------------------------
  public void Save(WeaveData data, bool isNew)
  {
    data.savedAt = ToNowDateString();

    // 모드 판단
    WeaveMode mode = (data.colCount > 64 || data.rowCount > 64)
      ? WeaveMode.Jacquard
      : WeaveMode.Dobby;

    string cellsValue;

    
    if (mode == WeaveMode.Dobby)
      cellsValue = string.Join(",", data.cells);
    else
    {
      var bits = new System.Collections.BitArray(System.Array.ConvertAll(data.cells, c => c == 1));
      byte[] blob = new byte[(bits.Length + 7) / 8];
      bits.CopyTo(blob, 0);
      cellsValue = Convert.ToBase64String(blob);
    }

    var param = new Dictionary<string, object>()
    {
      { "@name",       data.weaveName },
      { "@code",       data.weaveCode ?? "" },
      { "@repeatX",    data.colCount },
      { "@repeatY",    data.rowCount },
      { "@cells",      cellsValue },
      { "@warpColors",    data.warpColorNames != null ? string.Join(",", data.warpColorNames) : "" },
      { "@weftColors",    data.weftColorNames != null ? string.Join(",", data.weftColorNames) : "" },
      { "@warpThickness", data.warpThickness  != null ? string.Join(",", data.warpThickness)  : "" },
      { "@weftThickness", data.weftThickness  != null ? string.Join(",", data.weftThickness)  : "" },

      { "@weaveMode",     mode.ToString() },
      { "@savedAt",    data.savedAt }
    };

    if (isNew)
      DBConn.Instance.insert("insert_weave_pattern", param);
    else
      DBConn.Instance.update("update_weave_pattern_by_code", param);

    //Debug.Log($"[DB] Save ({mode}) : {data.weaveName}");
  }
  //-------------------------------------------------------------------------
  public WeaveData LoadJson(string patternName)
  {
    string path = Path.Combine(SaveDir, patternName + ".json");
    if (!File.Exists(path))
    {
      Debug.LogError($"Weave pattern '{patternName}' not found at {path}");
      return null;
    }
    string json = File.ReadAllText(path);
    return JsonUtility.FromJson<WeaveData>(json);
  }
  //-------------------------------------------------------------------------
  public WeaveData Load(string patternCode)
  {
    var param = new Dictionary<string, object>()
    {
      { "@code", patternCode }
    };
    DataResult result = DBConn.Instance.select("select_weave_pattern_by_code", param);

    if (result == null || result.Count == 0)
    {
      //Debug.LogError($"[DB] Load 실패 : {patternCode}");
      return null;
    }

    var row = result.Data[0];
    WeaveData data = new();
    data.weaveName = row["Name"].ToString();
    data.weaveCode = row["Code"].ToString();
    data.colCount = Convert.ToInt32(row["RepeatX"]);
    data.rowCount = Convert.ToInt32(row["RepeatY"]);
    data.savedAt = row["SavedAt"].ToString();

    WeaveMode mode = (data.colCount > 64 || data.rowCount > 64)
      ? WeaveMode.Jacquard
      : WeaveMode.Dobby;

    string cellsStr = row["Cells"].ToString();
    if (mode == WeaveMode.Dobby)
    {
      data.cells = Array.ConvertAll(cellsStr.Split(','), int.Parse);
    }
    else
    {
      byte[] blob = Convert.FromBase64String(cellsStr);
      var bits = new System.Collections.BitArray(blob);
      data.cells = new int[data.colCount * data.rowCount];
      for (int i = 0; i < data.cells.Length; i++)
        data.cells[i] = bits[i] ? 1 : 0;
    }

    data.warpColorNames = row["WarpColors"].ToString().Split(',');
    data.weftColorNames = row["WeftColors"].ToString().Split(',');
    string warpThicknessText = row["WarpThickness"].ToString();
    string weftThicknessText = row["WeftThickness"].ToString();
    data.warpThickness = string.IsNullOrEmpty(warpThicknessText)
      ? new float[0]
      : Array.ConvertAll(warpThicknessText.Split(','), float.Parse);

    data.weftThickness = string.IsNullOrEmpty(weftThicknessText)
      ? new float[0]
      : Array.ConvertAll(weftThicknessText.Split(','), float.Parse);
    Debug.Log($"[DB] Load ({mode}) : {data.weaveName}");

    return data;
  }
  //---------------------------
  public List<Dictionary<string, string>> GetList()
  {
    DataResult result = DBConn.Instance.select("select_weave_pattern_list", null);
    List<Dictionary<string, string>> list = new();

    if (result == null || result.Count == 0)
      return list;

    foreach (var row in result.Data)
    {
      Dictionary<string, string> dataSet = new()
      {
        { "Code", row["Code"].ToString() },
        { "Name", row["Name"].ToString() },
        { "SavedAt", row["SavedAt"].ToString() }
      };

      list.Add(dataSet);
    }

    return list;
  }

  //-------------------------------------------------------------------------
  public List<string> GetListJson()
  {
    List<string> list = new();
    string[] files = Directory.GetFiles(SaveDir, "*.json");
    foreach (string file in files)
    {
      list.Add(Path.GetFileNameWithoutExtension(file));
    }
    return list;
  }

  //-------------------------------------------------------------------------
  public void Delete(string patternCode)
  {
    var param = new Dictionary<string, object>()
    {
      { "@code", patternCode }
    };
    DBConn.Instance.delete("delete_weave_pattern_by_code", param);
    Debug.Log($"[DB] Delete : {patternCode}");
  }

  public void Rename(string code, string newName)
  {
    var param = new Dictionary<string, object>()
    {
      { "@code", code },
      { "@name", newName }
    };
    DBConn.Instance.update("update_weave_pattern_name", param);
    Debug.Log($"[DB] Rename : {code} -> {newName}");
  }
  //-------------------------------------------------------------------------
}