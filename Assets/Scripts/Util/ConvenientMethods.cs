using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ConvenientMethods : MonoBehaviour
{
    #region Vector3
    public static Vector3 RandomPositionInBox(Vector3 startPosition, Vector3 endPosition)
    {
        if (startPosition.x > endPosition.x)
        {
            SwapValue(ref startPosition.x, ref endPosition.x);
        }
        if (startPosition.y > endPosition.y)
        {
            SwapValue(ref startPosition.y, ref endPosition.y);
        }
        if (startPosition.z > endPosition.z)
        {
            SwapValue(ref startPosition.z, ref endPosition.z);
        }
        return new Vector3(Random.Range(startPosition.x, endPosition.x), Random.Range(startPosition.y, endPosition.y), Random.Range(startPosition.z, endPosition.z));
    }
    #endregion

    public static void SwapValue(ref float a, ref float b)
    {
        float temp = a;
        a = b;
        b = temp;
    }

    #region Debug
    public static void DebugLogInEditor(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj);
#else
            if (Debug.isDebugBuild) Debug.Log(obj);
#endif
    }
    public static void DebugLogErrorInEditor(object obj)
    {
#if UNITY_EDITOR
        Debug.LogError(obj);
#else
            if (Debug.isDebugBuild) Debug.LogError(obj);
#endif
    }
    #endregion
}
