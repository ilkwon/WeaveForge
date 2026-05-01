using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;
using static SaveData;

public class SaveDataWeaveConfig
{
  private static readonly string _path =
    Application.persistentDataPath + "/weave_config.bin";

  [Serializable]
  public class Data : ISerializable
  {
    public int version;
    public string lastSelectedCode;
    public WeaveSettings weaveSettings = new WeaveSettings();
    public Data()
    {
      version = 1;
      lastSelectedCode = "";
    }
    //--------------------------------------------------------------
    public Data(SerializationInfo info, StreamingContext context)
    {
      version = info.GetInt32("version");
      lastSelectedCode = info.GetString("lastSelectedCode");
      
      try { weaveSettings = (WeaveSettings)info.GetValue("weaveSettings", typeof(WeaveSettings)); }
      catch (SerializationException) { weaveSettings = new WeaveSettings(); }
    }

    //--------------------------------------------------------------
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("version", version);
      info.AddValue("lastSelectedCode", lastSelectedCode);
      info.AddValue("weaveSettings", weaveSettings);
    }
  }

  //--------------------------------------------------------------
  public static Data Load()
  {
    Data data = new();
    if (!System.IO.File.Exists(_path))
    {
      //Debug.Log("[SaveDataWeaveConfig] 파일 없음 → 기본값 사용");
      return data;
    }
    SaveData.Load<Data>(_path, ref data);
    if (data.weaveSettings == null)
      data.weaveSettings = new WeaveSettings();
    
    return data;
  }

  //--------------------------------------------------------------
  public static void Save(Data data)
  {
    SaveData.Save<Data>(_path, data);
  }
}
