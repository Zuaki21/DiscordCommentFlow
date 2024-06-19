using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MouseInWindow : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    Tween tween;
    bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }
    bool IsMouseOverGameWindowold = false;
    void Update()
    {
        if (IsMouseOverGameWindow)
        {
            if (!IsMouseOverGameWindowold)
            {
                EnterScreen();
            }
            IsMouseOverGameWindowold = true;
        }
        else
        {
            if (IsMouseOverGameWindowold)
            {
                ExitScreen();
            }
            IsMouseOverGameWindowold = false;
        }
    }

    void EnterScreen()
    {
        if (tween != null)
        {
            tween.Kill();
        }
        tween = canvasGroup.DOFade(1, 0.5f);
    }

    void ExitScreen()
    {
        if (tween != null)
        {
            tween.Kill();
        }
        tween = canvasGroup.DOFade(0, 0.5f).SetDelay(0.5f);
    }
}
