using UnityEditor;
using UnityEngine;
using AnnulusGames.LucidTools.Editor;
using System;
using System.Linq;

namespace Zuaki.Edit
{
    [CustomEditor(inspectedType: typeof(Zuaki.SpeakerData))]
    public class SpeakerData : LucidEditor
    {
        public override void OnInspectorGUI()
        {
            LucidEditorGUILayout.BeginBoxGroup("ロールごとのボイス設定");
            {
                //enum speakerをforeach文で回す
                foreach (SpeakerRole speakerRole in Enum.GetValues(typeof(SpeakerRole)))
                {
                    LucidEditorGUILayout.BeginHorizontal(GUI.skin.box);
                    {
                        GUIStyle myStyle = new GUIStyle
                        {
                            fontSize = 13,
                            normal = new GUIStyleState
                            {
                                textColor = Color.white
                            }
                        };
                        LucidEditorGUILayout.LabelField(speakerRole.ToString(), myStyle, GUILayout.MinWidth(90));

                        if (Zuaki.VoiceCharacterData.VoiceCharacters != null &&
                        Zuaki.VoiceCharacterData.VoiceCharacters.Length > 0)
                        {
                            string[] speaker = Zuaki.VoiceCharacterData.VoiceCharacters
                                .Select(x => x.name).ToArray();

                            speakerRole.GetSetting().characterNum =
                                LucidEditorGUILayout.Popup(speakerRole.GetSetting().characterNum, speaker, GUILayout.Width(130));

                            string[] style = Zuaki.VoiceCharacterData.VoiceCharacters[speakerRole.GetSetting().characterNum].styles.Select(x => x.name).ToArray();

                            speakerRole.GetSetting().styleNum =
                                LucidEditorGUILayout.Popup(speakerRole.GetSetting().styleNum, style, GUILayout.Width(80));
                            LucidEditorGUILayout.LabelField("ID:" + speakerRole.GetSetting().speakerID.ToString(), myStyle, GUILayout.Width(30));
                        }
                        else
                        {
                            LucidEditorGUILayout.LabelField("話者のデータがありません", myStyle, GUILayout.Width(130));
                        }
                    }
                    LucidEditorGUILayout.EndHorizontal();
                }
                LucidEditorGUILayout.EndBoxGroup();
                base.OnInspectorGUI();
            }
        }
    }
    public static class SpeakerRoleEx
    {
        public static RoleSetting GetSetting(this SpeakerRole speaker)
        => Zuaki.SpeakerData.GetRoleSetting(speaker);

    }
}
