using System;
using System.Reflection;
using TMPro;
using UnityEngine;

public class FixTMProTagLength : MonoBehaviour
{
    private const int RICH_TEXT_TAG_LENGTH_OVERRIDE = 1024;
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod]
#endif
    static void TryFixTMProTagLength()
    {
        // Fixing TMPro to have a bigger tag length. Currently it's just 128 which is peanuts - links can be very long.
        // More specifically, the link tag breaks if the text inside the tag is longer than 128 chars, eg:
        // For <link="http://..">
        // <whatever_is_here_has_to_be_shorter_than_128_chars>
        var fi = typeof(TMP_Text).GetField("m_htmlTag", BindingFlags.NonPublic | BindingFlags.Static);
        var arr = fi.GetValue(null) as char[];
        if (arr.Length < RICH_TEXT_TAG_LENGTH_OVERRIDE)
        {
            Array.Resize(ref arr, RICH_TEXT_TAG_LENGTH_OVERRIDE);
            fi.SetValue(null, arr);
        }
    }
}
