using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "PathDataScriptableObject", menuName = "Zuaki/PathDataScriptableObject", order = 100)]
public class PathDataScriptableObject : SingletonScriptableObject<PathDataScriptableObject>
{
    /// <summary>AudioMixer参照</summary>
    public static AudioMixer AudioMixer => Instance.audioMixer;
    [SerializeField] private AudioMixer audioMixer;
    public static FontAsset FontAsset => Instance.fontAsset;
    [SerializeField] FontAsset fontAsset;
}

