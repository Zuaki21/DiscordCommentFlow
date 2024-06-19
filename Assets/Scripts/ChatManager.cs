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
        [SerializeField, Range(-30, 30)] public float topMargin = -10;
        [SerializeField, Range(0f, 10)] public float margin = 1;
        [SerializeField] Transform commentParent;
        [SerializeField] GameObject flowCommentPrefab;
        [SerializeField] SuperChatMaterial[] superChatMaterials;

        const float topLocalY = 335;
        const float rightLocalX = 650;

        public static void AddChatElement(ChatElement[] chatElements)
        {
            Instance.CreateFlowChat(chatElements);

            // VoiceVoxを使う設定の場合は音声生成リストに追加
            if (SettingManager.Settings.useVoiceVox)
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

                if (newElement.role != SpeakerRole.GPT)
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
                TextMeshProUGUI textMesh = newFlowChatObject.GetComponent<TextMeshProUGUI>();
                //コメントを設定
                textMesh.text = newElement.Message;

                //色を設定
                if (SettingManager.Settings.useCustomColor)
                    textMesh.color = SettingManager.Settings.textColor;

                //Materialを設定
                textMesh.fontMaterial = FontData.GetRoleMaterial(textMesh.fontMaterial, newElement.role);

                //親を設定
                newFlowChatObject.transform.SetParent(commentParent);

                //サイズを設定
                newFlowChatObject.transform.localScale = Vector3.one;
                FlowComment flowComment = newFlowChatObject.GetComponent<FlowComment>();

                //コメントの速度を設定
                if (chatMoney > 0) flowComment.fixedSpeedParam = 0.8f;

                ////////////////////////////////////////////////////////////
                //コメントの初期位置を設定
                await UniTask.Yield();//textMeshPro.boundsの取得に1フレーム必要
                flowComment.transform.localPosition = new Vector2(rightLocalX, topLocalY - topMargin - flowComment.commentUpperHeights);
                //コメントの位置が重ならないように必要に応じて変更
                SetFlowChatPosition(flowComment);
                ////////////////////////////////////////////////////////////

                allFlowComments.Add(flowComment);
                await UniTask.Delay(millisecondsDelay: Random.Range(0, 2000));
            }
        }
        int JudgeSuperChat(string text)
        {
            //全角の数字を半角に変換
            text = Regex.Replace(text, @"[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString());

            // カンマを除外
            text = text.Replace(",", "");

            // 正規表現パターンを定義
            string pattern = @"[￥¥](\d+)";

            // 正規表現で金額を抽出
            Match match = Regex.Match(text, pattern);

            // 金額を取得し、文字列から除外
            int amount = 0;
            string remainingText = text;

            if (match.Success)
            {
                //金額を取得(Int.Parseできるように8文字以内にする)
                string money = match.Groups[1].Value.Substring(0, Mathf.Min(8, match.Groups[1].Value.Length));
                amount = int.Parse(money);
            }

            //100円以下はスーパーチャットとして認めない
            if (amount < 100) return 0;

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
        void SetFlowChatPosition(FlowComment newflowComment)
        {
            int roop = 0;
            while (true)
            {
                bool isOverlap = false;
                foreach (FlowComment flowComment in allFlowComments)
                {
                    //1フレーム待つ

                    //ローカル座標に変換
                    if (IsOverlappingHeights(flowComment, newflowComment) && IsPredictConflicts(flowComment, newflowComment) == true)
                    {
                        float yInterval = flowComment.commentLowerHeights + newflowComment.commentUpperHeights + margin + 0.01f;
                        newflowComment.transform.localPosition = new Vector2(newflowComment.transform.localPosition.x, flowComment.transform.localPosition.y - yInterval);
                        isOverlap = true;
                        break;
                    }
                }
                roop++;
                if (roop > 100)
                {
                    Debug.Log("roop over");
                    break;
                }

                if (isOverlap == false)
                    break;
            }
        }

        //y座標と高さ幅heighから重なるかどうかを予測する
        bool IsOverlappingHeights(FlowComment earlyComment, FlowComment lateComment)
        {
            float earlyTop = earlyComment.transform.localPosition.y + earlyComment.commentUpperHeights;
            float earlyBottom = earlyComment.transform.localPosition.y - earlyComment.commentLowerHeights;
            float lateTop = lateComment.transform.localPosition.y + lateComment.commentUpperHeights;
            float lateBottom = lateComment.transform.localPosition.y - lateComment.commentLowerHeights;

            if (earlyTop + margin < lateBottom || lateTop + margin < earlyBottom) return false;
            else return true;
        }


        // コメントが衝突するかどうかを予測する
        bool IsPredictConflicts(FlowComment earlyComment, FlowComment lateComment)
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
    public class SuperChatMaterial
    {
        public Material material;
        public int min = 0;
    }
}
