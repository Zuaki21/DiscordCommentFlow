using AnnulusGames.LucidTools.Editor;
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
        // "Name/Style"の形式で取得
        public static string[] AllCharacterStyles
        {
            get
            {
                List<string> AllCharacterStyles = new List<string>();
                foreach (var voiceCharacter in Instance.voiceCharacters)
                {
                    foreach (var style in voiceCharacter.styles)
                    {
                        AllCharacterStyles.Add($"{voiceCharacter.name}/{style.name}");
                    }
                }
                return AllCharacterStyles.ToArray();
            }
        }
        public static VoiceCharacter[] VoiceCharacters => Instance.voiceCharacters;

        [SerializeField, HideInInspector] VoiceCharacter[] voiceCharacters;
        [SerializeField] Object jsonObject;

#if UNITY_EDITOR
        [ContextMenu("Load"), Button("LoadJson")]
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

        [ContextMenu("Delete"), Button("DeleteData")]
        void DeleteData()
        {
            voiceCharacters = null;
            //データの保存
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();

        }
        [System.Serializable]
        public class VoiceCharacterArray
        {
            public VoiceCharacter[] voiceCharacters;
        }
#endif
    }
    ///////////////////////////////
    /// 以下、jsonのデータ構造
    [System.Serializable]
    public class VoiceCharacter
    {
        [HideInInspector] public string name;
        public Style[] styles;
    }
    [System.Serializable]
    public class Style
    {
        [HideInInspector] public string name;
        [ReadOnly] public int id;
    }
}
