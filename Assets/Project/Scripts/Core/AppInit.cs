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

      //MakeTestData();
      
    }
    // GetList 테스트
    var list = saveManager.GetList();
    foreach (var item in list)
      Debug.Log($"Code : {item["Code"]} | Name : {item["Name"]}");

    // Load 테스트
    WeaveData loaded = saveManager.Load("DB-260412-001");
    if (loaded != null)
      Debug.Log($"Load 완료 : {loaded.weaveName}");
  }
  //---------------------------------------------------------------
  private void MakeTestData()
  {
    // Save 테스트
      WeaveData data = new WeaveData();
      data.weaveName = "test_pattern";
      data.weaveCode = "DB-260412-001";
      data.colCount = 8;
      data.rowCount = 8;
      data.cells = new int[8 * 8];
      for (int i = 0; i < data.cells.Length; i++)
        data.cells[i] = i % 2;

      data.warpColorNames = new string[] { "White", "Black" };
      data.weftColorNames = new string[] { "White", "Black" };
      data.warpThickness = new float[] { 0.5f, 0.5f };
      data.weftThickness = new float[] { 0.5f, 0.5f };

      // WeaveSaveManager 가져오기
      var saveManager = WeaveSaveManager.Instance;
      saveManager.Save(data, true);

      Debug.Log("Test Data 완료");
  }
}