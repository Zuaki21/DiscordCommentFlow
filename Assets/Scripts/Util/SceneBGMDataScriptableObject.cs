using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SceneBGMDataScriptableObject", menuName = "Zuaki/SceneBGMDataScriptableObject", order = 100)]
public class SceneBGMDataScriptableObject : SingletonScriptableObject<SceneBGMDataScriptableObject>
{
    public static SceneBGM[] SceneBGMs => Instance.sceneBGMs;

    [Header(@"使い方
    SceneBGMs配列にシーンとBGMをセットしてください。
    対象のシーンを読み込んだら自動的にBGMが再生されます。
    (セットされていないシーンでは前シーンでのBGMが引き継がれます。)
    (シーンをセットし、BGMをセットしていない場合はBGMは再生されません。)")]
    public SceneBGM[] sceneBGMs;

    // インスペクター上に名前を変えたいだけのメソッド
    protected void OnValidate()
    {
        if (sceneBGMs != null)
        {
            foreach (var item in sceneBGMs)
            {
                if (item.clip != null)
                {
                    item.name = null;
                    item.name += item.sceneObject.name;

                    if (item.clip != null)
                    {
                        item.name += " -> ";
                    }
                    item.name += item.clip.name;
                }
                else
                {
                    item.name = null;
                }
            }
        }
    }
}

[System.Serializable]
/// <summary>
/// シーンとBGMの対応付けとループ設定を格納するクラス
/// </summary>
public class SceneBGM
{
    [SerializeField, HideInInspector] public string name;//配列の要素名
    [Header("対象のシーン")]
    public SceneObject sceneObject = null;
    [Header("流したいBGMファイル")]
    public AudioClip clip = null;
    [Range(0f, 1f)] public float volume = 0.5f;
    [Space(7)]
    [Tooltip("チェックを入れるとBGMがループします")]
    public bool isLoop = false;
    public AudioClip introClip = null;
}
