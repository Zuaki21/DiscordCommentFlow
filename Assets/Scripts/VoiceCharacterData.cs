using AnnulusGames.LucidTools.Inspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zuaki;

namespace Zuaki
{
    [CreateAssetMenu(fileName = "VoiceCharacterData", menuName = "Zuaki/VoiceCharacterData", order = 100)]
    public class VoiceCharacterData : SingletonScriptableObject<VoiceCharacterData>
    {
        public static VoiceCharacter[] VoiceCharacters => Instance.voiceCharacters;
        [SerializeField] VoiceCharacter[] voiceCharacters;
        [SerializeField] Object jsonObject;

#if UNITY_EDITOR
        [ContextMenu("Load"), Button("Load")]
        void LoadData()
        {
            //jsonObjectのパスを取得
            string path = UnityEditor.AssetDatabase.GetAssetPath(jsonObject);
            //jsonファイルをテキストとして読み込む
            string json = System.IO.File.ReadAllText(path);
            //jsonを配列に変換
            VoiceCharacterArray voiceCharacterArray = new VoiceCharacterArray();
            JsonUtility.FromJsonOverwrite(json, voiceCharacterArray);
            voiceCharacters = voiceCharacterArray.voiceCharacters;

            //データの保存
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        [ContextMenu("Delete"), Button("Delete")]
        void DeleteData()
        {
            voiceCharacters = null;
            //データの保存
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();

        }
#endif
    }

    [System.Serializable]
    public class VoiceCharacterArray
    {
        public VoiceCharacter[] voiceCharacters;
    }
    [System.Serializable]
    public class VoiceCharacter
    {
        public string name;
        public Style[] styles;
    }
    [System.Serializable]
    public class Style
    {
        public string name;
        public int id;
    }
}
