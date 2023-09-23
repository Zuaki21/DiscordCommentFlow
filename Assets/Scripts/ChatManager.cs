using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;
using TMPro;

namespace Zuaki
{
    public class ChatManager : SingletonMonoBehaviour<ChatManager>
    {
        List<FlowComment> allFlowComments = new List<FlowComment>();
        [SerializeField] Transform commentParent;
        [SerializeField] GameObject flowCommentPrefab;

        float topLocalY = 335;
        float rightLocalX = 650;
        float leftLocalY = -650;

        protected void Start()
        {
        }
        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log(allFlowComments.Count);
            }
        }

        public void AddChatElement(string[] newComments)
        {
            foreach (string newComment in newComments)
            {
                GameObject newFlowChatObject = Instantiate(flowCommentPrefab);
                //コメントを設定
                newFlowChatObject.GetComponent<TextMeshProUGUI>().text = newComment;
                //親を設定
                newFlowChatObject.transform.SetParent(commentParent);

                //サイズを設定
                newFlowChatObject.transform.localScale = Vector3.one;
                FlowComment flowComment = newFlowChatObject.GetComponent<FlowComment>();
                //コメントの位置が重ならないように必要に応じて変更
                newFlowChatObject.transform.localPosition = GetFlowChatPosition(flowComment);
                allFlowComments.Add(flowComment);
            }
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
            float relativeSpeed = lateComment.commentSpeed - earlyComment.commentSpeed;
            float relativeConflictTime = relativeDistance / relativeSpeed;

            if (relativeDistance <= 0) return true; // 既に衝突しているならtrueを返す
            else if (relativeSpeed <= 0) return false; //　離れていく関係ならfalseを返す

            // 先に出現したコメントの消失までの時間を求める
            float earlyDistance = earlyComment.transform.localPosition.x + earlyComment.commentLength - rightLocalX;
            float earlyEndTime = earlyDistance / earlyComment.commentSpeed;

            //  コメントが消える前に衝突するならtrueを返す
            if (earlyEndTime <= relativeConflictTime) return true;
            else return false;
        }
    }
}
