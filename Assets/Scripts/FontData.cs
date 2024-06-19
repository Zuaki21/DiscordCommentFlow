using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using Zuaki;

namespace Zuaki
{
    [CreateAssetMenu(fileName = "FontData", menuName = "Zuaki/FontData", order = 100)]
    public class FontData : SingletonScriptableObject<FontData>
    {
        public static string CurrentFontName => CurrentFontAsset.name.Replace(" SDF", "");
        public static TMP_FontAsset CurrentFontAsset
        {
            get
            {
                if (Instance.currentFontAsset == null)
                {
                    return Instance.defaultFontAsset;
                }
                return Instance.currentFontAsset;
            }
        }
        private string defaultFontName => defaultFontAsset.name.Replace(oldValue: " SDF", "");
        [SerializeField] private TMP_FontAsset defaultFontAsset;
        [SerializeField] private TMP_FontAsset currentFontAsset;
        //CurrentFontAssetから" SDF"を除外したもの
        [SerializeField] private List<string> fontPaths;
        [SerializeField] Material customFontMaterial;

        [SerializeField] RoleMaterial roleMaterial;
        bool useDefaultFont = true;

        public static Material GetRoleMaterial(Material baseMaterial, SpeakerRole role)
        {

            Material defaultMaterial = Instance.roleMaterial.GetRoleMaterial(role);
            if (SettingManager.Settings.useCustomColor) defaultMaterial = Instance.customFontMaterial;

            // デフォルトフォントを使用する場合はそのまま返す
            if (Instance.useDefaultFont) return defaultMaterial;

            // フォントによってマテリアルの名前を合わせる(フォント名 + 役職名)
            Material material = new Material(baseMaterial)
            {
                name = $"{CurrentFontAsset.name} {role}",
            };

            //Outlineの設定をコピー
            material.SetFloat("_OutlineWidth", defaultMaterial.GetFloat("_OutlineWidth"));
            material.SetColor("_OutlineColor", defaultMaterial.GetColor("_OutlineColor"));
            //Outlineの設定をONにする
            material.EnableKeyword("OUTLINE_ON");
            //FaceのDilateの設定をコピー
            material.SetFloat("_FaceDilate", defaultMaterial.GetFloat("_FaceDilate"));
            return material;
        }

        public static List<string> GetFontNames()
        {
            // streamingAssetsPath
            string dirPath = Application.streamingAssetsPath + "/Fonts";
            //ttfもしくはotfファイルをすべて取得する
            Instance.fontPaths = System.IO.Directory.GetFiles(dirPath, "*.*", System.IO.SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".ttf") || s.EndsWith(".otf")).ToList();

            //パスからフォント名を取得
            return Instance.fontPaths.Select(s => System.IO.Path.GetFileNameWithoutExtension(s)).ToList();
        }

        public static void SetCurrentFont(string fontName)
        {
            if (fontName == Instance.defaultFontName)
            {
                Instance.useDefaultFont = true;
                Instance.currentFontAsset = Instance.defaultFontAsset;
                SettingManager.Settings.fontName = fontName;
                return;
            }

            Instance.useDefaultFont = false;
            string path = GetFontPath(fontName);
            Font osFont = new Font(path);
            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(osFont);
            fontAsset.name = $"{fontName} SDF";
            fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>() { Instance.defaultFontAsset };
            Instance.currentFontAsset = fontAsset;
            SettingManager.Settings.fontName = fontName;
        }

        static string GetFontPath(string fontName)
        {
            string[] fontNames = Instance.fontPaths.Select(s => System.IO.Path.GetFileNameWithoutExtension(s)).ToArray();
            int index = System.Array.IndexOf(fontNames, fontName);

            if (index < 0)
            {
                Debug.LogError("フォントが見つかりませんでした");
                return null;
            }

            return Instance.fontPaths[index];
        }

        public static void Reset()
        {
            Instance.useDefaultFont = true;
            Instance.currentFontAsset = Instance.defaultFontAsset;
        }

        // パスのファイルをStreamingAssets/Fontsにコピーする
        public static void AddNewFont(string path)
        {
            string fileName = System.IO.Path.GetFileName(path);
            string dirPath = Application.streamingAssetsPath + "/Fonts";
            string copyPath = System.IO.Path.Combine(dirPath, fileName);
            System.IO.File.Copy(path, copyPath, true);
        }
        public static void SetCustomOutlineColor()
        {
            Instance.customFontMaterial.SetColor("_OutlineColor", SettingManager.Settings.outtlineColor);
        }
        [System.Serializable]
        public class RoleMaterial
        {
            public Material Programmer;
            public Material Illustrator;
            public Material SoundCreator;
            public Material ScenarioWriter;
            public Material GPT;
            public Material Other;

            public Material GetRoleMaterial(SpeakerRole commenter)
            {
                return commenter switch
                {
                    SpeakerRole.Programmer => Programmer,
                    SpeakerRole.Illustrator => Illustrator,
                    SpeakerRole.SoundCreator => SoundCreator,
                    SpeakerRole.ScenarioWriter => ScenarioWriter,
                    SpeakerRole.GPT => GPT,
                    _ => Other,
                };
            }
        }
    }
}
