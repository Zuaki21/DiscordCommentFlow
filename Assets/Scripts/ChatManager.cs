using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;
using TMPro;

namespace Zuaki
{
    public class ChatManager : SingletonMonoBehaviour<ChatManager>
    {
        List<ChatElement> allChatElements = new List<ChatElement>();
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

        public void AddChatElement(ChatElement[] newChatElements)
        {
            foreach (ChatElement newChatElement in newChatElements)
            {
                allChatElements.Add(newChatElement);

                GameObject newFlowChatObject = Instantiate(flowCommentPrefab);
                //コメントを設定
                newFlowChatObject.GetComponent<TextMeshProUGUI>().text = newChatElement.Message;
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
            //Debug.Log("----------------------------");
            //Debug.Log("【コメント】:" + newflowComment.gameObject.GetComponent<TextMeshProUGUI>().text);
            float localPosY = topLocalY;
            while (true)
            {
                //Debug.Log("☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆");
                bool isOverlap = false;
                foreach (FlowComment flowComment in allFlowComments)
                {
                    //ローカル座標に変換
                    if (Mathf.Approximately(flowComment.transform.localPosition.y, localPosY) && PredictCommentConflicts(flowComment, newflowComment) == true)
                    {
                        //Debug.Log("重なる:" + flowComment.transform.localPosition.y + " " + localPosY);
                        //重なるなら高さを下げる
                        localPosY -= (Settings.Instance.fontSize + 5);
                        isOverlap = true;
                        break;
                    }
                    else
                    {
                        //Debug.Log("重ならない:" + flowComment.transform.localPosition.y + " " + localPosY + "【比較】:" + Mathf.Approximately(flowComment.transform.localPosition.y, localPosY) + "【衝突】:" + PredictCommentConflicts(flowComment, newflowComment));
                        //重ならないなら次のコメントと比較
                        continue;
                    }
                }
                if (isOverlap == false)
                    break;
            }
            //Debug.Log("LocalPosY: " + localPosY);
            return new Vector3(rightLocalX, localPosY, 0);
        }

        // コメントが衝突するかどうかを予測する
        bool PredictCommentConflicts(FlowComment earlyComment, FlowComment lateComment)
        {
            float relativeDistance = rightLocalX - (earlyComment.transform.localPosition.x + earlyComment.commentLength);
            //Debug.Log("earlyComment.transform.localPosition.x: " + earlyComment.transform.localPosition.x + " earlyComment.commentLength: " + earlyComment.commentLength + " lateComment.transform.localPosition.x: " + rightLocalX);
            float relativeSpeed = lateComment.commentSpeed - earlyComment.commentSpeed;
            //Debug.Log("lateComment.commentSpeed: " + lateComment.commentSpeed + " earlyComment.commentSpeed: " + earlyComment.commentSpeed + " relativeSpeed: " + relativeSpeed);

            if (relativeDistance <= 0)
                return true;
            else if (relativeSpeed <= 0)
                return false;

            //Debug.Log("relativeDistance: " + relativeDistance + " relativeSpeed: " + relativeSpeed);
            float relativeConflictTime = relativeDistance / relativeSpeed;

            float earlyDistance = earlyComment.transform.localPosition.x + earlyComment.commentLength - rightLocalX;
            float earlyEndTime = earlyDistance / earlyComment.commentSpeed;

            if (earlyEndTime <= relativeConflictTime)
            {
                return true;
            }
            else
                return false;
        }
    }
}
