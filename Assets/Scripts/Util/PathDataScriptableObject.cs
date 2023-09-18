using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "PathDataScriptableObject", menuName = "Zuaki/PathDataScriptableObject", order = 100)]
public class PathDataScriptableObject : SingletonScriptableObject<PathDataScriptableObject>
{
    /// <summary>AudioMixer参照</summary>
    public static AudioMixer AudioMixer => Instance.audioMixer;
    [SerializeField] private AudioMixer audioMixer;
}