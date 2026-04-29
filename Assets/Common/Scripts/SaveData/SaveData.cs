using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;


public partial class SaveData : MonoBehaviour
{
  private static SaveData s_instance;
  public Info info;
  //
  private string path;
  //----------------------------------------------------------------------------
  public static SaveData instance
  {
    get
    {
      if (s_instance == null)
      {
        GameObject obj = new GameObject("SaveData");
        s_instance = obj.AddComponent<SaveData>();
      }
      return s_instance;
    }
  }
  //----------------------------------------------------------------------------
  void Awake()
  {
    if (s_instance == null)
    {
      s_instance = this;
      DontDestroyOnLoad(gameObject);

#if UNITY_IOS
  System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif

      InitPath();
      Load();
    }
    else if (s_instance != this)
    {
      Debug.LogError("SaveData is singleton");
      Destroy(gameObject);
    }
  }
  //----------------------------------------------------------------------------
  void InitPath()
  {
    path = Application.persistentDataPath + "/bin00.root";
    if (Application.isEditor)
      print(path);
  }
  //----------------------------------------------------------------------------
  public void Save()
  {
#if UNITY_WEBPLAYER && !UNITY_EDITOR
        
#else
    Save(path, info);
    Debug.LogWarning("User Data Saved!");
#endif
  }
  public void ResetAndSave()
  {
    info.Reset();
    Save();
  }
  //----------------------------------------------------------------------------
  public static void Save(string path, Info info)
  {
    try
    {
      using (Stream stream = File.Open(path, FileMode.Create))
      {
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Binder = new VersionBinder();
        using (Stream cryptoStream = new CryptoStream(stream, GetCryptoProvider().CreateEncryptor(), CryptoStreamMode.Write))
        {
          formatter.Serialize(cryptoStream, info);
        }
        stream.Close();
      }
    }
    catch (IOException e)
    {
      print(e.ToString());
    }
  }
  //----------------------------------------------------------------------------
  public void Load()
  {
#if UNITY_WEBPLAYER && !UNITY_EDITOR
        Init(ref info);
#else
    Load(path, ref info);
#endif
  }
  //----------------------------------------------------------------------------
  public static void Load(string path, ref Info info)
  {
    try
    {
      if (File.Exists(path))
      {
        using (Stream stream = File.Open(path, FileMode.Open))
        {
          Init(stream, ref info);
          stream.Close();
        }
      }
      else
      {
        Init(ref info);
      }

    }
    catch (IOException e)
    {
      print(e.ToString());
    }
  }
  //----------------------------------------------------------------------------
  public static void Init(ref Info info)
  {
    info = new Info();
    info.version = 1;
    info.id = "";

    info.name = "";
    info.option = new Defined.LocalGameOption();
    info.lastDay = DateTime.Today.DayOfYear;
    info.option.padType = 1;
    info.option.graphicType = 1;

    switch (Application.systemLanguage)
    {
      case SystemLanguage.Korean:
        info.langType = Defined.LanguageType.KOREAN;
        break;
      default:
        info.langType = Defined.LanguageType.ENGLISH;
        break;
    }
  }
  //----------------------------------------------------------------------------
  public static void Init(Stream stream, ref Info info)
  {
    BinaryFormatter formatter = new BinaryFormatter();
    formatter.Binder = new VersionBinder();
    using (Stream cryptoStream = new CryptoStream(stream, GetCryptoProvider().CreateDecryptor(), CryptoStreamMode.Read))
    {
      info = (Info)formatter.Deserialize(cryptoStream);
    }
  }
  //----------------------------------------------------------------------------

  // Data
  [Serializable]
  public class Info : ISerializable
  {
    public int version;
    public string id;
    public string name;
    public string nickname;

    public Defined.LanguageType langType;
    public Defined.LocalGameOption option = new Defined.LocalGameOption();
    public bool isRegisterNaverAchieve;
    public int lastDay;
    public bool isAgree;
    public bool usePassword;
    public Info()
    {
    }

    public void Reset()
    {
      // 초기화 시점에 따라 version이 달라질 수 있으므로 1로 고정
      id = "";
      name = "";
      nickname = "";
      isAgree = false;
      ///usePassword = false;  usePassword 는 초기화 시 false로 설정하지 않음 (사용자가 명시적으로 설정한 값을 유지하기 위해)

      version = 1;
    }

    public Info(SerializationInfo info, StreamingContext context)
    {
      version = (int)info.GetValue("Version", typeof(int));

      try { id = info.GetValue("ID", typeof(string)) as string; }
      catch (SerializationException)
      {
        id = "";
      }

      try { name = info.GetValue("NAME", typeof(string)) as string; }
      catch (SerializationException)
      {
        name = "";
      }

      try { nickname = info.GetValue("NICK", typeof(string)) as string; }
      catch (SerializationException)
      {
        nickname = "";
      }

      try { isAgree = (bool)info.GetValue("Agreement", typeof(bool)); }
      catch (SerializationException)
      {
        isAgree = false;
      }

      try { usePassword = (bool)info.GetValue("UsePassword", typeof(bool)); }
      catch (SerializationException)
      {
        usePassword = false;
      }

      try
      {
        langType = (Defined.LanguageType)info.GetValue("LangType", typeof(Defined.LanguageType));
      }
      catch (SerializationException)
      {
        langType = Defined.LanguageType.ENGLISH;
      }

      try { isRegisterNaverAchieve = (bool)info.GetValue("isRegisterNaverAchieve", typeof(bool)); }
      catch (SerializationException)
      {
        isRegisterNaverAchieve = false;
      }

      try { lastDay = (int)info.GetValue("lastDay", typeof(int)); }
      catch (SerializationException)
      {
        lastDay = DateTime.Today.DayOfYear;
      }

      try
      {
        option.isPlayMusic = (bool)info.GetValue("musicState", typeof(bool));
      }
      catch (SerializationException)
      {
        option.isPlayMusic = true;
      }

      try
      {
        option.soundVolume = (float)info.GetValue("soundState", typeof(float));
      }
      catch (SerializationException)
      {
        option.soundVolume = 1.0f;
      }

      try
      {
        option.graphicType = (int)info.GetValue("graphicState", typeof(int));
      }
      catch (SerializationException)
      {
        option.graphicType = 1;
      }

      try
      {
        option.isUseAlarm = (bool)info.GetValue("alarmState", typeof(bool));
      }
      catch (SerializationException)
      {
        option.isUseAlarm = true;
      }

      try
      {
        option.isUseVibrate = (bool)info.GetValue("vibrateState", typeof(bool));
      }
      catch (SerializationException)
      {
        option.isUseVibrate = true;
      }

      try
      {
        option.padType = (int)info.GetValue("padTypeState", typeof(int));
      }
      catch (SerializationException)
      {
        option.padType = 1;
      }

      try
      {
        option.isViewNoticeToday = (bool)info.GetValue("viewNoticeToday", typeof(bool));
      }
      catch (SerializationException)
      {
        option.isViewNoticeToday = true;
      }

      try
      {
        option.lastConnectDayOfYear = (int)info.GetValue("lastConnectDayOfYear", typeof(int));
      }
      catch (SerializationException)
      {
        option.lastConnectDayOfYear = DateTime.Today.DayOfYear;
      }
    }
    //----------------------------------------------------------------------------
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Version", version);
      info.AddValue("ID", id);
      info.AddValue("NAME", name);
      info.AddValue("NICK", nickname);
      info.AddValue("musicState", option.isPlayMusic);
      info.AddValue("soundState", option.soundVolume);
      info.AddValue("graphicState", option.graphicType);
      info.AddValue("alarmState", option.isUseAlarm);
      info.AddValue("vibrateState", option.isUseVibrate);
      info.AddValue("padTypeState", option.padType);
      info.AddValue("viewNoticeToday", option.isViewNoticeToday);
      info.AddValue("lastConnectDayOfYear", option.lastConnectDayOfYear);
      info.AddValue("LangType", langType);
      info.AddValue("isRegisterNaverAchieve", isRegisterNaverAchieve);
      info.AddValue("lastDay", lastDay);
      info.AddValue("Agreement", isAgree);
      info.AddValue("UsePassword", usePassword);
    }
  }
  //---------------------------------------------------------------------
  private static string MD5Hash(string str)
  {
    MD5 md5 = new MD5CryptoServiceProvider();
    byte[] hash = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(str));
    System.Text.StringBuilder sb = new System.Text.StringBuilder();
    for (int i = 0; i < hash.Length; i++)
    {
      sb.AppendFormat("{0:x2}", hash[i]);
    }

    return sb.ToString();
  }
  //---------------------------------------------------------------------
  private static byte[] GetbytesFromHexString(string inputString)
  {
    byte[] buffer = new byte[] { };
    // 입력 문자열이 유효한 16진수 문자열인지 확인
    if (!string.IsNullOrEmpty(inputString) && inputString.Length % 2 == 0)
    {
      // 16진수 문자열을 바이트 배열로 변환
      buffer = new byte[inputString.Length / 2];
      for (int i = 0; i < inputString.Length; i += 2)
      {
        //
        buffer[i / 2] = Convert.ToByte(inputString.Substring(i, 2), 16);
      }
    }
    return buffer;
  }
  //----------------------------------------------------------------------------
  private static RijndaelManaged GetCryptoProvider()
  {
    byte[] key = GetbytesFromHexString(MD5Hash("@white#hall$"));
    byte[] iv = GetbytesFromHexString(MD5Hash("deconim"));
    RijndaelManaged aes = new()
    {
      KeySize = 256,
      BlockSize = 128,
      Mode = CipherMode.CBC,
      Padding = PaddingMode.PKCS7,
      Key = key,
      IV = iv
    };
    return aes;
  }
  //----------------------------------------------------------------------------
  public sealed class VersionBinder : SerializationBinder
  {
    public override Type BindToType(string assemblyName, string typeName)
    {
      if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
      {
        Type typeToDeserialize = null;

        assemblyName = Assembly.GetExecutingAssembly().FullName;

        typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
        return typeToDeserialize;
      }
      return null;
    }
  }
  //----------------------------------------------------------------------------
}
