using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;
using System;
using OpenQA.Selenium.Chrome;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zuaki
{
    public class ScrapingSelenium : SingletonMonoBehaviour<ScrapingSelenium>
    {
        private ChromeDriver driver;
        public bool isHeadless = true;
        [SerializeField] GameObject LoadingCommentObj;

        async void Start()
        {
            LoadingCommentObj.SetActive(true);
            await Task.Run(() =>
            {
                var driverPath = Application.streamingAssetsPath;
                // 現在のプロセスの環境変数を取得
                string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);

                // 新しいディレクトリを追加
                string updatedPath = currentPath + ";" + driverPath;
                // 更新した環境変数をプロセスに適用
                Environment.SetEnvironmentVariable("PATH", updatedPath, EnvironmentVariableTarget.Process);

                // ChromeOptionsを設定してヘッドレスモードを有効にする
                ChromeOptions options = new ChromeOptions();

                if (isHeadless)
                {
                    // ヘッドレスモードを有効にする
                    options.AddArgument("--headless");
                }

                ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                // NOTE: 起動にはそこそこ時間がかかる
                driver = new ChromeDriver(driverService, options);

                // 起動後は好きなようにChromeを操作できる
                driver.Navigate().GoToUrl(Settings.Instance.url);
                // 画面キャプチャを撮る
                _ = CheckNewComment();
            });
        }

        public void ChangeURL(string newUrl)
        {
            Settings.Instance.url = newUrl;
            driver.Navigate().GoToUrl(Settings.Instance.url);
        }

        List<ChatElement> chatElementHistory = new List<ChatElement>();
        async UniTask CheckNewComment()
        {
            while (true)
            {
                await UniTask.SwitchToThreadPool();
                List<ChatElement> newChatElements = await GetNewComment();
                await UniTask.SwitchToMainThread(); // メインスレッドに戻る

                if (newChatElements.Count > 0)
                {
                    if (LoadingCommentObj.activeSelf) LoadingCommentObj.SetActive(false);
                    AddComment(newChatElements.ToArray());
                }
                await UniTask.Delay(1000);// 1秒待つ
            }
        }

        async void AddComment(ChatElement[] chatElements)
        {
            if (chatElements.Length == 0) return;
            // 新しいコメントがあったら追加
            ChatManager.AddChatElement(chatElements);

            // GPTを使う設定の場合はコメントを生成して追加
            if (Settings.Instance.useGPT && Settings.Instance.GPT_WebAPI != "")
            {
                ChatElement[] generatedComments = await CommentGenerator.GetComments(chatElements);
                ChatManager.AddChatElement(generatedComments);
            }
        }

        async UniTask<List<ChatElement>> GetNewComment()
        {
            await UniTask.SwitchToThreadPool();

            List<ChatElement> chatElements = GetComment();
            List<ChatElement> newChatElements = FilterNewComments(chatElements);

            await UniTask.SwitchToMainThread(); // メインスレッドに戻る
            return newChatElements;
        }

        List<ChatElement> GetComment()
        {
            ReadOnlyCollection<IWebElement> messageElements = driver.FindElementsByClassName("Chat_messageText__k79m4");
            ReadOnlyCollection<IWebElement> nameElements = driver.FindElementsByClassName("Chat_username__5fTg6");
            ReadOnlyCollection<IWebElement> timeElements = driver.FindElementsByClassName("Chat_timestamp__nyBmU");
            IWebElement channelElement = driver.FindElementByClassName("Chat_channelName__O6KEu");
            if (channelElement == null && channelElement.Text == null && channelElement.Text == "")
            {
                Debug.Log("チャンネルを取得できませんでした");
                SettingOperator.SetChannelText("チャンネルを取得できませんでした");
            }
            else
            {
                if (channelElement.Text.Contains("#loading.."))
                    SettingOperator.SetChannelText("ページ読込中です...");
                else
                    SettingOperator.SetChannelText(channelElement.Text);
            }
            // 取得した要素をnewChatElementにとして持つ
            List<ChatElement> chatElements = new List<ChatElement>();

            for (int i = 0; i < messageElements.Count; i++)
            {
                // メッセージの内容がないならスキップ
                if (messageElements[i] == null) continue;
                if (messageElements[i].Text == null) continue;
                if (messageElements[i].Text == "") continue;

                ChatElement chatElement = new ChatElement(
                    messageElements[i].Text,
                    nameElements[i].Text,
                    timeElements[i].Text,
                    nameElements[i].GetAttribute("style")
                );
                chatElements.Add(chatElement);
            }
            return chatElements;
        }
        List<ChatElement> FilterNewComments(List<ChatElement> chatElements)
        {
            List<ChatElement> newChatElements = new List<ChatElement>();
            foreach (ChatElement chatElement in chatElements)
            {
                // 重複しているかどうかをチェック
                bool isDuplicate = false;
                foreach (ChatElement oldChatElement in chatElementHistory)
                {
                    if (chatElement.IsEqual(oldChatElement))
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if (isDuplicate) continue; // 重複していたらスキップ

                // 重複していなかったら新しいコメントとして追加
                newChatElements.Add(chatElement);
                // 重複していなかったらchatElementsに追加
                chatElementHistory.Add(chatElement);
                // chatElementsの数が20を超えたら古いものから削除
                if (chatElementHistory.Count > 20)
                {
                    chatElementHistory.RemoveAt(0);
                }
            }
            return newChatElements;
        }


        void OnDestroy()
        {
            QuitWindow();
        }

        public void QuitWindow()
        {
            if (driver != null)
                driver.Quit();
        }
    }

    public class ChatElement
    {
        public string Message { get; private set; } = null;
        public string Name { get; private set; } = null;
        public string Time { get; private set; } = null;
        public SpeakerRole Commenter = SpeakerRole.Other;
        public int SpeakerID => SpeakerData.GetSpeakerID(Commenter);
        public ChatElement(string message = null, string name = null, string time = null, SpeakerRole commenter = SpeakerRole.Other)
        {
            this.Message = message;
            this.Name = name;
            this.Time = time;
            this.Commenter = commenter;
        }
        public ChatElement(string message = null, string name = null, string time = null, string style = null)
        {
            this.Message = message;
            this.Name = name;
            this.Time = time;
            this.Commenter = GetCommenter(style);
        }

        SpeakerRole GetCommenter(string style)
        {
            if (style.Contains("color: rgb(241, 196, 15);")) return SpeakerRole.Programmer;
            if (style.Contains("color: rgb(46, 204, 113);")) return SpeakerRole.Illustrator;
            if (style.Contains("color: rgb(52, 152, 219);")) return SpeakerRole.SoundCreator;
            if (style.Contains("color: rgb(231, 76, 60);")) return SpeakerRole.ScenarioWriter;
            return SpeakerRole.Other;
        }
        public bool IsEqual(ChatElement other)
        {
            if (this.Message == other.Message &&
                this.Name == other.Name &&
                this.Time == other.Time &&
                this.Commenter == other.Commenter)
            {
                return true;
            }
            return false;
        }
    }
}
