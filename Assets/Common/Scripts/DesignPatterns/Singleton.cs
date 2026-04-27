using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
  private static T s_instance;
  public static T Instance
  {
    get
    {
      if (s_instance == null)
      {
        s_instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
        if (s_instance == null)
        {
          GameObject singletonObj = new GameObject(typeof(T).Name);
          s_instance = singletonObj.AddComponent<T>();
          DontDestroyOnLoad(singletonObj);
        }
      }
      return s_instance;
    }
  }
  //-------------------------------------------------------------------------
  protected virtual void Awake()
  {
    if (s_instance == null)
    {
      s_instance = this as T;
      DontDestroyOnLoad(gameObject);
    }

    else if (s_instance != this)
    {
      Destroy(gameObject);
    }
  }
}
