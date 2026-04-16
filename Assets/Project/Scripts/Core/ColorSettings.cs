using System.Collections.Generic;
using Deconim.DBConn;
using UnityEngine;

public class ColorSettings
{
  private static readonly string WARP_KEY = "WarpColors";
  private static readonly string WEFT_KEY = "WeftColors";

  //-------------------------------------------------------------------------
  public static void SaveWarpColors(string[] colors)
  {
    Save(WARP_KEY, colors);
  }

  //-------------------------------------------------------------------------
  public static void SaveWeftColors(string[] colors)
  {
    Save(WEFT_KEY, colors);
  }

  //-------------------------------------------------------------------------
  public static string[] LoadWarpColors(int count)
  {
    return Load(WARP_KEY, count);
  }

  //-------------------------------------------------------------------------
  public static string[] LoadWeftColors(int count)
  {
    return Load(WEFT_KEY, count);
  }

  //---------------------------------------------------------------------------
  private static void Save(string key, string[] colors)
  {
    var param = new Dictionary<string, object>()
    {
      { "@key", key },
      { "@value", string.Join(",", colors) }
    };
    DBConn.Instance.insert("upsert_colors", param);
  }

  //---------------------------------------------------------------------------
  private static string[] Load(string key, int count)
  {
    var param = new Dictionary<string, object>()
    {
      {"@key", key}
    };

    DataResult result = DBConn.Instance.select("select_colors_by_key", param);

    if (result == null || result.Count == 0)
    {
      string[] defaults = new string[count];
      for (int i = 0; i < count; i++)
        defaults[i] = "White";
      return defaults;

    }
    return result.Data[0]["Value"].ToString().Split(',');
  }
}