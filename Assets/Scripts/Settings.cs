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
        protected void Start()
        {
            urlInputField.text = ScrapingSelenium.Instance.url;
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
    }
}
