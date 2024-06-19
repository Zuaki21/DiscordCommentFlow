using System.Collections.Generic;
using UnityEngine;
using System;
using OpenQA.Selenium.Chrome;
using Cysharp.Threading.Tasks;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Threading;
using System.Text.RegularExpressions;


namespace Zuaki
{
    public class ScrapingSelenium : SingletonMonoBehaviour<ScrapingSelenium>
    {
        private ChromeDriver driver;
        public bool isHeadless = true;
        [SerializeField] GameObject LoadingCommentObj;
        void Start()
        {
            LoadingCommentObj.SetActive(true);

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

            try
            {
                // NOTE: 起動にはそこそこ時間がかかる
                driver = new ChromeDriver(driverService, options);
            }
            catch (Exception e)
            {
                Debug.LogError("ChromeDriverの起動に失敗しました。起動にはバージョン117台のGoogleChromeが必要です。");
                Debug.LogError($"<size=15>{e.Message}</size>");
                return;
            }

            // 起動後は好きなようにChromeを操作できる
            Debug.Log($"Discord Streamkit Overlayを開きます。\n<size=10><color=#e0ffff><u><link=\"{SettingManager.URL}\">{SettingManager.URL}</link></u></color></size>");
            driver.Navigate().GoToUrl(SettingManager.URL);
            // 画面キャプチャを撮る(あえて非同期で実行している)
            _ = CheckNewComment();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && Input.GetKey(KeyCode.LeftControl))
            {
                AddTestChat();
            }
        }
        public void AddTestChat()
        {
            //役職はランダム
            SpeakerRole role = (SpeakerRole)UnityEngine.Random.Range(0, Enum.GetNames(typeof(SpeakerRole)).Length);
            AddChatElements(new ChatElement[] { new ChatElement("これはテストコメントです", role: role) });
        }

        public void ChangeURL(string newUrl)
        {
            Debug.Log($"Discord Streamkit OverlayのURLを変更しました。\n<size=10><color=#e0ffff><u><link=\"{newUrl}\">{newUrl}</link></u></color></size=10>");
            SettingManager.URL = newUrl;
            driver.Navigate().GoToUrl(SettingManager.URL);
        }

        List<ChatElement> chatElementHistory = new List<ChatElement>();
        async UniTask CheckNewComment()
        {
            while (true)
            {
                _ = GetNewComment();
                await UniTask.Delay(1000);// 1秒待つ
            }
        }

        async void AddChatElements(ChatElement[] chatElements)
        {
            if (chatElements.Length == 0) return;
            // 新しいコメントがあったら追加
            ChatManager.AddChatElement(chatElements);

            // GPTを使う設定の場合はコメントを生成して追加
            if (SettingManager.Settings.useGPT && SettingManager.GPT_WebAPI != "")
            {
                ChatElement[] generatedComments = await CommentGenerator.GetComments(chatElements);
                if (generatedComments == null) return;
                ChatManager.AddChatElement(generatedComments);
            }
        }

        async UniTask GetNewComment()
        {
            // スレッドプールに切り替え、コメント取得時に固まらないようにする
            await UniTask.SwitchToThreadPool();

            List<ChatElement> chatElements = GetComment();
            List<ChatElement> newChatElements = FilterNewComments(chatElements);

            await UniTask.SwitchToMainThread(); // メインスレッドに戻る

            if (newChatElements.Count > 0)
            {
                if (LoadingCommentObj.activeSelf) LoadingCommentObj.SetActive(false);
                AddChatElements(newChatElements.ToArray());
            }
        }
        bool isError = false;
        List<ChatElement> GetComment()
        {
            // 取得した要素をnewChatElementにとして持つ
            List<ChatElement> chatElements = new List<ChatElement>();

            // メッセージの要素を取得
            ReadOnlyCollection<IWebElement> messageElements = driver.FindElementsByClassName("Chat_messageText__k79m4");
            ReadOnlyCollection<IWebElement> nameElements = driver.FindElementsByClassName("Chat_username__5fTg6");
            ReadOnlyCollection<IWebElement> timeElements = driver.FindElementsByClassName("Chat_timestamp__nyBmU");
            ReadOnlyCollection<IWebElement> channelElements = driver.FindElementsByClassName("Chat_channelName__O6KEu");

            if (channelElements.Count == 0)
            {
                RunOnMainThread(() =>
                {
                    if (!isError)
                    {
                        Debug.LogError("チャットを取得できませんでした。Discord Streamkit OverlayのURLが間違っている可能性があります。");
                        isError = true;
                    }
                });
                SettingOperator.SetChannelText("チャットを取得できませんでした");
                return chatElements;
            }

            IWebElement channelElement = channelElements[0];
            if (channelElement != null && !string.IsNullOrWhiteSpace(channelElement.Text))
            {
                isError = false;
                if (channelElement.Text.Contains("#loading.."))
                    SettingOperator.SetChannelText("チャット読込中です...");
                else
                    SettingOperator.SetChannelText(channelElement.Text);
            }
            else
            {
                RunOnMainThread(() =>
                {
                    if (!isError)
                    {
                        Debug.Log("チャットを取得できませんでした");
                        isError = true;
                    }
                });
                SettingOperator.SetChannelText("チャットを取得できませんでした");
            }


            for (int i = 0; i < messageElements.Count; i++)
            {
                // メッセージの内容がないならスキップ
                if (messageElements[i] == null) continue;
                if (string.IsNullOrWhiteSpace(messageElements[i].Text)) continue;

                SpeakerRole role = ChatElement.GetRole(nameElements[i].GetAttribute("style"));
                ChatElement chatElement = new ChatElement(
                    messageElements[i].Text,
                    nameElements[i].Text,
                    timeElements[i].Text,
                    role
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
                if (chatElementHistory.Count > 50)
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
        public SpeakerRole role = SpeakerRole.Other;
        public int SpeakerID => SpeakerData.GetSpeakerID(role);
        public ChatElement(string message = null, string name = null, string time = null, SpeakerRole role = SpeakerRole.Other)
        {
            this.Message = message;
            this.Name = name;
            this.Time = time;
            this.role = role;
        }
        public static SpeakerRole GetRole(string style)
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
                this.role == other.role)
            {
                return true;
            }
            return false;
        }
    }
}
