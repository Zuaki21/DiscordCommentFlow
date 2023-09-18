using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using DG.Tweening;
using UnityEngine.Events;

public enum VolumeType { Master, BGM, SE }
//[RequireComponent(typeof(AudioSource))]
public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    public AudioMixer AudioMixer => PathDataScriptableObject.AudioMixer;
    public static float BGMTime => Instance.audioSourceBGM.time;
    public static float BGMLength
    {
        get
        {
            if (Instance.audioSourceBGM.clip == null)
                return 0;
            else
                return Instance.audioSourceBGM.clip.length;
        }
    }
    private SceneBGM[] SceneBGMs => SceneBGMDataScriptableObject.SceneBGMs;
    public static void SetVolume(float volume, VolumeType type) => Instance._SetVolume(volume, type);
    public static float GetVolume(VolumeType type) => Instance._GetVolume(type);
    public static void SetTime(float time) => Instance._SetTime(time);
    public static void PlaySE(AudioClip clip) => Instance._PlaySE(clip);
    public static void PlayBGM(AudioClip clip) => Instance._PlayBGM(clip);
    public static void PlayBGM(AudioClip clip, float volume) => Instance._PlayBGM(clip, volume);
    public static void PauseBGM() => Instance._PauseBGM();
    public static void UnPauseBGM() => Instance._UnPauseBGM();
    public static void StopBGM() => Instance._StopBGM();
    public static void RePlayBGM() => Instance._RePlayBGM();

    /// <summary>
    /// 指定のAudioClipを指定の秒数によってフェードイン、フェードアウトさせて再生する
    /// IsLoopをtrueにするとフェードアウト後、再度フェードインしてループする
    /// StopBGM()を呼べば瞬間的に停止できる
    /// /// </summary>
    /// <param name="clip">再生するBGM</param>
    /// <param name="beginDuration">フェードインにかける秒数</param>
    /// <param name="volume">再生する音量</param>
    /// <param name="beginSec">再生開始位置(秒数)</param>
    /// <param name="playSec">再生する長さ(秒数)</param>
    /// <param name="endDuration">フェードアウトにかける秒数</param>
    /// <param name="isLoop">ループするかどうか</param>
    public static Sequence FadeInOut(AudioClip clip, float beginDuration = 0f, float endDuration = 0f, float volume = 0.5f, float playSec = 0, bool isLoop = false, float beginSec = 0f) => Instance._FadeInOut(clip, beginDuration, endDuration, volume, playSec, isLoop, beginSec);
    public static Sequence FadeOut(float endDuration = 0f) => Instance._FadeOut(endDuration);
    Sequence fadeSequence;

    private AudioSource audioSourceSE;
    private AudioSource audioSourceBGM;
    private AudioSource sub_audioSourceBGM;

    public static float MasterVolume = 0;
    public static float BGMVolume = 0;
    public static float SEVolume = 0;
    private bool isPausingIntro = false;
    override protected void Awake()
    {
        base.Awake();
        //DontDestroyOnLoad(gameObject);
        audioSourceSE = gameObject.AddComponent<AudioSource>();//SE用のAudioSource
        audioSourceBGM = gameObject.AddComponent<AudioSource>();//BGM用のAudioSource
        sub_audioSourceBGM = gameObject.AddComponent<AudioSource>();//イントロBGM用のAudioSource
    }

    protected void Start()
    {
        //AudioMixerGroupにBGMとSEがあるかを確認する
        if (AudioMixer.FindMatchingGroups("BGM").Length == 0)
        {
            Debug.LogError("BGM用のAudioMixerGroupがありません");
        }
        if (AudioMixer.FindMatchingGroups("SE").Length == 0)
        {
            Debug.LogError("SE用のAudioMixerGroupがありません");
        }

        audioSourceSE.outputAudioMixerGroup = AudioMixer.FindMatchingGroups("SE")[0];
        audioSourceSE.playOnAwake = false;
        audioSourceBGM.outputAudioMixerGroup = AudioMixer.FindMatchingGroups("BGM")[0];
        audioSourceBGM.playOnAwake = false;
        sub_audioSourceBGM.outputAudioMixerGroup = AudioMixer.FindMatchingGroups("BGM")[0];
        sub_audioSourceBGM.playOnAwake = false;
        //シーンがロードされるたびに呼び出されるメソッドの設定
        SceneManager.sceneLoaded += SceneLoaded;
        //初回用はSceneManager.sceneLoadedが実行されないためここで呼び出す
        SceneLoaded(SceneManager.GetActiveScene(), 0);
    }
    //////////////////////////
    ///AudioMixerを生成し、Master、BGM、SEの変数を用意する必要がある。

#pragma warning disable IDE1006//IDE1006警告の無効化
    void _SetVolume(float volume, VolumeType type)
    {
        switch (type)
        {
            case VolumeType.Master:
                AudioMixer.SetFloat("MasterVolume", ConvertVolumeToDecibel(volume));
                SEVolume = volume;
                break;
            case VolumeType.BGM:
                AudioMixer.SetFloat("BGMVolume", ConvertVolumeToDecibel(volume));
                MasterVolume = volume;
                break;
            case VolumeType.SE:
                AudioMixer.SetFloat("SEVolume", ConvertVolumeToDecibel(volume));
                BGMVolume = volume;
                break;
        }
    }
    float _GetVolume(VolumeType type)
    {
        switch (type)
        {
            case VolumeType.BGM:
                AudioMixer.GetFloat("BGMVolume", out float BGMVolume);
                return ConvertDecibelToVolume(BGMVolume);
            case VolumeType.SE:
                AudioMixer.GetFloat("SEVolume", out float SEVolume);
                return ConvertDecibelToVolume(SEVolume);
            case VolumeType.Master:
                AudioMixer.GetFloat("MasterVolume", out float MasterVolume);
                return ConvertDecibelToVolume(MasterVolume);
        }
        return 0;
    }
    void _SetTime(float time)
    {
        audioSourceBGM.time = time;
    }
    void _PlaySE(AudioClip clip, float volume = 0.5f)
    {
        audioSourceSE.volume = volume;
        audioSourceSE.PlayOneShot(clip);
    }
    void _PlayBGM(AudioClip clip, float volume = 0.5f, bool isLoop = false, AudioClip introClip = null, UnityAction OnMusicEnd = null)
    {
        if (fadeSequence != null) fadeSequence.Kill();
        if (clip == null)
        {
            audioSourceBGM.Stop();
            return;
        }
        if (audioSourceBGM != null)
        {
            if (audioSourceBGM.clip == clip && audioSourceBGM.isPlaying)
                return;
        }
        else
        {
            return;
        }

        audioSourceBGM.volume = volume;
        audioSourceBGM.clip = clip;
        audioSourceBGM.loop = isLoop;
        if (introClip == null)
        {
            audioSourceBGM.time = 0;
            audioSourceBGM.Play();
            sub_audioSourceBGM.Stop();
        }
        else
        {
            sub_audioSourceBGM.volume = volume;
            sub_audioSourceBGM.clip = introClip;
            sub_audioSourceBGM.volume = volume;
            sub_audioSourceBGM.clip = introClip;
            sub_audioSourceBGM.PlayScheduled(AudioSettings.dspTime);
            audioSourceBGM.time = 0;
            audioSourceBGM.PlayScheduled(AudioSettings.dspTime + introClip.length);
            ///introClip.length == ((float)_introAudioSource.clip.samples / (float)_introAudioSource.clip.frequency)だよね？？
        }

    }


    Sequence _FadeInOut(AudioClip clip, float beginDuration = 0f, float endDuration = 0f, float volume = 0.5f, float playSec = 0, bool isLoop = false, float beginSec = 0f)
    {
        if (fadeSequence != null) fadeSequence.Kill();
        fadeSequence = DOTween.Sequence();
        if (clip == null)
        {
            audioSourceBGM.Stop();
            return fadeSequence;
        }
        // if (audioSourceBGM.clip != clip || !audioSourceBGM.isPlaying)
        {
            audioSourceBGM.Stop();
            audioSourceBGM.volume = 0;
            audioSourceBGM.clip = clip;
            audioSourceBGM.Play();
            audioSourceBGM.time = beginSec;
            if (beginDuration > 0)
            {
                fadeSequence.Append(audioSourceBGM.DOFade(volume, beginDuration));
            }
            else
            {
                audioSourceBGM.volume = volume;
            }
            if (playSec > 0)
            {
                fadeSequence.AppendInterval(playSec);
            }
            else
            {
                fadeSequence.AppendInterval(clip.length);
            }
            if (endDuration > 0)
            {
                fadeSequence.Append(audioSourceBGM.DOFade(0, endDuration));
            }
            else
            {
                audioSourceBGM.volume = 0;
            }
            if (isLoop)
            {
                fadeSequence.OnComplete(() => _FadeInOut(clip, beginDuration, endDuration, volume, playSec, isLoop, beginSec));
            }
        }
        return fadeSequence;
    }
    Sequence _FadeOut(float duration = 0f)
    {
        if (fadeSequence != null) fadeSequence.Kill();
        fadeSequence = DOTween.Sequence().OnComplete(() => audioSourceBGM.Stop());
        if (duration > 0)
        {
            fadeSequence.Append(audioSourceBGM.DOFade(0, duration));
        }
        else
        {
            audioSourceBGM.volume = 0;
        }
        return fadeSequence;
    }

    void _PauseBGM()
    {
        if (fadeSequence != null) fadeSequence.Kill();
        //イントロ中に一時停止したかどうか
        isPausingIntro = sub_audioSourceBGM.isPlaying;

        audioSourceBGM.Pause();
        sub_audioSourceBGM.Pause();
    }
    void _UnPauseBGM()
    {
        if (fadeSequence != null) fadeSequence.Kill();
        if (isPausingIntro)
        {
            sub_audioSourceBGM.UnPause();
            audioSourceBGM.Stop();
            audioSourceBGM.PlayScheduled(AudioSettings.dspTime + sub_audioSourceBGM.clip.length);
        }
        else
        {
            audioSourceBGM.UnPause();
        }
    }
    void _StopBGM()
    {
        if (fadeSequence != null) fadeSequence.Kill();
        audioSourceBGM.Stop();
        sub_audioSourceBGM.Stop();
    }
    void _RePlayBGM()
    {
        if (fadeSequence != null) fadeSequence.Kill();
        if (sub_audioSourceBGM.isPlaying)//イントロ中なら
        {
            sub_audioSourceBGM.Play();
            audioSourceBGM.Stop();
            audioSourceBGM.PlayScheduled(AudioSettings.dspTime + sub_audioSourceBGM.clip.length);
        }
        else//イントロ中じゃないなら
        {
            audioSourceBGM.Play();
        }
    }

    void SceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        var name = nextScene.name;
        foreach (var sceneBGM in SceneBGMs)
        {
            //シーン名と同じ名前のシーンがあったら
            if (sceneBGM.sceneObject.name == name)
            {
                _PlayBGM(sceneBGM.clip, sceneBGM.volume, sceneBGM.isLoop, sceneBGM.introClip);
                break;
            }
        }
    }
#pragma warning restore//警告の解除

    float ConvertVolumeToDecibel(float volume = 0)
    {
        return Mathf.Clamp(Mathf.Log10(volume) * 20, -80, 0);
    }
    float ConvertDecibelToVolume(float decibel)
    {
        return Mathf.Clamp(Mathf.Pow(10, decibel / 20), 0, 1);
    }
}
