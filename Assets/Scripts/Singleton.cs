/*
 * Singleton.cs
 * 
 * - Unity Implementation of Singleton template
 * 
 */

using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [Header("Singleton Options")]
    public bool UseLazyInstantiation = false;

    protected virtual void Awake()
    {              
        if(I != null && I != this as T)
        {
            Destroy(this.gameObject);
        }
        else
        {
            I = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public static T I { get; private set; }

    public virtual void ResetSingleton()
    {
        if (I) 
        {
            Destroy(I.gameObject);
            I = null;   
        }

    }
}
