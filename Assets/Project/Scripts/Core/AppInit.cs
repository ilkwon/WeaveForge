using UnityEngine;
using Deconim.DBConn;
using System.Collections.Generic;
using System;

public class AppInit : MonoBehaviour
{
  void Awake()
  {
    DBConn.Instance.Init(
      mdbPath: Defines.DB_PATH,
      xmlPath: Defines.XML_PATH
    );
    var saveManager = WeaveSaveManager.Instance;
    bool isNew = !DBConn.Instance.ExistsTable("WeavePattern");
    DBConn.Instance.create("create_colors_table");
    if (isNew)
    {
      DBConn.Instance.create("create_weave_pattern");
      Debug.Log("테이블 생성 완료");


    }
    else
    {      
      var list = WeaveSaveManager.Instance.GetList();
      int latest = list.Count - 1;
      if (list.Count > 0)
        WeaveDocumentManager.Instance.OpenDocument(list[latest]["Code"]);
      else
        WeaveDocumentManager.Instance.NewDocument();
    }
  }

}