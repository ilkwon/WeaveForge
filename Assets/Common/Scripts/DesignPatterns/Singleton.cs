using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
  private static T s_instance;
  private static readonly object s_lock = new();
  private static bool _applicationIsQuitting = false;
  //-------------------------------------------------------------------------
  public static T Instance
  {
    get
    {
      if (_applicationIsQuitting)
      {
        Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Returning null.");
        return null;
      }
      
      if (s_instance == null)
      {
        lock (s_lock)
        {
          if (s_instance == null)
          {
            s_instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
          }
        }
      }
      return s_instance;
    }
  }
  //-------------------------------------------------------------------------
  protected virtual void Awake()
  {
    if (s_instance == null)    
      s_instance = this as T;    
    else 
      Destroy(gameObject);    
  }
  //-------------------------------------------------------------------------
  protected virtual void OnApplicationQuit()
  {
    // 애플리케이션이 종료될 때 인스턴스가 다시 생성되는 것을 방지하기 위해 null로 설정
    _applicationIsQuitting = true;
    s_instance = null;
  }
}
