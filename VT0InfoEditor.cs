using UnityEngine;
using UnityEditor;
using static UnityEngine.GUILayout;
using E = UnityEditor.EditorGUILayout;

namespace VT0
{
    [CustomEditor(typeof(VT0Info))]
    public class VT0InfoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            var channels = serializedObject.FindProperty(nameof(VT0Info.Channels));

            for (int j = 0; j < channels.arraySize; j++)
            {
                var channel = channels.GetArrayElementAtIndex(j);
                BeginVertical("HelpBox");
                BeginHorizontal();
                Label("VT0 Channel", EditorStyles.largeLabel);
                var removeChannel = Button("x", Width(20f));
                EndHorizontal();

                BeginHorizontal();
                Label("Format");
                var format = channel.FindPropertyRelative(nameof(VT0Channel.Format));
                E.PropertyField(format, GUIContent.none, MaxWidth(120f));
                // TODO: Platform-specific formats
                if (Button("Opaque")) {
                    format.intValue = (int)TextureFormat.DXT1;
                }
                if (Button("Transparent")) {
                    format.intValue = (int)TextureFormat.DXT5;
                }
                if (Button("Alpha")) {
                    format.intValue = (int)TextureFormat.Alpha8;
                }
                EndHorizontal();

                var textureNames = channel.FindPropertyRelative(nameof(VT0Channel.TextureNames));
                BeginHorizontal();
                if (Button("+", Width(20f))) {
                    textureNames.arraySize++;
                }
                Label("Property names:", EditorStyles.miniLabel);
                EndHorizontal();
                
                for (int i = 0; i < textureNames.arraySize; i++)
                {
                    BeginHorizontal();
                    var removeMe = Button("x", Width(20f));
                    E.PropertyField(textureNames.GetArrayElementAtIndex(i), GUIContent.none);
                    EndHorizontal();
                    if (removeMe) {
                        textureNames.DeleteArrayElementAtIndex(i--);
                    }
                }

                E.Space();
                EndVertical();

                if (removeChannel) {
                    channels.DeleteArrayElementAtIndex(j--);
                }
            }

            BeginHorizontal();
            FlexibleSpace();
            if (Button("New channel")) {
                channels.arraySize++;
            }
            FlexibleSpace();
            EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}