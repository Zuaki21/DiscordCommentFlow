using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zuaki;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zuaki
{
    public class Settings : SingletonMonoBehaviour<Settings>
    {
        [SerializeField] public float flowSpeed = 1.0f;
        [SerializeField, Range(20, 60)] public int fontSize = 40;
        [SerializeField, Range(0, 100)] public float topMargin = 0;
        [SerializeField] GameObject SettingPanel;
        [SerializeField] TMP_InputField urlInputField;
        public bool useGPT = true;
        public bool useVoiceVox = true;
        public bool useVoiceVoxOnGPT = false;
        public bool useTestChat = false;
        public bool useLocalVoiceVox = false;
        public bool useNameOnVoice = false;
        public bool useVoiceRecognize = false;
        class TextObject { public string text; public TextObject(string text) { this.text = text; } }
        public string GPT_WebAPI
        {
            get
            {
                if (_GPT_WebAPI == "")
                {
                    TextObject textObject = new TextObject("");
                    SaveMethods.Load(textObject, "GPT_WebAPI");
                    _GPT_WebAPI = textObject.text;
                }
                return _GPT_WebAPI;
            }
            set
            {
                Debug.Log("GPT_WebAPIを設定しました");
                TextObject textObject = new TextObject(value);
                SaveMethods.Save(textObject, "GPT_WebAPI");
                _GPT_WebAPI = value;
            }
        }
        private string _GPT_WebAPI = "";
        public string VOICEVOX_WebAPI = "インスペクターから設定してください";
        public string url = "https://www.google.com/";
        protected void Start()
        {
            if (useTestChat) url = test_url;

            urlInputField.text = url;
        }
        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (SettingPanel.activeSelf)
                {
                    SettingPanel.SetActive(false);
                }
                else
                {
                    SettingPanel.SetActive(true);
                }
            }
        }
        const string test_url = "https://streamkit.discord.com/overlay/chat/966842017902657626/1154233939733520484?icon=true&online=true&logo=white&text_color=%23ffffff&text_size=14&text_outline_color=%23000000&text_outline_size=0&text_shadow_color=%23000000&text_shadow_size=0&bg_color=%231e2124&bg_opacity=0.95&bg_shadow_color=%23000000&bg_shadow_size=0&invite_code=wu8pUa3nZ7&limit_speaking=false&small_avatars=false&hide_names=false&fade_chat=0";

        [System.Serializable]
        public class DefaultSpeechOption
        {
            [Range(0, 66)] public int defaultSpeakerID = 3;
            [Range(0.5f, 2)] public float defaultSpeechSpeed = 1.0f;
            [Range(-0.1f, 0.1f)] public float defaultPitch = 0.0f;
            [Range(0f, 2f)] public float defaultIntonationScale = 1.0f;
        }
    }
}

