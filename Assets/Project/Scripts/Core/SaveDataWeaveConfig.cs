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
    }

    //--------------------------------------------------------------
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("version", version);
      info.AddValue("lastSelectedCode", lastSelectedCode);
    }
  }

  //--------------------------------------------------------------
  public static Data Load()
  {
    Data data = new();
    SaveData.Load<Data>(_path, ref data);
    return data;
  }

  //--------------------------------------------------------------
  public static void Save(Data data)
  {
    SaveData.Save<Data>(_path, data);
  }
}
