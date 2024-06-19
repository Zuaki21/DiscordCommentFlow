using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using NUnit.Framework.Internal;
using Cysharp.Threading.Tasks;

namespace Zuaki
{
    public class FlowComment : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        Rigidbody2D rb;
        TextMeshProUGUI textMeshPro;
        float leftLocalX = -650;
        public float commentLength => textMeshPro.preferredWidth + 5;
        public float commentUpperHeights => textMeshPro.bounds.extents.y + textMeshPro.bounds.center.y;
        public float commentLowerHeights => textMeshPro.bounds.extents.y - textMeshPro.bounds.center.y;
        public float commentSpeed => SettingManager.Settings.flowSpeed * (commentLength + 1300) / 1300;
        public float fixedSpeedParam = 1;
        public bool isPointEnter = false;
        private Vector2 offset; // ドラッグ中のオフセット
        Tween tween;
        protected void Awake()
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
            textMeshPro.fontSize = SettingManager.Settings.fontSize;
            textMeshPro.font = FontData.CurrentFontAsset;
            rb = GetComponent<Rigidbody2D>();
        }
        protected void Start()
        {
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
        public void OnPointerDown(PointerEventData eventData)
        {
            rb.velocity = Vector2.zero;
            // クリックした位置とコメントの位置の差を記憶
            offset = transform.position - Camera.main.ScreenToWorldPoint(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // マウスの位置をワールド座標に変換してコメントの位置を更新
            Vector2 newPosition = (Vector2)Camera.main.ScreenToWorldPoint(eventData.position) + offset;
            Vector2 newLocalPosition = transform.parent.InverseTransformPoint(newPosition);

            //Z座標を0のままにするためにlocalPositionを使う
            transform.localPosition = newLocalPosition;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            rb.velocity = new Vector2(-commentSpeed * fixedSpeedParam, 0);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tween != null) tween.Kill();
            //1.2倍の大きさにする
            tween = transform.DOScale(1.05f, 0.1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //ドラッグ中は処理しない
            if (eventData.dragging == true) return;
            if (tween != null) tween.Kill();
            //元の大きさに戻す
            tween = transform.DOScale(1f, 0.1f);
        }
    }
}
