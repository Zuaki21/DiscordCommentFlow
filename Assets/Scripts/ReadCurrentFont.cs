using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Zuaki;

public class ReadCurrentFont : MonoBehaviour
{
    TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {

        text = GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        TMP_FontAsset fontAsset = FontData.CurrentFontAsset;
        text.font = fontAsset;
    }
}
