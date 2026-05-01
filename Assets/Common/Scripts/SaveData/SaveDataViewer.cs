using UnityEngine;

public abstract class SaveDataViewer<T> : MonoBehaviour where T : class, new()
{
    public T data = new T();
}