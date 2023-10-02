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
    public class Settings : SingletonMonoBehaviour<Settings>
    {
        SynchronizationContext context;
        [SerializeField] public float flowSpeed = 1.0f;
        [SerializeField, Range(20, 60)] public int fontSize = 40;
        [SerializeField, Range(0, 100)] public float topMargin = 0;
        [SerializeField] GameObject SettingPanel;
        [SerializeField] GameObject LogPanel;
        [SerializeField] TMP_InputField urlInputField;
        public bool useGPT = true;
        public bool useVoiceVox = true;
        public bool useVoiceVoxOnGPT = false;
        public bool useTestChat = false;
        public bool useLocalVoiceVox = false;
        public bool useNameOnVoice = false;
        public bool useVoiceRecognize = false;
        public int maxCommentLength = 50;
        public int maxAllCommentLength = 100;
        class TextObject { public string text; public TextObject(string text) { this.text = text; } }
        class BoolObject { public bool boolean; public BoolObject(bool boolean) { this.boolean = boolean; } }
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
                Instance.context.Post(_ => { SaveMethods.Save(textObject, "GPT_WebAPI"); }, null);
                _GPT_WebAPI = value;
            }
        }
        private string _GPT_WebAPI = "";
        public static string[] VOICEVOX_WebAPI => Instance._VOICEVOX_WebAPI;
        [SerializeField] string[] _VOICEVOX_WebAPI;
        public string url
        {
            get
            {
                if (_url == "")
                {
                    TextObject textObject = new TextObject("");
                    SaveMethods.Load(textObject, "url");
                    if (textObject.text == "") textObject.text = temporary_chat_url;
                    _url = textObject.text;
                }
                return _url;
            }
            set
            {
                Debug.Log("urlを設定しました");
                TextObject textObject = new TextObject(value);
                Instance.context.Post(_ => { SaveMethods.Save(textObject, "url"); }, null);
                _url = value;
            }
        }
        private string _url = "";
        protected override void Awake()
        {
            base.Awake();
            context = SynchronizationContext.Current;
        }
        protected void Start()
        {
#if UNITY_EDITOR
            if (useTestChat) url = test_url;
#endif
            urlInputField.text = url;
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
        const string test_url = "https://streamkit.discord.com/overlay/chat/966842017902657626/1154233939733520484?icon=true&online=true&logo=white&text_color=%23ffffff&text_size=14&text_outline_color=%23000000&text_outline_size=0&text_shadow_color=%23000000&text_shadow_size=0&bg_color=%231e2124&bg_opacity=0.95&bg_shadow_color=%23000000&bg_shadow_size=0&invite_code=wu8pUa3nZ7&limit_speaking=false&small_avatars=false&hide_names=false&fade_chat=0";
        const string temporary_chat_url = "https://streamkit.discord.com/overlay/chat/694543179885838376/1150323972932636753?icon=true&online=true&logo=white&text_color=%23ffffff&text_size=14&text_outline_color=%23000000&text_outline_size=0&text_shadow_color=%23000000&text_shadow_size=0&bg_color=%231e2124&bg_opacity=0.95&bg_shadow_color=%23000000&bg_shadow_size=0&invite_code=wu8pUa3nZ7&limit_speaking=false&small_avatars=false&hide_names=false&fade_chat=0";

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

