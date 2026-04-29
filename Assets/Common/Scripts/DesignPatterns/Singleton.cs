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
}
