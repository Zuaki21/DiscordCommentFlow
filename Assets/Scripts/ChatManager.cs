using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;
using TMPro;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Zuaki
{
    public class ChatManager : SingletonMonoBehaviour<ChatManager>
    {
        List<FlowComment> allFlowComments = new List<FlowComment>();
        [SerializeField] Transform commentParent;
        [SerializeField] GameObject flowCommentPrefab;
        [SerializeField] ChatMaterial chatMaterial;
        [SerializeField] SuperChatMaterial[] superChatMaterials;

        float topLocalY = 335;
        float rightLocalX = 650;

        public static void AddChatElement(ChatElement[] chatElements)
        {
            Instance.CreateFlowChat(chatElements);

            // VoiceVoxを使う設定の場合は音声生成リストに追加
            if (Settings.Instance.useVoiceVox)
            {
                VoiceSpeaker.AddComment(chatElements);
            }
        }

        async void CreateFlowChat(ChatElement[] chatElements)
        {
            string[] newComments = chatElements.Select(e => e.Message).ToArray();
            foreach (ChatElement newElement in chatElements)
            {
#if UNITY_EDITOR
                if (EditorApplication.isPlaying == false) return;
#endif
                Material superChatMaterial = null;
                int chatMoney = 0;
                chatMoney = JudgeSuperChat(newElement.Message);
                superChatMaterial = GetSuperChatMaterial(chatMoney);

                GameObject newFlowChatObject = Instantiate(flowCommentPrefab);

                if (chatMoney > 0)
                {
                    var background = newFlowChatObject.GetComponent<TextMeshProBackground>();
                    background.enabled = true;
                    background.material = superChatMaterial;
                }

                //新しいコメントを生成
                TextMeshProUGUI textMeshProUGUI = newFlowChatObject.GetComponent<TextMeshProUGUI>();
                //コメントを設定
                textMeshProUGUI.text = newElement.Message;
                //Materialを設定
                textMeshProUGUI.fontMaterial = chatMaterial.GetMaterial(newElement.Commenter);
                //親を設定
                newFlowChatObject.transform.SetParent(commentParent);

                //サイズを設定
                newFlowChatObject.transform.localScale = Vector3.one;
                FlowComment flowComment = newFlowChatObject.GetComponent<FlowComment>();
                //コメントの位置が重ならないように必要に応じて変更
                newFlowChatObject.transform.localPosition = GetFlowChatPosition(flowComment);
                allFlowComments.Add(flowComment);
                if (chatMoney > 0)
                    flowComment.fixedSpeedParam = 0.5f;

                await UniTask.Delay(millisecondsDelay: Random.Range(0, 2000));
            }
        }
        int JudgeSuperChat(string text)
        {
            // 正規表現パターンを定義
            string pattern = @"￥(\d+)";

            // 正規表現で金額を抽出
            Match match = Regex.Match(text, pattern);

            // 金額を取得し、文字列から除外
            int amount = 0;
            string remainingText = text;

            if (match.Success)
            {
                amount = int.Parse(match.Groups[1].Value);
            }

            return amount;
        }

        Material GetSuperChatMaterial(int amount)
        {
            foreach (SuperChatMaterial superChat in superChatMaterials)
            {
                if (amount >= superChat.min)
                {
                    return superChat.material;
                }
            }
            return null;
        }

        public void RemoveFlowChatList(FlowComment comment)
        {
            allFlowComments.RemoveAt(allFlowComments.IndexOf(comment));
        }

        //コメントの高さが重ならないようにする
        Vector3 GetFlowChatPosition(FlowComment newflowComment)
        {
            float localPosY = topLocalY;
            while (true)
            {
                bool isOverlap = false;
                foreach (FlowComment flowComment in allFlowComments)
                {
                    //ローカル座標に変換
                    if (Mathf.Approximately(flowComment.transform.localPosition.y, localPosY) && PredictCommentConflicts(flowComment, newflowComment) == true)
                    {
                        localPosY -= (Settings.Instance.fontSize + 5);
                        isOverlap = true;
                        break;
                    }
                }
                if (isOverlap == false)
                    break;
            }
            return new Vector3(rightLocalX, localPosY, 0);
        }

        // コメントが衝突するかどうかを予測する
        bool PredictCommentConflicts(FlowComment earlyComment, FlowComment lateComment)
        {
            // 相対衝突時間を求める
            float relativeDistance = rightLocalX - (earlyComment.transform.localPosition.x + earlyComment.commentLength);
            float relativeSpeed = lateComment.commentSpeed * lateComment.fixedSpeedParam - earlyComment.commentSpeed * earlyComment.fixedSpeedParam;
            float relativeConflictTime = relativeDistance / relativeSpeed;

            if (relativeDistance <= 0) return true; // 既に衝突しているならtrueを返す
            else if (relativeSpeed <= 0) return false; //　離れていく関係ならfalseを返す

            // 先に出現したコメントの消失までの時間を求める
            float earlyDistance = earlyComment.transform.localPosition.x + earlyComment.commentLength - rightLocalX;
            float earlyEndTime = earlyDistance / (earlyComment.commentSpeed * earlyComment.fixedSpeedParam);

            //  コメントが消える前に衝突するならtrueを返す
            if (earlyEndTime <= relativeConflictTime) return true;
            else return false;
        }
    }
    [System.Serializable]
    public class ChatMaterial
    {
        public Material Programmer;
        public Material Illustrator;
        public Material SoundCreator;
        public Material ScenarioWriter;
        public Material GPT;
        public Material Other;

        public Material GetMaterial(SpeakerRole commenter)
        {
            return commenter switch
            {
                SpeakerRole.Programmer => Programmer,
                SpeakerRole.Illustrator => Illustrator,
                SpeakerRole.SoundCreator => SoundCreator,
                SpeakerRole.ScenarioWriter => ScenarioWriter,
                SpeakerRole.GPT => GPT,
                _ => Other,
            };
        }
    }

    [System.Serializable]
    public class SuperChatMaterial
    {
        public Material material;
        public int min = 0;
    }
}
