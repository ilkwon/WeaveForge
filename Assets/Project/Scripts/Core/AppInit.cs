using UnityEngine;
using Deconim.DBConn;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class AppInit : MonoBehaviour
{
  void Awake()
  {
    DBConn.Instance.Init(
      mdbPath: Defines.DB_PATH,
      xmlPath: Defines.XML_PATH
    );
    
    // 로컮 세팅 로드.
    var weaveConfig = SaveDataWeaveConfig.Load();
    WeaveDocumentManager.Instance.ApplySettings(weaveConfig.weaveSettings);

    bool isNew = !DBConn.Instance.ExistsTable("WeavePattern");
    DBConn.Instance.create("create_colors_table");
    if (isNew)
    {
      DBConn.Instance.create("create_weave_pattern");
      WeaveDocumentManager.Instance.NewDocument();
    }
    else
    {
      // 최근 열었던 문서 열기     
      var list = WeaveSaveManager.Instance.GetList();

      if (list.Count > 0)
      {
       
        string lastSelectedCode = weaveConfig.lastSelectedCode;
        bool exists = list.Exists(item => item["Code"] == lastSelectedCode);

        if (!string.IsNullOrEmpty(lastSelectedCode) && exists)
          WeaveDocumentManager.Instance.OpenDocument(lastSelectedCode);
        else
          WeaveDocumentManager.Instance.OpenDocument(list[list.Count - 1]["Code"]);
      }
      else
      {
        WeaveDocumentManager.Instance.NewDocument();
      }
    }
  }

}