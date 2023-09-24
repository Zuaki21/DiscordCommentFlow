using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zuaki;

namespace Zuaki
{
    public class VoiceSpeaker : SingletonMonoBehaviour<VoiceSpeaker>
    {
        public static List<ChatElement> ChatElementList => Instance.chatElementList;
        List<ChatElement> chatElementList = new List<ChatElement>();

        public static List<AudioClip> ClipList => Instance.clipList;
        List<AudioClip> clipList = new List<AudioClip>();

        protected void Start()
        {
            PostVoiceVoxRequest();
        }

        async void PostVoiceVoxRequest()
        {
            while (true)
            {
                if (chatElementList.Count > 0 && ClipList.Count < 2)
                {
                    CombineComment();// 同時に読み上げられそうなら結合する
                    AudioClip newClip = await VoiceVoxWebManager.PostVoiceVoxWebRequest(chatElementList[0]);
                    if (newClip != null) ClipList.Add(newClip);
                    chatElementList.RemoveAt(0);
                    await UniTask.Delay(500);
                }
                await UniTask.DelayFrame(1);
            }
        }
        // 同時に読み上げられそうなら同時に読み上げる
        // 読み上げ対象が同じRoleVoiceの場合に結合する
        static void CombineComment()
        {
            while (ChatElementList.Count >= 2)
            {
                if (ChatElementList[0].SpeakerID == ChatElementList[1].SpeakerID)
                {
                    string newMessage = $"{ChatElementList[0].Message} {ChatElementList[1].Message}";

                    var ChatElement = new ChatElement(newMessage, commenter: ChatElementList[0].Commenter);
                    ChatElementList.RemoveAt(0);
                    ChatElementList.RemoveAt(0);
                    ChatElementList.Insert(0, ChatElement);
                }
                else
                {
                    break;
                }
            }
        }
        public static void AddComment(ChatElement[] newChatElements)
        {
            // GPTのコメントを読み上げない設定の場合は読み上げない
            if (newChatElements[0].Commenter == Commenter.GPT && Settings.Instance.useVoiceOnGPT == false) return;

            foreach (ChatElement newChatElement in newChatElements)
            {
                string message = RemoveSpaces(newChatElement.Message);

                if (Settings.Instance.useNameOnVoice && newChatElement.Name != null && newChatElement.Commenter != Commenter.GPT)
                {
                    string name = RemoveSpaces(newChatElement.Name);
                    message = $"{name}さん{message}";
                }
                ChatElement fixedChatElement = new ChatElement(message, commenter: newChatElement.Commenter);
                ChatElementList.Add(fixedChatElement);
            }
        }
        static string RemoveSpaces(string input)
        {
            // 半角スペースと全角スペースを削除して返す
            return input.Replace(" ", "").Replace("　", "");
        }

        protected void Update()
        {
            if (SoundManager.IsPlayingBGM == false)
            {
                PlayCommentClip();
            }
        }
        void PlayCommentClip()
        {
            if (clipList.Count > 0)
            {
                CombineAudioClip();
                SoundManager.PlayBGM(clipList[0]);
                clipList.RemoveAt(0);
            }
        }

        void CombineAudioClip()
        {
            while (ClipList.Count >= 2)
            {
                AudioClip newClip = AudioUtils.Combine(ClipList[0], ClipList[1]);
                ClipList.RemoveAt(0);
                ClipList.RemoveAt(0);
                ClipList.Insert(0, newClip);
            }
        }


    }
    public static class AudioUtils
    {
        public static AudioClip Combine(AudioClip clip_A, AudioClip clip_B)
        {
            if (clip_A == null || clip_B == null)
            {
                Debug.LogError("Both clips must be valid AudioClip objects.");
                return null;
            }

            // 結合後の音声データの長さを計算します
            int combinedLength = clip_A.samples + clip_B.samples;

            // 新しい AudioClip を作成します
            AudioClip newClip = AudioClip.Create("CombinedClip", combinedLength, clip_A.channels, clip_A.frequency, false);

            // AudioClip のデータを取得します
            float[] data_A = new float[clip_A.samples * clip_A.channels];
            float[] data_B = new float[clip_B.samples * clip_B.channels];
            clip_A.GetData(data_A, 0);
            clip_B.GetData(data_B, 0);

            // 新しい AudioClip にデータをコピーします
            float[] combinedData = new float[combinedLength * clip_A.channels];
            for (int i = 0; i < clip_A.samples; i++)
            {
                for (int channel = 0; channel < clip_A.channels; channel++)
                {
                    combinedData[i * clip_A.channels + channel] = data_A[i * clip_A.channels + channel];
                }
            }
            for (int i = 0; i < clip_B.samples; i++)
            {
                for (int channel = 0; channel < clip_B.channels; channel++)
                {
                    combinedData[(i + clip_A.samples) * clip_B.channels + channel] = data_B[i * clip_B.channels + channel];
                }
            }

            // 新しい AudioClip にデータをセットします
            newClip.SetData(combinedData, 0);

            return newClip;
        }

    }

}
