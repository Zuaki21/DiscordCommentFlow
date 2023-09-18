using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ScreenManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UpdateManager.Subscribe(_ => ManagedUpdate()).AddTo(gameObject);
    }
    void ManagedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (Screen.height == 1080)
            {
                //画面サイズを変更する
                Screen.SetResolution(1280, 720, Screen.fullScreen);
            }
            else
            {
                //画面サイズを変更する
                Screen.SetResolution(1920, 1080, Screen.fullScreen);
            }
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            //フルスクリーン
            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }
            else
            {
                Screen.fullScreen = true;
            }
        }
    }

}
