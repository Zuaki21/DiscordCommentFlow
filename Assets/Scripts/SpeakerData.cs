using System.Collections;
using System.Collections.Generic;
using Mochineko.ChatGPT_API;
using UnityEngine;
using Zuaki;

namespace Zuaki
{
    [CreateAssetMenu(fileName = "SpeakerData", menuName = "Zuaki/SpeakerData", order = 100)]
    public class SpeakerData : SingletonScriptableObject<SpeakerData>
    {
        public RoleVoice roleVoice;

        public static RoleVoice RoleVoice => Instance.roleVoice;
        public DefaultSpeechOption defaultOption;
        public static DefaultSpeechOption DefaultOption => Instance.defaultOption;
        public static int GetSpeakerID(Commenter commenter)
        {
            return commenter switch
            {
                Commenter.Programmer => RoleVoice.Programmer,
                Commenter.Illustrator => RoleVoice.Illustrator,
                Commenter.SoundCreator => RoleVoice.SoundCreator,
                Commenter.ScenarioWriter => RoleVoice.ScenarioWriter,
                Commenter.GPT => RoleVoice.GPT,
                _ => RoleVoice.Other,
            };
        }

    }

    [System.Serializable]
    public class RoleVoice
    {
        [Range(0, 66)] public int Programmer;
        [Range(0, 66)] public int Illustrator;
        [Range(0, 66)] public int SoundCreator;
        [Range(0, 66)] public int ScenarioWriter;
        [Range(0, 66)] public int GPT;
        public int Other => SpeakerData.DefaultOption.defaultSpeakerID;
    }
    [System.Serializable]
    public class DefaultSpeechOption
    {
        [Range(0, 66)] public int defaultSpeakerID = 3;
        [Range(0.5f, 2)] public float defaultSpeechSpeed = 1.0f;
        [Range(-0.1f, 0.1f)] public float defaultPitch = 0.0f;
        [Range(0f, 2f)] public float defaultIntonationScale = 1.0f;
    }
}
