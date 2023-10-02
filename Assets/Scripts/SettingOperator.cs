using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zuaki;
using System.Linq;
using Fab.UITKDropdown;
using UnityEditor.U2D.Animation;
using System.Threading;

namespace Zuaki
{
    public class SettingOperator : SingletonMonoBehaviour<SettingOperator>
    {
        SynchronizationContext context;
        UIDocument uiDocument;
        Label loadingChannelLabel;
        string loadingChannelText = "ページ読込中です...";

        protected void OnEnable()
        {
            context = SynchronizationContext.Current;

            uiDocument = GetComponent<UIDocument>();
            VisualElement root = uiDocument.rootVisualElement;

            // URL設定
            TextField urlField = root.Q<TextField>("URLField");
            Button URLSubmit = root.Q<Button>("URLSubmit");
            urlField.value = Settings.Instance.url;
            URLSubmit.SetEnabled(false);
            URLSubmit.RegisterCallback<ClickEvent>((evt) =>
            {
                Settings.Instance.url = urlField.value;
                ScrapingSelenium.Instance.ChangeURL(Settings.Instance.url);
                URLSubmit.SetEnabled(false);
            });
            urlField.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                URLSubmit.SetEnabled(true);
            });

            // チャット設定(GPTとVOICEVOXが両方必要)
            var readAICommentsToggle = root.Q<Toggle>("ReadAIComments");
            readAICommentsToggle.value = Settings.Instance.useVoiceVoxOnGPT;
            readAICommentsToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                Settings.Instance.useVoiceVoxOnGPT = evt.newValue;
            });

            // GPT設定
            TextField chatGPTAPIField = root.Q<TextField>("ChatGPTAPIField");
            Button chatGPTAPISubmit = root.Q<Button>(name: "ChatGPTAPISubmit");
            GroupBox GPTAPIGroup = root.Q<GroupBox>("GPTAPIGroup");
            Toggle useAiToggle = root.Q<Toggle>("UseAIToggle");
            if (Settings.Instance.GPT_WebAPI == "")
            {
                chatGPTAPIField.value = "APIキーを入力してください";
            }
            else
            {
                chatGPTAPIField.value = Settings.Instance.GPT_WebAPI;
            }
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

            // VoiceVox設定
            Toggle useVoiceVoxToggle = root.Q<Toggle>("UseVoiceVoxToggle");
            Toggle readNameToggle = root.Q<Toggle>("ReadUserName");
            RadioButtonGroup VoiceVoxTypeGroup = root.Q<RadioButtonGroup>("VoiceVoxTypeGroup");
            RadioButton localVoiceVox = root.Q<RadioButton>("LocalVoiceVox");
            RadioButton webVoiceVox = root.Q<RadioButton>("WebVoiceVox");
            readNameToggle.value = Settings.Instance.useNameOnVoice;
            localVoiceVox.value = Settings.Instance.useLocalVoiceVox;
            webVoiceVox.value = !Settings.Instance.useLocalVoiceVox;

            VisualElement readCharacterGroup = root.Q<VisualElement>("ReadCharacterGroup");


            useVoiceVoxToggle.value = Settings.Instance.useVoiceVox;
            VoiceVoxTypeGroup.SetEnabled(Settings.Instance.useVoiceVox);
            readNameToggle.SetEnabled(Settings.Instance.useVoiceVox);
            readCharacterGroup.SetEnabled(Settings.Instance.useVoiceVox);
            useVoiceVoxToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                Settings.Instance.useVoiceVox = evt.newValue;
                VoiceVoxTypeGroup.SetEnabled(evt.newValue);
                readNameToggle.SetEnabled(evt.newValue);
                readAICommentsToggle.SetEnabled(evt.newValue && Settings.Instance.useGPT);
                readCharacterGroup.SetEnabled(evt.newValue);
            });
            readNameToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                Settings.Instance.useNameOnVoice = evt.newValue;
            });
            readAICommentsToggle.SetEnabled(Settings.Instance.useVoiceVox && Settings.Instance.useGPT);
            localVoiceVox.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                Settings.Instance.useLocalVoiceVox = evt.newValue;
            });

            //読み上げキャラクター設定
            foreach (SpeakerRole role in System.Enum.GetValues(typeof(SpeakerRole)))
            {
                MakeMenu(role, root);
            }

            //LoadingChannelText
            loadingChannelLabel = root.Q<Label>("LoadingChannelText");
            loadingChannelLabel.text = loadingChannelText;
        }

        void MakeMenu(SpeakerRole role, VisualElement root)
        {
            Dropdown dropdown = new Dropdown(root);
            DropdownMenu characterMenu = new DropdownMenu();
            Button btn = root.Q<Button>(name: role.ToString() + "Voice");
            btn.clickable.clicked += () => dropdown.Open(characterMenu, btn.worldBound);

            foreach (var characterStyle in VoiceCharacterData.AllCharacterStyles)
                characterMenu.AppendAction(characterStyle, (DropdownMenuAction action)
                => SetVoice(action, role, btn));
            btn.text = role.ToString() + "\n<size=20>" + VoiceCharacterData.AllCharacterStyles[SpeakerData.GetRoleSetting(role).speakerID];
        }

        private void SetVoice(DropdownMenuAction action, SpeakerRole role, Button button)
        {
            string[] characterStyle = action.name.Split('/');
            VoiceCharacter character = VoiceCharacterData.VoiceCharacters.First(x => x.name == characterStyle[0]);
            Style style = character.styles.First(x => x.name == characterStyle[1]);
            SpeakerData.GetRoleSetting(role).speakerID = style.id;
            button.text = role.ToString() + "\n<size=20>" + action.name;
        }

        // 設定画面のオブジェクトを更新する
        public static void Reload()
        {
            Instance.context.Post(_ =>
            {
                Instance.OnEnable();
            }, null);
        }

        public static void SetVoiceVoxType()
        {
            Instance.context.Post(_ =>
            {
                Instance.uiDocument.rootVisualElement.Q<RadioButtonGroup>("VoiceVoxTypeGroup").value = Settings.Instance.useLocalVoiceVox ? 0 : 1;
            }, null);
        }
        // チャンネルの読み込み中のテキストを表示する
        public static void SetChannelText(string text)
        {
            Instance.context.Post(_ =>
            {
                Instance.loadingChannelText = text;
                Instance.loadingChannelLabel.text = text;
            }, null);
        }
    }
}
