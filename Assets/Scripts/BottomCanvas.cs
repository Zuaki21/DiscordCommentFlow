using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;

namespace Zuaki
{
    public class BottomCanvas : MonoBehaviour
    {
        [SerializeField] GameObject SettingPanel;
        [SerializeField] GameObject LogPanel;
        public void OpenSetting()
        {
            SettingPanel.SetActive(true);
        }
        public void OpenLogPanel()
        {
            LogPanel.SetActive(!LogPanel.activeSelf);
        }
    }
}
