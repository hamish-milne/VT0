using UnityEngine;
using UnityEditor;
using static UnityEngine.GUILayout;
using E = UnityEditor.EditorGUILayout;

namespace VT0
{
    [CustomEditor(typeof(VT0Info))]
    public class VT0InfoEditor : Editor
    {
        [MenuItem("VT0/Settings")]
        public static void Show()
        {
            Selection.activeObject = VT0Info.Current;
        }

        public static void UpdateChannelFile()
        {
            VT0Info.Current.UpdateChannelFile(System.IO.File.Create("Assets/VT0/VT0_channels.cginc"));
            AssetDatabase.Refresh();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            Label("General", EditorStyles.boldLabel);
            E.PropertyField(serializedObject.FindProperty(nameof(VT0Info.ThumbSize)));
            E.PropertyField(serializedObject.FindProperty(nameof(VT0Info.MemoryCompression)));

            var vramBytes = ((VT0Info)target).EstimateVRAM(100);
            string specifier = "B";
            if (vramBytes > (1 << 30)) {
                vramBytes /= (1 << 30);
                specifier = "GiB";
            } else if (vramBytes > (1 << 20)) {
                vramBytes /= (1 << 20);
                specifier = "MiB";
            } else if (vramBytes > (1 << 10)) {
                vramBytes /= (1 << 10);
                specifier = "KiB";
            }

            E.HelpBox(
                $"This configuration uses approximately {vramBytes:F1} {specifier} of VRAM",
                MessageType.Info);
            
            BeginHorizontal();
            if (serializedObject.FindProperty(nameof(VT0Info.Modified)).boolValue) 
            {
                GUI.color = Color.red;
                E.HelpBox("Settings were modified; apply changes before running", MessageType.None);
                GUI.color = Color.white;
            } else {
                E.HelpBox("No modifications", MessageType.None);
            }
            if (Button("Apply", ExpandWidth(false))) {
                UpdateChannelFile();
                serializedObject.FindProperty(nameof(VT0Info.Modified)).boolValue = false;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            EndHorizontal();

            var channels = serializedObject.FindProperty(nameof(VT0Info.Channels));

            for (int j = 0; j < channels.arraySize; j++)
            {
                var channel = channels.GetArrayElementAtIndex(j);
                BeginVertical("HelpBox");
                BeginHorizontal();
                Label("Channel", EditorStyles.boldLabel);
                var removeChannel = Button("x", Width(20f));
                EndHorizontal();

                E.PropertyField(channel.FindPropertyRelative(nameof(VT0Channel.Count)));

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

            if (serializedObject.ApplyModifiedProperties()) {
                serializedObject.FindProperty(nameof(VT0Info.Modified)).boolValue = true;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}