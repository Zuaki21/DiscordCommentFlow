using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor.PackageManager.Requests;
#endif
using UnityEngine;
using UnityEngine.Networking;
using Zuaki;

namespace Zuaki
{
    public class VoiceVoxWebManager
    {
        static public string uri = "https://deprecatedapis.tts.quest/v2/voicevox/audio/";
        public static int APIkeyIndex = 0;
        public static async UniTask<AudioClip> PostVoiceVoxWebRequest(string text, int? speakerID = null, float? speechSpeed = null, float? pitch = null, float? intonationScale = null)
        {
            SpeechOption option = new SpeechOption(speakerID, speechSpeed, pitch, intonationScale);
            return await PostVoiceVoxWebRequest(text, option);
        }
        public static async UniTask<AudioClip> PostVoiceVoxWebRequest(string text, SpeakerRole commenter, float? speechSpeed = null, float? pitch = null, float? intonationScale = null)
        => await PostVoiceVoxWebRequest(text, SpeakerData.GetSpeakerID(commenter), speechSpeed, pitch, intonationScale);

        public static async UniTask<AudioClip> PostVoiceVoxWebRequest(ChatElement chatElement, float? speechSpeed = null, float? pitch = null, float? intonationScale = null)
        => await PostVoiceVoxWebRequest(chatElement.Message, chatElement.Commenter, speechSpeed, pitch, intonationScale);

        /// <summary>
        /// Web版VOICEVOXを使って音声を生成する
        /// </summary>
        /// <param name="text">読み上げる内容</param>
        /// <param name="option">オプション</param>
        /// <returns>音声データ</returns>
        public static async UniTask<AudioClip> PostVoiceVoxWebRequest(string text, SpeechOption option = null)
        {
            AudioClip audioClip = null;

            // フォームデータを作成
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("text", text),
                new MultipartFormDataSection("key", Settings.VOICEVOX_WebAPI[APIkeyIndex])
            };

            // オプションがないならデフォルト値を使う
            if (option == null) option = new SpeechOption();

            float speed = option.speechSpeed;

            string parameter = $"?speaker={option.speakerID}&speed={speed}&pitch={option.pitch}&intonationScale={option.intonationScale}";

            // POSTリクエストを作成
            using UnityWebRequest request = UnityWebRequest.Post(uri + parameter, formData);

            try
            {
                await request.SendWebRequest(); // リクエスト送信
            }
            catch (UnityWebRequestException e) when (e.ResponseCode == 400)
            {
                Debug.LogError("読めない文字が含まれているか、文字列が長すぎる可能性があります");
                return null;
            }
            catch (UnityWebRequestException e) when (e.Message.Contains("notEnoughPoints"))
            {
                Debug.LogWarning("WEB版VOICEVOXのAPIのポイントが足りません");
                Debug.Log("APIキーを変更します");
                APIkeyIndex++;
                if (APIkeyIndex >= Settings.VOICEVOX_WebAPI.Length)
                {
                    Debug.LogError("APIキーがすべて使われました");
                    Debug.LogError("WEB版VOICEVOXのAPIのポイントが足りません");
                    Debug.Log("Web版VOICEVOXを使わない設定にします");
                    Settings.Instance.useLocalVoiceVox = true;
                    SettingOperator.SetVoiceVoxType();
                    Settings.Instance.useVoiceVox = false;
                    SettingOperator.SetUseVoiceVox();
                    APIkeyIndex = 0;
                    return null;
                }
                return await PostVoiceVoxWebRequest(text, option);
            }
            catch (UnityWebRequestException e) when (e.ResponseCode == 429)
            {
                Debug.LogError("WEB版VOICEVOXへのリクエストが多すぎます");
                return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }

            // 音声データを取得
            byte[] results = request.downloadHandler.data;
            // AudioClipに変換
            audioClip = ToAudioClip(results);
            return audioClip;
        }

        /// <summary>
        /// バイナリデータをAudioClipに変換する
        /// </summary>
        /// <param name="data">バイナリデータ</param>
        /// <returns>AudioClip</returns>
        public static AudioClip ToAudioClip(byte[] data)
        {
            // ヘッダー解析
            int channels = data[22];
            int frequency = BitConverter.ToInt32(data, 24);
            int length = data.Length - 44;
            float[] samples = new float[length / 2];

            // 波形データ解析
            for (int i = 0; i < length / 2; i++)
            {
                short value = BitConverter.ToInt16(data, i * 2 + 44);
                samples[i] = value / 32768f;
            }

            // AudioClipを作成
            AudioClip audioClip = AudioClip.Create("AudioClip", samples.Length, channels, frequency, false);
            audioClip.SetData(samples, 0);

            return audioClip;
        }
    }

    [System.Serializable]
    /// <summary>
    /// 読み上げのオプション
    /// </summary>
    public class SpeechOption
    {
        public int speakerID { get; private set; } = 3;
        public float speechSpeed { get; private set; } = 1;
        public float pitch { get; private set; } = 0;
        public float intonationScale { get; private set; } = 1;


        /// <summary>
        /// オプションを設定する
        /// </summary>
        /// <param name="speakerID">話者ID</param>
        /// <param name="speechSpeed">スピード</param>
        /// <param name="pitch">ピッチ(±0.1範囲が良い)</param>
        /// <param name="intonationScale">抑揚(0-5)</param>
        public SpeechOption(int? speakerID = null, float? speechSpeed = null, float? pitch = null, float? intonationScale = null)
        {
            this.speakerID = speakerID ?? SpeakerData.GetSpeakerID(SpeakerRole.Other);
            this.speechSpeed = speechSpeed ?? SpeakerData.DefaultOption.speed;
            this.pitch = pitch ?? SpeakerData.DefaultOption.pitch;
            this.intonationScale = intonationScale ?? SpeakerData.DefaultOption.intonationScale;
        }
    }
}
