using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zuaki;
using TMPro;
using UnityEngine.SceneManagement;

namespace Zuaki
{
    public class Settings : SingletonMonoBehaviour<Settings>
    {
        static bool FirstTime = true;
        [SerializeField] public float flowSpeed = 1.0f;
        [SerializeField, Range(20, 60)] public int fontSize = 40;
        [SerializeField, Range(0, 100)] public float topMargin = 0;
        [SerializeField] GameObject SettingPanel;
        [SerializeField] TMP_InputField urlInputField;
        public bool useGPT = true;
        public bool isTest = false;
        public string GPT_WebAPI = "インスペクターから設定してください";
        public string VOICEVOX_WebAPI = "インスペクターから設定してください";
        public string url = "https://www.google.com/";
        protected void Start()
        {
            if (isTest) url = test_url;

            urlInputField.text = url;
        }
        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (SettingPanel.activeSelf)
                {
                    SettingPanel.SetActive(false);

                    ScrapingSelenium.Instance.ChangeURL(urlInputField.text);
                }
                else
                {
                    SettingPanel.SetActive(true);
                }
            }
        }
        const string test_url = "https://streamkit.discord.com/overlay/chat/966842017902657626/1154233939733520484?icon=true&online=true&logo=white&text_color=%23ffffff&text_size=14&text_outline_color=%23000000&text_outline_size=0&text_shadow_color=%23000000&text_shadow_size=0&bg_color=%231e2124&bg_opacity=0.95&bg_shadow_color=%23000000&bg_shadow_size=0&invite_code=wu8pUa3nZ7&limit_speaking=false&small_avatars=false&hide_names=false&fade_chat=0";
    }
}

