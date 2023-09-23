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
        [SerializeField, TextArea(30, 30)] string json;

        [ContextMenu("Load"), Button("Load")]
        void LoadData()
        {
            VoiceCharacterArray voiceCharacterArray = new VoiceCharacterArray(voiceCharacters);
            JsonUtility.FromJsonOverwrite(json, voiceCharacterArray);
        }
    }

    [System.Serializable]
    public class VoiceCharacterArray
    {
        [SerializeField] VoiceCharacter[] voiceCharacters;
        public VoiceCharacterArray(VoiceCharacter[] voiceCharacters)
        {
            this.voiceCharacters = voiceCharacters;
        }
    }
    [System.Serializable]
    public class VoiceCharacter
    {
        [SerializeField] string name;
        [SerializeField] Tone[] styles;
    }
    [System.Serializable]
    public class Tone
    {
        [SerializeField] string name;
        [SerializeField] int id;
    }
}
