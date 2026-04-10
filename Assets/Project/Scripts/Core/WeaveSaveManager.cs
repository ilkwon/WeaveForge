using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


public class WeaveSaveManager : MonoBehaviour
{
  private string SaveDir =>
    Path.Combine(Application.persistentDataPath, "WeaveSaves");
  //-------------------------------------------------------------------------
  private void Awake()
  {
    if (!Directory.Exists(SaveDir))
      Directory.CreateDirectory(SaveDir);
  }


  //-------------------------------------------------------------------------
  public void Save(WeaveData data)
  {
    data.savedAt = DateTime.Now.ToString("yy-MM-dd HH:mm");
    string json = JsonUtility.ToJson(data, true);
    string path = Path.Combine(SaveDir, data.weaveName + ".json");
    File.WriteAllText(path, json);
    Debug.Log($"Weave pattern '{data.weaveName}' saved at {path}");
  }
  //-------------------------------------------------------------------------
  public WeaveData Load(string patternName)
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
  public List<string> GetList()
  {
    List<string> list = new List<string>();
    string[] files = Directory.GetFiles(SaveDir, "*.json");
    foreach (string file in files)
    {
      list.Add(Path.GetFileNameWithoutExtension(file));
    }
    return list;
  }
  //-------------------------------------------------------------------------

}