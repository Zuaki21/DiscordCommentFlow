using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zuaki;
using System.Linq;

namespace Zuaki
{
    public class SettingOperator : SingletonMonoBehaviour<SettingOperator>
    {
        UIDocument uiDocument;
        Label channelName;
        protected void Start()
        {
            uiDocument = GetComponent<UIDocument>();
            VisualElement root = uiDocument.rootVisualElement;

            // URL設定
            TextField urlField = root.Q<TextField>("URLField");
            Button URLSubmit = root.Q<Button>("URLSubmit");
            urlField.value = Settings.Instance.url;
            URLSubmit.RegisterCallback<ClickEvent>((evt) =>
            {
                Settings.Instance.url = urlField.value;
                ScrapingSelenium.Instance.ChangeURL(Settings.Instance.url);
            });

            // チャット設定(GPTとVOICEVOXが両方必要)
            var readAICommentsToggle = root.Q<Toggle>("ReadAIComments");

            // GPT設定
            TextField chatGPTAPIField = root.Q<TextField>("ChatGPTAPIField");
            Button chatGPTAPISubmit = root.Q<Button>(name: "ChatGPTAPISubmit");
            GroupBox GPTAPIGroup = root.Q<GroupBox>("GPTAPIGroup");
            Toggle useAiToggle = root.Q<Toggle>("UseAIToggle");
            useAiToggle.value = Settings.Instance.useGPT;
            GPTAPIGroup.SetEnabled(Settings.Instance.useGPT);
            useAiToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                Settings.Instance.useGPT = evt.newValue;
                GPTAPIGroup.SetEnabled(evt.newValue);
                readAICommentsToggle.SetEnabled(evt.newValue && Settings.Instance.useVoiceVox);
            });
            chatGPTAPISubmit.RegisterCallback<ClickEvent>((evt) =>
            {
                Settings.Instance.GPT_WebAPI = chatGPTAPIField.value;
            });

            channelName = root.Q<Label>("ChannelName");


            // VoiceVox設定
            Toggle useVoiceVoxToggle = root.Q<Toggle>("UseVoiceVoxToggle");
            Toggle readNameToggle = root.Q<Toggle>("ReadName");
            RadioButtonGroup VoiceVoxTypeGroup = root.Q<RadioButtonGroup>("VoiceVoxTypeGroup");
            RadioButton localVoiceVox = root.Q<RadioButton>("LocalVoiceVox");
            RadioButton webVoiceVox = root.Q<RadioButton>("WebVoiceVox");
            localVoiceVox.value = Settings.Instance.useLocalVoiceVox;
            webVoiceVox.value = !Settings.Instance.useLocalVoiceVox;

            Foldout readCharacterFoldout = root.Q<Foldout>("ReadCharacterFoldout");


            useVoiceVoxToggle.value = Settings.Instance.useVoiceVox;
            VoiceVoxTypeGroup.SetEnabled(Settings.Instance.useVoiceVox);
            readNameToggle.SetEnabled(Settings.Instance.useVoiceVox);
            readCharacterFoldout.SetEnabled(Settings.Instance.useVoiceVox);
            useVoiceVoxToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                Settings.Instance.useVoiceVox = evt.newValue;
                VoiceVoxTypeGroup.SetEnabled(evt.newValue);
                readNameToggle.SetEnabled(evt.newValue);
                readAICommentsToggle.SetEnabled(evt.newValue && Settings.Instance.useGPT);
                readCharacterFoldout.SetEnabled(evt.newValue);
            });

            readAICommentsToggle.SetEnabled(Settings.Instance.useVoiceVox && Settings.Instance.useGPT);
            localVoiceVox.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                Settings.Instance.useLocalVoiceVox = evt.newValue;
            });

            //読み上げキャラクター設定
            DropdownField programmerVoice = root.Q<DropdownField>("ProgrammerVoice");
            DropdownField illustratorVoice = root.Q<DropdownField>("IllustratorVoice");
            DropdownField soundCreatorVoice = root.Q<DropdownField>("SoundCreatorVoice");
            DropdownField GPTVoice = root.Q<DropdownField>("GPTVoice");
            DropdownField otherVoice = root.Q<DropdownField>("OtherVoice");
            // 選択肢を動的に生成
            List<string> voiceNameStyles = VoiceCharacterData.AllCharacterStyles.ToList();
            programmerVoice.choices = voiceNameStyles;
            illustratorVoice.choices = voiceNameStyles;
            soundCreatorVoice.choices = voiceNameStyles;
            GPTVoice.choices = voiceNameStyles;
            otherVoice.choices = voiceNameStyles;

        }

        public static void SetChannelName(string name)
        => Instance.channelName.text = name;
    }
}
