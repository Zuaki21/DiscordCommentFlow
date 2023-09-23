using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using Zuaki;

namespace Zuaki
{
    public class VoiceVoxWebManager
    {
        public class VoiceGenerator
        {
            static public string request = "https://deprecatedapis.tts.quest/v2/voicevox/audio/";

            /// <summary>
            /// VOICEVOXを使って音声を生成する
            /// </summary>
            public static async UniTask<AudioClip> GetVoice(string text)
            {
                AudioClip audioClip = null;

                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                formData.Add(new MultipartFormDataSection("text", text));
                formData.Add(new MultipartFormDataSection("key", Settings.Instance.VOICEVOX_WebAPI));
                formData.Add(new MultipartFormDataSection("speaker", Settings.Instance.VOICEVOX_WebAPI));

                UnityWebRequest www = UnityWebRequest.Post("https://www.my-server.com/myform", formData);
                await www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("Form upload complete!");
                }

                return audioClip;
            }
        }
    }
}
