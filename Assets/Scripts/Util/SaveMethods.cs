using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Zuaki
{
    public enum SaveType
    {
        PlayerPrefs,//PlayerPrefsで保存(暗号化なし)
        SecurePlayerPrefs,//PlayerPrefsで保存(暗号化あり)
        Json,//Jsonファイルで保存(暗号化なし)
        Binary,//Binaryファイルで保存(暗号化あり)
    }
    public class SaveMethods
    {
#if UNITY_WEBGL
        static SaveType saveType = SaveType.SecurePlayerPrefs;
#elif UNITY_EDITOR
        static SaveType saveType => SaveType.PlayerPrefs;
#else
        static SaveType saveType = SaveType.Binary;
#endif

        //保存
        public static void Save(object obj, string dataName)
        {
            switch (saveType)
            {
                case SaveType.PlayerPrefs:
                    SavePlayerPrefs(obj, dataName);
                    break;
                case SaveType.SecurePlayerPrefs:
                    SaveSecurePlayerPrefs(obj, dataName);
                    break;
                case SaveType.Json:
                    SaveJson(obj, dataName);
                    break;
                case SaveType.Binary:
                    SaveBinary(obj, dataName);
                    break;
            }
        }
        public static void SavePlayerPrefs(object obj, string dataName)
        {
            string json = JsonUtility.ToJson(obj);
            PlayerPrefs.SetString(dataName + "_Json", json);
            PlayerPrefs.Save();
        }
        public static void SaveSecurePlayerPrefs(object obj, string dataName)
        {
            string json = JsonUtility.ToJson(obj);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            byte[] arrEncrypted = AesEncrypt(bytes);
            PlayerPrefs.SetString(dataName + "_SecureJson", BitConverter.ToString(arrEncrypted));
            PlayerPrefs.Save();
        }
        public static void SaveJson(object obj, string dataName)
        {
            string json = JsonUtility.ToJson(obj, true);
            string path = Path.Combine(Application.persistentDataPath, dataName + ".json");
            File.WriteAllText(path, json);
        }
        public static void SaveBinary(object obj, string dataName)
        {
            string json = JsonUtility.ToJson(obj);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            byte[] arrEncrypted = AesEncrypt(bytes);
            string path = Path.Combine(Application.persistentDataPath, dataName + ".bin");
            File.WriteAllBytes(path, arrEncrypted);
        }

        //読み込み
        public static void Load(object obj, string dataName)
        {
            switch (saveType)
            {
                case SaveType.PlayerPrefs:
                    LoadPlayerPrefs(obj, dataName);
                    break;
                case SaveType.SecurePlayerPrefs:
                    LoadSecurePlayerPrefs(obj, dataName);
                    break;
                case SaveType.Json:
                    LoadJson(obj, dataName);
                    break;
                case SaveType.Binary:
                    LoadBinary(obj, dataName);
                    break;
            }
        }

        public static void LoadPlayerPrefs(object obj, string dataName)
        {
            if (PlayerPrefs.HasKey(dataName + "_Json"))
            {
                string json = PlayerPrefs.GetString(dataName + "_Json");
                JsonUtility.FromJsonOverwrite(json, obj);
            }
        }
        public static void LoadSecurePlayerPrefs(object obj, string dataName)
        {
            if (PlayerPrefs.HasKey(dataName + "_SecureJson"))
            {
                byte[] bytes = ConvertToBytes(PlayerPrefs.GetString(dataName + "_SecureJson"));
                byte[] arrDecrypted = AesDecrypt(bytes);
                string json = Encoding.UTF8.GetString(arrDecrypted);
                JsonUtility.FromJsonOverwrite(json, obj);
            }
        }
        public static void LoadJson(object obj, string dataName)
        {
            string path = Path.Combine(Application.persistentDataPath, dataName + ".json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(json, obj);
            }
        }

        public static void LoadBinary(object obj, string dataName)
        {
            string path = Path.Combine(Application.persistentDataPath, dataName + ".bin");
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                byte[] arrDecrypted = AesDecrypt(bytes);
                string json = Encoding.UTF8.GetString(arrDecrypted);
                JsonUtility.FromJsonOverwrite(json: json, obj);
            }
        }


        /// 暗号化部分
        /// (参考)https://qiita.com/InfiniteGame/items/01da9d83853fecb95132
        /// AesManagedマネージャーを取得

        private static AesManaged GetAesManager()
        {
            //任意の半角英数16文字
            string aesIv = "d94j6psqbmqzifj3";
            string aesKey = "bi2o498sgq3gr2e4";

            AesManaged aes = new AesManaged();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.IV = Encoding.UTF8.GetBytes(aesIv);
            aes.Key = Encoding.UTF8.GetBytes(aesKey);
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }
        /// AES暗号化
        public static byte[] AesEncrypt(byte[] byteText)
        {
            // AESマネージャーの取得
            AesManaged aes = GetAesManager();
            // 暗号化
            byte[] encryptText = aes.CreateEncryptor().TransformFinalBlock(byteText, 0, byteText.Length);

            return encryptText;
        }
        /// AES復号化
        public static byte[] AesDecrypt(byte[] byteText)
        {
            // AESマネージャー取得
            var aes = GetAesManager();
            // 復号化
            byte[] decryptText = aes.CreateDecryptor().TransformFinalBlock(byteText, 0, byteText.Length);

            return decryptText;
        }

        /// バイト配列をハイフン区切りの文字列から変換
        public static byte[] ConvertToBytes(string arrayStr)
        {
            // バイト型配列をハイフン(-)で文字列の配列に変換
            string[] arrayStr2 = arrayStr.Split('-');
            byte[] arrayOut = new byte[arrayStr2.Length];
            for (int i = 0; i < arrayStr2.Length; i++)
            {
                // 16進数文字列に変換
                arrayOut[i] = Convert.ToByte(arrayStr2[i], 16);
            }
            return arrayOut;
        }
        public static bool IsExist(string dataName)
        {
            switch (saveType)
            {
                case SaveType.PlayerPrefs:
                    return PlayerPrefs.HasKey(dataName + "_Json");
                case SaveType.SecurePlayerPrefs:
                    return PlayerPrefs.HasKey(dataName + "_SecureJson");
                case SaveType.Json:
                    string path_json = Path.Combine(Application.persistentDataPath, dataName + ".json");
                    return File.Exists(path_json);
                case SaveType.Binary:
                    string path_binary = Path.Combine(Application.persistentDataPath, dataName + ".bin");
                    return File.Exists(path_binary);
            }
            return false;
        }

        public static void Delete(string dataName)
        {
            switch (saveType)
            {
                case SaveType.PlayerPrefs:
                    PlayerPrefs.DeleteKey(dataName + "_Json");
                    break;
                case SaveType.SecurePlayerPrefs:
                    PlayerPrefs.DeleteKey(dataName + "_SecureJson");
                    break;
                case SaveType.Json:
                    string path_json = Path.Combine(Application.persistentDataPath, dataName + ".json");
                    if (File.Exists(path_json))
                    {
                        File.Delete(path_json);
                    }
                    break;
                case SaveType.Binary:
                    string path_binary = Path.Combine(Application.persistentDataPath, dataName + ".bin");
                    if (File.Exists(path_binary))
                    {
                        File.Delete(path_binary);
                    }
                    break;
            }
        }
    }
}
