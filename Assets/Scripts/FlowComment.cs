using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;
using TMPro;

namespace Zuaki
{
    public class FlowComment : MonoBehaviour
    {
        Rigidbody2D rb;
        TextMeshProUGUI textMesh;
        float leftLocalX = -650;
        public float commentLength => textMesh.text.Length * textMesh.fontSize;
        public float commentSpeed => Settings.Instance.flowSpeed * (commentLength + 1300) / 1300;
        public float fixedSpeedParam = 1;
        protected void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            rb = GetComponent<Rigidbody2D>();
        }
        protected void Start()
        {
            textMesh.fontSize = Settings.Instance.fontSize;
            rb.velocity = new Vector2(-commentSpeed * fixedSpeedParam, 0);
        }
        protected void Update()
        {
            //画面外に出たら消す
            if (transform.localPosition.x < leftLocalX - commentLength)
            {
                //コメントリストから削除
                ChatManager.Instance.RemoveFlowChatList(this);
                Destroy(gameObject);
            }
        }
    }
}
