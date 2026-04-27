using UnityEngine;

public static class Defines
{
#if UNITY_EDITOR
  public static string DB_PATH  => Application.streamingAssetsPath + "/weave.db";
  public static string XML_PATH => Application.streamingAssetsPath + "/WeaveSQL.xml";
#else
  public static string DB_PATH => Application.persistentDataPath + "/weave.db";
  public static string XML_PATH => Application.streamingAssetsPath + "/WeaveSQL.xml";
#endif
}