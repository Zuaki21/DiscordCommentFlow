using System.Collections;
using System.Collections.Generic;
using Mochineko.ChatGPT_API;
using Unity.Burst;
using UnityEngine;
using Zuaki;
using AnnulusGames.LucidTools.Inspector;
using UnityEditor;

namespace Zuaki
{
    [CreateAssetMenu(fileName = "SpeakerData", menuName = "Zuaki/SpeakerData", order = 100)]
    public class SpeakerData : SingletonScriptableObject<SpeakerData>
    {
        [HideInInspector] public RoleVoice roleVoice;

        public static RoleVoice RoleVoice => Instance.roleVoice;
        public SpeakerOption speakerOption;
        public static SpeakerOption SpeakerOption => Instance.speakerOption;
        public static int GetSpeakerID(SpeakerRole commenter)
        => GetRoleSetting(commenter).speakerID;
        public static RoleSetting GetRoleSetting(SpeakerRole commenter)
        => Instance._GetRoleSetting(commenter);

        RoleSetting _GetRoleSetting(SpeakerRole commenter)
        {
            return commenter switch
            {
                SpeakerRole.Programmer => roleVoice.Programmer,
                SpeakerRole.Illustrator => roleVoice.Illustrator,
                SpeakerRole.SoundCreator => roleVoice.SoundCreator,
                SpeakerRole.ScenarioWriter => roleVoice.ScenarioWriter,
                SpeakerRole.GPT => roleVoice.GPT,
                SpeakerRole.Other => roleVoice.Other,
                _ => roleVoice.Other,
            };
        }


#if UNITY_EDITOR
        void OnValidate()
        {
            foreach (var role in RoleVoice.GetType().GetFields())
            {
                if (role.FieldType == typeof(RoleSetting))
                {
                    var roleSetting = (RoleSetting)role.GetValue(RoleVoice);
                    roleSetting.Setting();
                }
            }
        }
#endif

    }

    [System.Serializable]
    public class RoleVoice
    {
        public RoleSetting Programmer;
        public RoleSetting Illustrator;
        public RoleSetting SoundCreator;
        public RoleSetting ScenarioWriter;
        public RoleSetting GPT;
        public RoleSetting Other;
    }

    [System.Serializable]
    public class SpeakerOption
    {
        [Range(0.5f, 2)] public float speed = 1.0f;
        [Range(-0.1f, 0.1f)] public float pitch = 0.0f;
        [Range(0f, 2f)] public float intonationScale = 1.0f;
        [Range(10, 100)] public int maxCommentLength = 50;
        [Range(50, 200)] public int maxAllCommentLength = 100;
    }
    public enum SpeakerRole
    {
        Programmer = 0,
        Illustrator = 1,
        SoundCreator = 2,
        ScenarioWriter = 3,
        GPT = 4,
        Other = 5,
    }
    
    [System.Serializable]
    public class RoleSetting
    {
        [ReadOnly] public string name;
        [Range(0, 25)] public int characterNum = 0;
        [Range(0, 5)] public int styleNum = 0;
        public int speakerID
        {
            get
            {
                if (characterNum < 0 || characterNum >= VoiceCharacterData.VoiceCharacters.Length) return 0;
                if (styleNum < 0 || styleNum >= VoiceCharacterData.VoiceCharacters[characterNum].styles.Length) return 0;
                return VoiceCharacterData.VoiceCharacters[characterNum].styles[styleNum].id;
            }
            set
            {
                for (int i = 0; i < VoiceCharacterData.VoiceCharacters.Length; i++)
                {
                    for (int j = 0; j < VoiceCharacterData.VoiceCharacters[i].styles.Length; j++)
                    {
                        if (VoiceCharacterData.VoiceCharacters[i].styles[j].id == value)
                        {
                            characterNum = i;
                            styleNum = j;
                            return;
                        }
                    }
                }
                characterNum = 0;
                styleNum = 0;
            }
        }

#if UNITY_EDITOR
        public void Setting()
        {
            if (VoiceCharacterData.VoiceCharacters.Length == 0) return;
            if ((styleNum < 0 || styleNum > VoiceCharacterData.VoiceCharacters[characterNum].styles.Length - 1))
            {
                EditorGUI.FocusTextInControl("");
                name = "未定義";
                if (styleNum < 0) styleNum = 0;
                else styleNum = VoiceCharacterData.VoiceCharacters[characterNum].styles.Length - 1;

                //保存する
                EditorUtility.SetDirty(VoiceCharacterData.Instance);
                AssetDatabase.SaveAssets();
            }

            name = $"{VoiceCharacterData.VoiceCharacters[characterNum].name}({VoiceCharacterData.VoiceCharacters[characterNum].styles[styleNum].name})";
        }
#endif
    }
}
