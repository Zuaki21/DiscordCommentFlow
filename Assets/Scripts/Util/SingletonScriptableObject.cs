using UnityEngine;
using UnityEditor;
using System;

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T instance = null;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load(typeof(T).Name) as T;
                (instance as SingletonScriptableObject<T>).OnInitialize();
            }
            return instance;
        }
    }
    // インスタンスを初期化するためのオプションのオーバーライド可能なメソッド
    protected virtual void OnInitialize() { }
    //OnEnableはエディタ上含め有効になった時に呼ばれる
    //OnInitializeは他のスクリプトから最初にアクセスされた時に呼ばれる
}
