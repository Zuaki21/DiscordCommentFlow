using UnityEngine;
using System;
using System.Threading;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static SynchronizationContext context;
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Type t = typeof(T);

                instance = (T)FindObjectOfType(t);
                if (instance == null)
                {
                    ConvenientMethods.DebugLogErrorInEditor(t + " をアタッチしているGameObjectはありません");
                }
            }
            return instance;
        }
    }

    virtual protected void Awake()
    {
        // 他のゲームオブジェクトにアタッチされているか調べる
        // アタッチされている場合は破棄する。
        CheckInstance();
        context = SynchronizationContext.Current;
        //継承先でAwakeを実装する場合は必ずbase.Awake()を呼ぶこと
    }

    protected bool CheckInstance()
    {
        if (instance == null)
        {
            instance = this as T;
            return true;
        }
        else if (Instance == this)
        {
            return true;
        }
        Destroy(this);
        return false;
    }
    protected static void RunOnMainThread(Action action)
    {
        context.Post(_ => action(), null);
    }
}
