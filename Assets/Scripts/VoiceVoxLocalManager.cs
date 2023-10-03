//参考
//https://lily00.hatenablog.jp/entry/2023/06/08/200913

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;
using Cysharp.Threading.Tasks;
using System.Text;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

namespace Zuaki
{
    public class VoiceVoxLocalManager
    {
        private AudioClip _audioClip; //プライベートフィールドを定義
        private string baseURL = "http://localhost:50021/";  //VoiceBoxサーバのベースURL
        public AudioClip AudioClip { get => _audioClip; } //プライベートフィールド_audioClipの値を返すプロパティ

        private async UniTask<byte[]> GetQuery(string text, int speakerId) // VoiceVoxサーバにテキストを投げて、音声合成用のクエリを作成してもらう
        {
            string EncodedUrl = UnityWebRequest.EscapeURL(text, Encoding.UTF8); //音声に変換したいテキストをURLエンコード
            string url = baseURL + $"audio_query?text={EncodedUrl}&speaker={speakerId}"; // 問い合わせ先URLを作成
            using (UnityWebRequest request = new UnityWebRequest(url, "POST")) //urlとPOSTを指定してリクエストを作成
            {
                request.downloadHandler = new DownloadHandlerBuffer(); //そのままの形式でダウンロード
                await request.SendWebRequest();//WEBサーバにリクエストを送信する

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"エラーです。{request.error}");
                    return null;
                }
                else
                {
                    return request.downloadHandler.data; //返ってきたデータ（クエリ）をそのまま戻り値として返す
                }
            }
        }

        public async UniTask DownloadAudioClip(string text, int speakerId) //クエリを投げて音声合成してもらい、AudioClipを受け取る関数
        {
            byte[] query = await GetQuery(text, speakerId);//音声合成用のクエリを作成する
            query = SetOption(query, SpeakerData.SpeakerOption);//デフォルトのオプションを設定する

            string Url = baseURL + $"synthesis?speaker={speakerId}";// 問い合わせ先URLを作成
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(Url, AudioType.WAV))//WAV形式のデータを要求するリクエストを作成
            {
                request.method = "POST";//POSTメソッドを指定
                request.SetRequestHeader("Content-Type", "application/json");//ヘッダーを設定
                request.uploadHandler = new UploadHandlerRaw(query);//BODYにクエリを設定
                await request.SendWebRequest();//WEBサーバにリクエストを送信する

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"エラーです。{request.error}");
                }
                else
                {
                    _audioClip = DownloadHandlerAudioClip.GetContent(request); //AudioClip形式でデータを受け取る
                }
            }
        }

        byte[] SetOption(byte[] query, SpeakerOption option)
        {
            string querystring = Encoding.UTF8.GetString(query); //クエリを文字列に変換してデバッグログに出力
            JObject jsonObject = JObject.Parse(querystring);
            jsonObject["speedScale"] = SpeakerData.SpeakerOption.speed;
            jsonObject["pitchScale"] = SpeakerData.SpeakerOption.pitch;
            jsonObject["intonationScale"] = SpeakerData.SpeakerOption.intonationScale;
            querystring = jsonObject.ToString();
            return Encoding.UTF8.GetBytes(querystring);
        }
    }
}
