using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zuaki;
using System.Linq;
using Fab.UITKDropdown;
using System.Threading;
using DG.Tweening;
using SFB;
using Game.UI;

namespace Zuaki
{
    public class SettingOperator : SingletonMonoBehaviour<SettingOperator>
    {
        UIDocument uiDocument;
        Label loadingChannelLabel;
        [SerializeField] GameObject LogPanel;
        bool detailSetting = false;
        string loadingChannelText = "チャット読込中です...";
        Scroller verticalScroller;

        protected void OnEnable()
        {

            uiDocument = GetComponent<UIDocument>();
            VisualElement root = uiDocument.rootVisualElement;

            ScrollView scrollView = root.Q<ScrollView>("ScrollView");
            verticalScroller = scrollView.verticalScroller;

            // URL設定
            TextField urlField = root.Q<TextField>("URLField");
            Button URLSubmit = root.Q<Button>("URLSubmit");
            if (SettingManager.URL == "")
            {
                urlField.value = "URLを入力してください";
            }
            else
            {
                urlField.value = SettingManager.URL;
            }
            URLSubmit.SetEnabled(false);
            URLSubmit.RegisterCallback<ClickEvent>((evt) =>
            {
                SettingManager.URL = urlField.value;
                ScrapingSelenium.Instance.ChangeURL(SettingManager.URL);
                URLSubmit.SetEnabled(false);
            });
            urlField.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                URLSubmit.SetEnabled(true);
            });

            // チャット設定(GPTとVOICEVOXが両方必要)
            var readAICommentsToggle = root.Q<Toggle>("ReadAIComments");
            readAICommentsToggle.value = SettingManager.Settings.useVoiceVoxOnGPT;
            readAICommentsToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                SettingManager.Settings.useVoiceVoxOnGPT = evt.newValue;
            });

            // GPT設定
            TextField chatGPTAPIField = root.Q<TextField>("ChatGPTAPIField");
            Button chatGPTAPISubmit = root.Q<Button>(name: "ChatGPTAPISubmit");
            GroupBox GPTAPIGroup = root.Q<GroupBox>("GPTAPIGroup");
            Toggle useAiToggle = root.Q<Toggle>("UseAIToggle");
            if (SettingManager.GPT_WebAPI == "")
            {
                chatGPTAPIField.value = "APIキーを入力してください";
            }
            else
            {
                chatGPTAPIField.value = SettingManager.GPT_WebAPI;
            }
            useAiToggle.value = SettingManager.Settings.useGPT;
            GPTAPIGroup.SetEnabled(SettingManager.Settings.useGPT);
            useAiToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                SettingManager.Settings.useGPT = evt.newValue;
                GPTAPIGroup.SetEnabled(evt.newValue);
                readAICommentsToggle.SetEnabled(evt.newValue && SettingManager.Settings.useVoiceVox);
            });
            chatGPTAPISubmit.RegisterCallback<ClickEvent>((evt) =>
            {
                SettingManager.GPT_WebAPI = chatGPTAPIField.value;
            });

            // VoiceVox設定
            Toggle useVoiceVoxToggle = root.Q<Toggle>("UseVoiceVoxToggle");
            Toggle readNameToggle = root.Q<Toggle>("ReadUserName");
            RadioButtonGroup VoiceVoxTypeGroup = root.Q<RadioButtonGroup>("VoiceVoxTypeGroup");
            RadioButton localVoiceVox = root.Q<RadioButton>("LocalVoiceVox");
            RadioButton webVoiceVox = root.Q<RadioButton>("WebVoiceVox");
            readNameToggle.value = SettingManager.Settings.useNameOnVoice;
            localVoiceVox.value = SettingManager.Settings.useLocalVoiceVox;
            webVoiceVox.value = !SettingManager.Settings.useLocalVoiceVox;
            VisualElement readGroup = root.Q<VisualElement>("ReadGroup");
            VisualElement readCharacterGroup = root.Q<VisualElement>("ReadCharacterGroup");


            useVoiceVoxToggle.value = SettingManager.Settings.useVoiceVox;
            readGroup.SetEnabled(SettingManager.Settings.useVoiceVox);
            useVoiceVoxToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                SettingManager.Settings.useVoiceVox = evt.newValue;
                readGroup.SetEnabled(evt.newValue);
                readAICommentsToggle.SetEnabled(evt.newValue && SettingManager.Settings.useGPT);
            });
            readNameToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                SettingManager.Settings.useNameOnVoice = evt.newValue;
            });
            readAICommentsToggle.SetEnabled(SettingManager.Settings.useVoiceVox && SettingManager.Settings.useGPT);
            localVoiceVox.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                SettingManager.Settings.useLocalVoiceVox = evt.newValue;
            });

            //読み上げキャラクター設定
            foreach (SpeakerRole role in System.Enum.GetValues(typeof(SpeakerRole)))
            {
                MakeRoleVoiceMenu(role, root);
            }

            //LoadingChannelText
            loadingChannelLabel = root.Q<Label>("LoadingChannelText");
            loadingChannelLabel.text = "状態：" + loadingChannelText;

            Button consoleLogButton = root.Q<Button>("ConsoleLogButton");
            consoleLogButton.RegisterCallback<ClickEvent>((evt) =>
            {
                LogPanel.SetActive(true);
            });

            Button closeButton = root.Q<Button>("CloseButton");
            closeButton.RegisterCallback<ClickEvent>((evt) =>
            {
                gameObject.SetActive(false);
            });

            VisualElement footer = root.Q<VisualElement>("Footer");
            Foldout detailSettingFoldout = root.Q<Foldout>("DetailSettingFoldout");
            ChangeFooterPosition(footer, root, detailSettingFoldout);

            detailSettingFoldout.value = detailSetting;
            detailSettingFoldout.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (!detailSetting && detailSettingFoldout.value)
                {
                    //0.1秒でverticalScroller.valueを600にする(DoTweenで)
                    DOTween.To(() => verticalScroller.value, x => verticalScroller.value = x, 560, 0.1f);
                }
                detailSetting = detailSettingFoldout.value;
                ChangeFooterPosition(footer, root, detailSettingFoldout);
            });

            Button resetButton = root.Q<Button>("ResetButton");
            resetButton.RegisterCallback<ClickEvent>((evt) =>
            {
                Reset();
            });
            SliderInt fontSizeSlider = root.Q<SliderInt>("FontSizeSlider");
            fontSizeSlider.value = SettingManager.Settings.fontSize;
            fontSizeSlider.RegisterCallback<ChangeEvent<int>>((evt) =>
            {
                SettingManager.Settings.fontSize = evt.newValue;
            });

            Slider flowSpeedSlider = root.Q<Slider>("FlowSpeedSlider");
            flowSpeedSlider.value = SettingManager.Settings.flowSpeed;
            flowSpeedSlider.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                SettingManager.Settings.flowSpeed = evt.newValue;
            });

            DropdownField fontField = root.Q<DropdownField>("FontField");
            fontField.choices = FontData.GetFontNames();
            fontField.value = FontData.CurrentFontName;
            fontField.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                FontData.SetCurrentFont(evt.newValue);
            });
            Button addNewFontButton = root.Q<Button>("AddNewFontButton");
            addNewFontButton.RegisterCallback<ClickEvent>((evt) =>
            {
                OpenAddFontBrowser(fontField);
            });

            SliderInt readVolumeSlider = root.Q<SliderInt>("ReadVolumeSlider");
            readVolumeSlider.value = SpeakerData.SpeakerOption.volume;
            readVolumeSlider.RegisterCallback<ChangeEvent<int>>((evt) =>
            {
                SoundManager.SetVolume((float)evt.newValue / 100, VolumeType.Master);
                SpeakerData.SpeakerOption.volume = evt.newValue;
            });

            Slider readSpeedSlider = root.Q<Slider>("ReadSpeedSlider");
            readSpeedSlider.value = SpeakerData.SpeakerOption.speed;
            readSpeedSlider.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                SpeakerData.SpeakerOption.speed = evt.newValue;
            });

            Slider readPitchSlider = root.Q<Slider>("ReadPitchSlider");
            readPitchSlider.value = SpeakerData.SpeakerOption.pitch;
            readPitchSlider.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                SpeakerData.SpeakerOption.pitch = evt.newValue;
            });

            Slider readIntonationScaleSlider = root.Q<Slider>("ReadIntonationScaleSlider");
            readIntonationScaleSlider.value = SpeakerData.SpeakerOption.intonationScale;
            readIntonationScaleSlider.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                SpeakerData.SpeakerOption.intonationScale = evt.newValue;
            });

            SliderInt readMaxLengthSlider = root.Q<SliderInt>("ReadMaxLengthSlider");
            readMaxLengthSlider.value = SpeakerData.SpeakerOption.maxCommentLength;
            readMaxLengthSlider.RegisterCallback<ChangeEvent<int>>((evt) =>
            {
                SpeakerData.SpeakerOption.maxCommentLength = evt.newValue;
            });

            SliderInt readMaxAllLengthSlider = root.Q<SliderInt>("ReadMaxAllLengthSlider");
            readMaxAllLengthSlider.value = SpeakerData.SpeakerOption.maxAllCommentLength;
            readMaxAllLengthSlider.RegisterCallback<ChangeEvent<int>>((evt) =>
            {
                SpeakerData.SpeakerOption.maxAllCommentLength = evt.newValue;
            });

            Label versionLabel = root.Q<Label>("VersionLabel");
            //製品名 Ver.バージョン
            versionLabel.text = $"{Application.productName} Ver.{Application.version}";

            var colorGroup = root.Q<VisualElement>("ColorGroup");
            var useCustomColorToggle = root.Q<Toggle>("UseCustomColorToggle");
            useCustomColorToggle.value = SettingManager.Settings.useCustomColor;
            colorGroup.SetEnabled(SettingManager.Settings.useCustomColor);
            useCustomColorToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                SettingManager.Settings.useCustomColor = evt.newValue;
                colorGroup.SetEnabled(evt.newValue);
            });

            var colorPopup = root.Q<ColorPopup>("color-popup");
            var textColorField = root.Q<ColorField>("TextColorField");
            var outlineColorField = root.Q<ColorField>("OutlineColorField");
            textColorField.ColorPopup = colorPopup;
            outlineColorField.ColorPopup = colorPopup;
            textColorField.value = SettingManager.Settings.textColor;
            outlineColorField.value = SettingManager.Settings.outtlineColor;
            textColorField.ResetButtonPressed += () => textColorField.value = Color.white;
            outlineColorField.ResetButtonPressed += () => outlineColorField.value = Color.black;
            textColorField.RegisterValueChangedCallback(evt =>
            {
                SettingManager.Settings.textColor = evt.newValue;
            });
            outlineColorField.RegisterValueChangedCallback(evt =>
            {
                SettingManager.Settings.outtlineColor = evt.newValue;
                FontData.SetCustomOutlineColor();
            });
        }

        void MakeRoleVoiceMenu(SpeakerRole role, VisualElement root)
        {
            Dropdown dropdown = new Dropdown(root);
            DropdownMenu characterMenu = new DropdownMenu();
            Button btn = root.Q<Button>(name: role.ToString() + "Voice");
            btn.clickable.clicked += () =>
            {
                if (verticalScroller.value > 350)
                {
                    //0.1秒でverticalScroller.valueを410にする(DoTweenで)
                    DOTween.To(() => verticalScroller.value, x => verticalScroller.value = x, 350, 0.1f).OnComplete(() =>
                    {
                        dropdown.Open(characterMenu, btn.worldBound);
                    });
                }
                else
                {
                    dropdown.Open(characterMenu, btn.worldBound);
                }

            };
            foreach (var characterStyle in VoiceCharacterData.AllCharacterStyles)
                characterMenu.AppendAction(characterStyle, (DropdownMenuAction action)
                => SetVoice(action, role, btn));
            btn.text = role.ToString() + "\n<size=12>" + VoiceCharacterData.AllCharacterStyles[SpeakerData.GetRoleSetting(role).speakerID];
        }

        private void SetVoice(DropdownMenuAction action, SpeakerRole role, Button button)
        {
            string[] characterStyle = action.name.Split('/');
            VoiceCharacter character = VoiceCharacterData.VoiceCharacters.First(x => x.name == characterStyle[0]);
            Style style = character.styles.First(x => x.name == characterStyle[1]);
            SpeakerData.GetRoleSetting(role).speakerID = style.id;
            button.text = role.ToString() + "\n<size=12>" + action.name;
        }

        // 設定画面のオブジェクトを更新する
        public static void Reload()
        {
            RunOnMainThread(() =>
             {
                 Instance.OnEnable();
             });
        }

        public static void SetVoiceVoxType()
        {
            RunOnMainThread(() =>
            {
                if (Instance.gameObject.activeSelf == false) return;
                Instance.uiDocument.rootVisualElement.Q<RadioButtonGroup>("VoiceVoxTypeGroup").value = SettingManager.Settings.useLocalVoiceVox ? 0 : 1;
            });
        }
        public static void SetUseGPT()
        {
            RunOnMainThread(() =>
            {
                if (Instance.gameObject.activeSelf == false) return;
                Instance.uiDocument.rootVisualElement.Q<Toggle>("UseAIToggle").value = SettingManager.Settings.useGPT;
            });
        }

        public static void SetUseVoiceVox()
        {
            RunOnMainThread(() =>
            {
                if (Instance.gameObject.activeSelf == false) return;
                Instance.uiDocument.rootVisualElement.Q<Toggle>("UseVoiceVoxToggle").value = SettingManager.Settings.useVoiceVox;
            });
        }

        // チャンネルの読み込み中のテキストを表示する
        public static void SetChannelText(string text)
        {
            RunOnMainThread(() =>
            {
                if (Instance.gameObject.activeSelf == false) return;
                Instance.loadingChannelText = text;
                Instance.loadingChannelLabel.text = "状態：" + text;
            });
        }

        public static void Reset()
        {
            SettingManager.Reset();
            Debug.Log("設定をリセットしました。");
            Reload();
        }

        public static void Restart()
        {
            // アプリを再起動する
            System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe"));
            Application.Quit();

        }

        void ChangeFooterPosition(VisualElement footer, VisualElement root, Foldout detailSettingFoldout)
        {
            if (detailSetting)
            {
                //DetailSettingFoldoutの親を取得
                VisualElement parent = detailSettingFoldout.parent;
                //footerをDetailSettingFoldoutの次に移動
                parent.Insert(parent.IndexOf(detailSettingFoldout) + 1, footer);
            }
            else
            {
                //ルート直下にあるVisualElementを取得
                VisualElement rootElement = root.ElementAt(0);
                //footerをルート直下にあるVisualElementの最後の要素として追加
                rootElement.Add(footer);
            }

        }
        public void OpenAddFontBrowser(DropdownField field)
        {

            ExtensionFilter[] extensions = new ExtensionFilter[] { new ExtensionFilter("フォントファイル", "ttf", "otf") };
            var dirPath = Application.streamingAssetsPath + "/Fonts";

            //選択画面を開いてパスを取得
            var paths = StandaloneFileBrowser.OpenFilePanel("フォントを追加する", dirPath, extensions, true);
            //ファイルを選択されてたら
            if (paths.Length > 0)
            {
                string fontPath = paths[0];
                FontData.AddNewFont(fontPath);

                string fontName = System.IO.Path.GetFileNameWithoutExtension(fontPath);
                field.choices = FontData.GetFontNames();
                FontData.SetCurrentFont(fontName);
                field.value = FontData.CurrentFontName;
            }
        }
    }
}
