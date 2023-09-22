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

namespace Zuaki
{
    public class ScrapingSelenium : SingletonMonoBehaviour<ScrapingSelenium>
    {
        public string url = "https://www.google.com/";
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
                driver.Navigate().GoToUrl(url);
                // 画面キャプチャを撮る
                _ = CheckNewComment();
            });
        }
        public void ChangeURL(string newUrl)
        {
            url = newUrl;
            driver.Navigate().GoToUrl(url);
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
                    ChatManager.Instance.AddChatElement(newChatElements.ToArray());
                }
                await UniTask.Delay(1000);// 1秒待つ
            }
        }

        async UniTask<List<ChatElement>> GetNewComment()
        {
            await UniTask.SwitchToThreadPool();

            ReadOnlyCollection<IWebElement> messageElements = driver.FindElementsByClassName("Chat_messageText__k79m4");
            ReadOnlyCollection<IWebElement> nameElements = driver.FindElementsByClassName("Chat_username__5fTg6");
            ReadOnlyCollection<IWebElement> timeElements = driver.FindElementsByClassName("Chat_timestamp__nyBmU");

            // 取得した要素をnewChatElementにとして持つ
            List<ChatElement> newChatElements = new List<ChatElement>();
            for (int i = 0; i < messageElements.Count; i++)
            {
                // メッセージの内容がないならスキップ
                if (messageElements[i].Text == "") continue;

                ChatElement chatElement = new ChatElement();
                chatElement.Message = messageElements[i].Text;
                chatElement.Name = nameElements[i].Text;
                chatElement.Time = timeElements[i].Text;

                // 重複しているかどうかをチェック
                bool isDuplicate = false;
                foreach (ChatElement element in chatElementHistory)
                {
                    if (chatElement.Message == element.Message &&
                        chatElement.Name == element.Name &&
                        chatElement.Time == element.Time)
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
            }

            await UniTask.SwitchToMainThread(); // メインスレッドに戻る
            return newChatElements;
        }

        void OnDestroy()
        {
            driver.Dispose();
        }

        public void QuitWindow()
        {
            driver.Quit();
        }
    }

    public class ChatElement
    {
        public string Message;
        public string Name;
        public string Time;
    }

}
