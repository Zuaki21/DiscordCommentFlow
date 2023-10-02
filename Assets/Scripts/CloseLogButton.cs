using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;
using UnityEngine.UI;

namespace Zuaki
{
    public class CloseLogButton : MonoBehaviour
    {
        [SerializeField] GameObject LogPanel;
        void Awake()
        {
            Button button = GetComponent<Button>();
            button.onClick.AddListener(CloseLog);
        }
        public void CloseLog()
        {
            LogPanel.SetActive(false);
        }
    }
}
