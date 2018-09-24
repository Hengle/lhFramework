using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Singleton<T> where T : class, new()
{
    protected static T p_instance;
    public static T GetInstance()
    {
        if (Singleton<T>.p_instance != null) return null;
        return Activator.CreateInstance<T>();
    }
    public async virtual Task Initialize(Action onInitialOver=null)
    {

    }
    public virtual void Dispose()
    {
        if (Singleton<T>.p_instance != null)
        {
            Singleton<T>.p_instance = (T)((object)null);
        }
    }

}
