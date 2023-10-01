using UnityEditor;
using UnityEngine;
using Zuaki;
using AnnulusGames.LucidTools.Editor;

namespace Zuaki.Edit
{
    [CustomEditor(typeof(Zuaki.VoiceCharacterData))]
    public class VoiceCharacterData : LucidEditor
    {
        // Implement this function to make a custom inspector.
        public override void OnInspectorGUI()
        {
            LucidEditorGUILayout.BeginVertical();
            {
                LucidEditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    GUILayout.Space(EditorGUIUtility.singleLineHeight / 2);
                    GUIStyle myStyle = new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 20,
                        normal = new GUIStyleState
                        {
                            textColor = Color.white
                        }
                    };
                    LucidEditorGUILayout.LabelField("VOICEVOXの音声一覧", myStyle);
                }
                LucidEditorGUILayout.EndHorizontal();

                if (Zuaki.VoiceCharacterData.VoiceCharacters != null &&
                    Zuaki.VoiceCharacterData.VoiceCharacters.Length > 0)
                    foreach (var character in Zuaki.VoiceCharacterData.VoiceCharacters)
                    {

                        LucidEditorGUILayout.BeginHorizontal(GUI.skin.box);
                        {
                            LucidEditorGUILayout.BeginVertical();
                            {
                                GUILayout.FlexibleSpace();
                                GUIStyle nameStyle = new GUIStyle
                                {
                                    alignment = TextAnchor.MiddleCenter,
                                    fontSize = 15,
                                    normal = new GUIStyleState
                                    {
                                        textColor = Color.white
                                    }
                                };
                                LucidEditorGUILayout.LabelField(character.name, nameStyle, GUILayout.MinWidth(120));
                                GUILayout.FlexibleSpace();
                            }
                            LucidEditorGUILayout.EndVertical();


                            LucidEditorGUILayout.BeginVertical(GUI.skin.box);
                            {
                                foreach (var style in character.styles)
                                {
                                    LucidEditorGUILayout.LabelField(style.name, GUILayout.MinWidth(50));
                                }
                            }
                            LucidEditorGUILayout.EndVertical();

                            LucidEditorGUILayout.BeginVertical();
                            {
                                GUIStyle rightAlignStyle = new GUIStyle
                                {
                                    alignment = TextAnchor.MiddleRight,
                                    normal = new GUIStyleState
                                    {
                                        textColor = Color.white
                                    }
                                };
                                foreach (var style in character.styles)
                                {
                                    LucidEditorGUILayout.LabelField(style.id.ToString(), rightAlignStyle, GUILayout.Width(10));
                                }
                            }
                            LucidEditorGUILayout.EndVertical();
                        }
                        LucidEditorGUILayout.EndHorizontal();
                    }
                else
                {
                    LucidEditorGUILayout.BeginHorizontal(GUI.skin.box);
                    {
                        GUIStyle centerStyle = new GUIStyle
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 13,
                            normal = new GUIStyleState
                            {
                                textColor = Color.white
                            }
                        };
                        LucidEditorGUILayout.LabelField("話者のデータがありません", centerStyle);
                    }
                    LucidEditorGUILayout.EndHorizontal();
                }

            }
            LucidEditorGUILayout.EndVertical();
            base.OnInspectorGUI();

        }
    }
}
