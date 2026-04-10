using UnityEngine;
using Deconim.DBConn;
using System.Collections.Generic;

public class AppInit : MonoBehaviour
{
  void Start()
  {
    DBConn.Instance.Init(
        mdbPath: Defines.DB_PATH,
        xmlPath: Defines.XML_PATH
    );

    DBConn.Instance.create("create_weave_pattern");
    Debug.Log("테이블 생성 완료");

    // insert 테스트
    var param = new Dictionary<string, object>()
    {
        { "@name",       "test_pattern" },
        { "@code",       "TP-001" },
        { "@repeatX",    8 },
        { "@repeatY",    8 },
        { "@cells",      "1,0,1,0,1,0,1,0" },
        { "@warpColors", "White,Black" },
        { "@weftColors", "White,Black" },
        { "@savedAt",    "26-04-11 10:00" }
    };

    int ret = DBConn.Instance.insert("insert_weave_pattern", param);
    Debug.Log("insert result : " + ret);

    // select 테스트
    DataResult result = DBConn.Instance.select("select_weave_pattern_list", null);
    if (result != null && result.Count > 0)
    {
      Debug.Log("select count : " + result.Count);
      foreach (var row in result.Data)
        Debug.Log("row : " + string.Join(" | ", row));
    }
    else
    {
      Debug.Log("select 결과 없음");
    }
  }
}