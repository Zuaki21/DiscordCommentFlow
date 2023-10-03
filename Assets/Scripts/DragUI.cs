using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragUI : MonoBehaviour, IBeginDragHandler, IDragHandler

{
    public Vector2 startPos;
    public Vector2 mouseStartPos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        //移動したいUIオブジェクトの最初の位置
        startPos = transform.position;
        //ドラッグを開始したときのマウスの位置
        mouseStartPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //ドラッグ中のUIオブジェクトの位置
        transform.position
                    = startPos + (eventData.position - mouseStartPos);
        //(eventData.position - mouseStartPos)は移動量
    }
}

