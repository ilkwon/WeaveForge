using System;
using System.Xml;
using System.Data;
using SQLite4Unity3d;
using System.Collections.Generic;
using UnityEngine;

namespace Deconim.DBConn
{
  public class DBConn
  {
    private XmlDocument sqlDocument;
    private static DBConn s_instance;
    private SQLiteConnection localDBConn;

    public bool localDBState = false;
    public int dbConnType = 0;

    public delegate void DBConnectDelegate(int dbConnType);
    public event DBConnectDelegate dbConnectEvent;

    //-------------------------------------------------------------------------
    public static DBConn Instance
    {
      get
      {
        if (s_instance == null)
          s_instance = new DBConn();
        return s_instance;
      }
    }

    //-------------------------------------------------------------------------
    private DBConn()
    {
      FileLogger.InitLog();
    }

    //-------------------------------------------------------------------------
    public void Init(string mdbPath, string xmlPath)
    {
      initXMLFile(xmlPath);
      initMDBConnect(mdbPath);
      FileLogger.info("dbConnType : [" + dbConnType + "]");
    }

    //-------------------------------------------------------------------------
    public void initMDBConnect(string localDB_path)
    {
      try
      {
        FileLogger.debug("localDB_path : [" + localDB_path + "]");
        localDBConn = new SQLiteConnection(localDB_path);
        localDBState = true;
        dbConnType = 1;
        FileLogger.info("LocalDB open 성공");
      }
      catch (Exception ex)
      {
        localDBState = false;
        dbConnType = 0;
        FileLogger.info(ex.StackTrace);
        Debug.LogError(ex.Message);
      }

      if (dbConnectEvent != null)
        dbConnectEvent(dbConnType);
    }

    //-------------------------------------------------------------------------
    public void closeMDB()
    {
      localDBConn?.Close();
      localDBState = false;
      dbConnType = 0;
    }

    //-------------------------------------------------------------------------
    private void initXMLFile(string xmlPath)
    {
      FileLogger.info("xmlPath : [" + xmlPath + "]");
      try
      {
        sqlDocument = new XmlDocument();
        sqlDocument.Load(xmlPath);
      }
      catch (Exception e)
      {
        FileLogger.info(e.StackTrace.ToString());
        Debug.LogError("XML 파일을 읽을 수 없습니다. [" + xmlPath + "]");
      }
    }

    //-------------------------------------------------------------------------
    private int ExcuteLocalDB(string sql)
    {
      try
      {
        return localDBConn.Execute(sql);
      }
      catch (InvalidOperationException e)
      {
        string msg = "SQLite InvalidOperationException : [" + e.Message + "]";
        Debug.LogError(msg);
        FileLogger.info(msg);
      }
      catch (SQLiteException e)
      {
        string msg = "SQLite Exception : [" + e.Message + "]";
        Debug.LogError(msg);
        FileLogger.info(msg);
      }
      catch (Exception e)
      {
        string msg = "General Exception : [" + e.Message + "]";
        Debug.LogError(msg);
        FileLogger.info(msg);
      }
      return 0;
    }

    //-------------------------------------------------------------------------
    private string GetQueryNode(string id, Dictionary<string, object> param, XmlNode queryNode)
    {
      string query = queryNode.InnerText;

      if (param != null)
      {
        foreach (string key in param.Keys)
        {
          if (param[key].GetType() == typeof(int))
          {
            string value = ((int)param[key]).ToString();
            query = query.Replace(key, value);
          }
          else if (param[key].GetType() == typeof(string))
          {
            string value = (string)param[key];
            query = query.Replace(key, "'" + value + "'");
          }
        }
      }
      return query;
    }

    //-------------------------------------------------------------------------
    private XmlNode FindQueryNode(string tag, string id)
    {
      XmlNodeList nodes = sqlDocument.SelectNodes("sqlMap/" + tag);
      if (nodes == null || nodes.Count == 0)
        throw new Exception(tag + " 로 선언된 쿼리가 없습니다.");

      foreach (XmlNode item in nodes)
      {
        if (item.Attributes["id"].Value.Equals(id))
          return item;
      }

      throw new Exception(id + " 이름으로 선언된 " + tag + " 쿼리가 없습니다.");
    }

    //-------------------------------------------------------------------------
    private bool CheckDB()
    {
      if (dbConnType == 0)
      {
        Debug.LogWarning("현재 연결된 DB가 없습니다.");
        return false;
      }
      return true;
    }

    //-------------------------------------------------------------------------
    public int create(string id)
    {
      if (!CheckDB()) return 0;
      XmlNode queryNode = FindQueryNode("create", id);
      string query = queryNode.InnerText.Trim();
      FileLogger.infoSQL("SQL : [" + query + "]");
      return ExcuteLocalDB(query);
    }

    //-------------------------------------------------------------------------
    public int insert(string id, Dictionary<string, object> param)
    {
      if (!CheckDB()) return 0;
      XmlNode queryNode = FindQueryNode("insert", id);
      string query = GetQueryNode(id, param, queryNode);
      FileLogger.infoSQL("SQL : [" + query + "]");
      return ExcuteLocalDB(query);
    }

    //-------------------------------------------------------------------------
    public int update(string id, Dictionary<string, object> param)
    {
      if (!CheckDB()) return 0;
      XmlNode queryNode = FindQueryNode("update", id);
      string query = GetQueryNode(id, param, queryNode);
      FileLogger.infoSQL("SQL : [" + query + "]");
      return ExcuteLocalDB(query);
    }

    //-------------------------------------------------------------------------
    public int delete(string id, Dictionary<string, object> param)
    {
      if (!CheckDB()) return 0;
      XmlNode queryNode = FindQueryNode("delete", id);
      string query = GetQueryNode(id, param, queryNode);
      FileLogger.infoSQL("SQL : [" + query + "]");
      return ExcuteLocalDB(query);
    }

    //-------------------------------------------------------------------------
    public DataResult select(string id, Dictionary<string, object> param)
    {
      if (!CheckDB()) return null;

      XmlNode queryNode = FindQueryNode("select", id);
      string query = GetQueryNode(id, param, queryNode);
      FileLogger.infoSQL("SQL : [" + query + "]");

      DataResult result = new DataResult();
      List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();

      try
      {
        var stmt = SQLite3.Prepare2(localDBConn.Handle, query);
        try
        {
          int colCount = SQLite3.ColumnCount(stmt);

          while (SQLite3.Step(stmt) == SQLite3.Result.Row)
          {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < colCount; i++)
            {
              string colName = SQLite3.ColumnName16(stmt, i);
              SQLite3.ColType colType = SQLite3.ColumnType(stmt, i);

              object val = null;
              switch (colType)
              {
                case SQLite3.ColType.Integer:
                  val = SQLite3.ColumnInt(stmt, i);
                  break;
                case SQLite3.ColType.Float:
                  val = SQLite3.ColumnDouble(stmt, i);
                  break;
                case SQLite3.ColType.Text:
                  val = SQLite3.ColumnString(stmt, i);
                  break;
                case SQLite3.ColType.Blob:
                  val = SQLite3.ColumnByteArray(stmt, i);
                  break;
                case SQLite3.ColType.Null:
                  val = null;
                  break;
              }
              row[colName] = val;
            }
            resultList.Add(row);
          }
        }
        finally
        {
          SQLite3.Finalize(stmt);
        }

        result.Count = resultList.Count;
        result.Data = resultList;
        return result;
      }
      catch (Exception ex)
      {
        FileLogger.error(ex.Message);
        Debug.LogError(ex.Message);
        return result;
      }
    }
  }
}