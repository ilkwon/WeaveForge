using System.Collections;
using System.IO;
using System;
using System.Text;

public class FileLogger
{
    public static int LOG_LEVEL_TRACE = 0;

    public static int LOG_LEVEL_DEBUG = 1;

    public static int LOG_LEVEL_INFO = 2;

    public static int LOG_LEVEL_ERROR = 3;

    public static int logLevel = 0;

    public static string logDir;
    /// <summary>
    /// FileLogger사용방법
    /// FileLogger에서 사용하는 섹션은 LOG으로 [LOG]섹션에 로그가 저장될 파일 디렉토리 위치 및 로그 레벨에 대해서 정의
    /// 반드시 Application이 로딩될 초기에 InitLog함수를 호출함으로써 로그 저장위치 및 로그레벨에 대해 지정할 수 있음
    /// 
    /// 
    /// </summary>
    /// <param name="iniPath"></param>
    public static void InitLog()
    {
        logDir = "LOG/";
        string level = "DEBUG";
        
        if(level == null || level == "")
        {
            logLevel = LOG_LEVEL_TRACE;
        }
        else
        {
            if(level.Equals("TRACE"))
            {
                logLevel = LOG_LEVEL_TRACE;
            }
            else if (level.Equals("DEBUG"))
            {
                logLevel = LOG_LEVEL_DEBUG;
            }
            else if (level.Equals("INFO"))
            {
                logLevel = LOG_LEVEL_INFO;
            }
            else if (level.Equals("ERROR"))
            {
                logLevel = LOG_LEVEL_ERROR;
            }
        }
    }

    public static void traceSQL(string message)
    {
        if(logLevel <= LOG_LEVEL_TRACE)
        {
            writeSQLLog("TRACE", message);
        }
    }

    public static void debugSQL(string message)
    {
        if (logLevel <= LOG_LEVEL_DEBUG)
        {
            writeSQLLog("DEBUG", message);
        }
    }


    public static void infoSQL(string message)
    {
        if (logLevel <= LOG_LEVEL_INFO)
        {
            writeSQLLog("INFO", message);
        }
    }
    public static void errorSQL(string message)
    {
        if (logLevel <= LOG_LEVEL_ERROR)
        {
            writeSQLLog("ERROR", message);
        }
    }

    public static void trace(string message)
    {
        if (logLevel <= LOG_LEVEL_TRACE)
        {
            writeLog("TRACE", message);
        }
    }

    public static void debug(string message)
    {
        if (logLevel <= LOG_LEVEL_DEBUG)
        {
            writeLog("DEBUG", message);
        }
    }


    public static void info(string message)
    {
        if (logLevel <= LOG_LEVEL_INFO)
        {
            writeLog("INFO", message);
        }
    }
    //-------------------------------------------------------------------------
    public static void error(string message)
    {
        if (logLevel <= LOG_LEVEL_ERROR)
        {
            writeLog("ERROR", message);
        }
    }
    //-------------------------------------------------------------------------
    private static void writeLog(string tag, string message)
    {
        try
        {
            if(logDir == null || logDir == "")
            {
                logDir = AppDomain.CurrentDomain.BaseDirectory;
            }

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            DateTime today = DateTime.Now;

            string todayStr = string.Format("{0:yyyy-MM-dd}", today);
            string filePath = Path.GetFullPath(logDir + "ApplicationLog-" + todayStr + ".txt");

            StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8);

            string fullMessage = string.Format("{0:yyyy-MM-dd HH:mm:ss} [{1}] : {2}", today, tag, message) + "\n";

            sw.WriteLine( fullMessage );
            sw.Flush();
            sw.Close();
        }
        catch(Exception e)
        {
            Console.WriteLine(e.StackTrace.ToString());
        }
    }
    //-------------------------------------------------------------------------
    private static void writeSQLLog(string tag, string message)
    {
        try
        {
            if (logDir == null || logDir == "")
            {
                logDir = System.IO.Directory.GetCurrentDirectory().Replace("\\", "/");
            }

            if(!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            DateTime today = DateTime.Now;

            string todayStr = string.Format("{0:yyyy-MM-dd}", today);
            string filePath = Path.GetFullPath(logDir + "SQL-" + todayStr + ".txt");

            StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8);

            string fullMessage = string.Format("{0:yyyy-MM-dd HH:mm:ss} [{1}] : {2}", today, tag, message) + "\n";

            sw.WriteLine(fullMessage);
            sw.Flush();
            sw.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace.ToString());
        }
    }
    //-------------------------------------------------------------------------
}