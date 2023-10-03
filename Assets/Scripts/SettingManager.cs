using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zuaki;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zuaki
{
    [System.Serializable]
    public class Settings
    {
        [SerializeField, Range(20, 100)] public int fontSize = 40;
        [SerializeField, Range(1f, 10f)] public float flowSpeed = 3.0f;
        [SerializeField, Range(0, 100)] public float topMargin = 30;
        public bool useGPT = false;
        public bool useVoiceVox = false;
        public bool useVoiceVoxOnGPT = false;
        public bool useTestChat = false;
        public bool useLocalVoiceVox = false;
        public bool useNameOnVoice = false;
    }
    public class SettingManager : SingletonMonoBehaviour<SettingManager>
    {
        [SerializeField] GameObject SettingPanel;
        [SerializeField] GameObject LogPanel;
        public static Settings Settings => Instance.settings;
        [SerializeField] Settings settings;
        class TextObject { public string text; public TextObject(string text) { this.text = text; } }
        class BoolObject { public bool boolean; public BoolObject(bool boolean) { this.boolean = boolean; } }
        public static string GPT_WebAPI
        {
            get
            {
                if (Instance.gpt_WebAPI == "")
                {
                    TextObject textObject = new TextObject("");
                    SaveMethods.Load(textObject, "GPT_WebAPI");
                    Instance.gpt_WebAPI = textObject.text;
                }
                return Instance.gpt_WebAPI;
            }
            set
            {
                TextObject textObject = new TextObject(value);
                RunOnMainThread(() => { SaveMethods.Save(textObject, "GPT_WebAPI"); });
                Instance.gpt_WebAPI = value;
            }
        }
        private string gpt_WebAPI = "";
        public static string[] VOICEVOX_WebAPI => Instance._VOICEVOX_WebAPI;
        [SerializeField] string[] _VOICEVOX_WebAPI;
        public static string URL
        {
            get
            {
                if (Instance.url == "")
                {
                    TextObject textObject = new TextObject("");
                    SaveMethods.Load(textObject, "url");
                    if (textObject.text == "") textObject.text = temporary_chat_url;
                    Instance.url = textObject.text;
                }
                return Instance.url;
            }
            set
            {
                TextObject textObject = new TextObject(value);
                RunOnMainThread(() => { SaveMethods.Save(textObject, "url"); });
                Instance.url = value;
            }
        }
        private string url = "";
        protected override void Awake()
        {
            base.Awake();
            settings.Load("Settings");
            if (settings.useTestChat) url = test_url;
        }
        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
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
            if (Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.LeftControl))
            {
                LogPanel.SetActive(!LogPanel.activeSelf);
            }
        }
        protected void OnApplicationQuit()
        {
            // セーブ
            SpeakerData.Instance.Save("SpeakerData");
            settings.Save("Settings");
        }

        const string test_url = "https://streamkit.discord.com/overlay/chat/966842017902657626/1154233939733520484?icon=true&online=true&logo=white&text_color=%23ffffff&text_size=14&text_outline_color=%23000000&text_outline_size=0&text_shadow_color=%23000000&text_shadow_size=0&bg_color=%231e2124&bg_opacity=0.95&bg_shadow_color=%23000000&bg_shadow_size=0&invite_code=wu8pUa3nZ7&limit_speaking=false&small_avatars=false&hide_names=false&fade_chat=0";
        const string temporary_chat_url = "https://streamkit.discord.com/overlay/chat/694543179885838376/1150323972932636753?icon=true&online=true&logo=white&text_color=%23ffffff&text_size=14&text_outline_color=%23000000&text_outline_size=0&text_shadow_color=%23000000&text_shadow_size=0&bg_color=%231e2124&bg_opacity=0.95&bg_shadow_color=%23000000&bg_shadow_size=0&invite_code=wu8pUa3nZ7&limit_speaking=false&small_avatars=false&hide_names=false&fade_chat=0";
    }
}

