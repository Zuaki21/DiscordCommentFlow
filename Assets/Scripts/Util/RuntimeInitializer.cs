using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zuaki;

public static class RuntimeInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static async void Init()
    {
        VoiceVoxWebManager.APIkeyIndex = 0;

        // SpeakerDataのデータを読み込む
        SpeakerData.Instance.Load("SpeakerData");

        // シーン遷移時に破棄されないオブジェクトを生成する
        CreateDontDestroyObject<SoundManager>();
        CreateDontDestroyObject<VoiceSpeaker>();

        // 1フレーム待機
        await UniTask.Yield();

        // 音量を設定
        SoundManager.SetVolume((float)SpeakerData.SpeakerOption.volume / 100, VolumeType.Master);
    }

    /// <summary>
    /// シーン遷移時に破棄されないオブジェクトを生成する
    /// </summary>
    /// <typeparam name="T">生成時に追加でアタッチするコンポーネント</typeparam>
    static GameObject CreateDontDestroyObject<T>(GameObject prefab = null) where T : MonoBehaviour
    {
        GameObject obj = CreateDontDestroyObject(prefab);
        obj.AddComponent<T>();

        if (prefab == null)
            obj.name = typeof(T).Name;

        return obj;
    }

    static GameObject CreateDontDestroyObject(GameObject prefab = null)
    {
        // prefabが指定されている場合はprefabから生成する
        // prefabがnullの場合はGameObjectを生成する
        GameObject obj = prefab
        ? GameObject.Instantiate(prefab)
        : new GameObject();
        Object.DontDestroyOnLoad(obj);

        obj.name = obj.name.Replace("(Clone)", "");
        return obj;
    }

}
