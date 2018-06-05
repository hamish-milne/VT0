#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Text.RegularExpressions;
using UnityEditor.Build.Reporting;

namespace VT0
{
    /// <summary>
    /// Moves VT0 textures to the Resources folder before a build, so the engine
    /// can find them at runtime.
    /// </summary>
    public class VT0BuildProcess : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private List<ValueTuple<string, string>> _movedFiles;

        public void OnPreprocessBuild(BuildReport report)
        {
            const string resourcesDir = "Assets/Resources/" + TextureData.ResourcesDir;
            Directory.CreateDirectory(resourcesDir);
            _movedFiles = new List<ValueTuple<string, string>>();
            foreach (var oldPath in EditorBuildSettings.scenes
                .SelectMany(s => AssetDatabase.GetDependencies(s.path, true))
                .Where(IsVT0Placeholder)
                .Distinct())
            {
                var obj = AssetDatabase.LoadAssetAtPath<Texture2D>(oldPath);
                var fileName = obj.GetImageHash().ToString("x8");
                if (Directory.GetFiles(resourcesDir, fileName + ".*").Length == 0)
                {
                    var newPath = resourcesDir + "/" + fileName;
                    File.Move(oldPath, newPath);
                    File.Move(oldPath + ".meta", newPath + ".meta");
                    _movedFiles.Add(ValueTuple.Create(oldPath, newPath));
                }
            }
            AssetDatabase.Refresh();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            foreach (var pair in _movedFiles)
            {
                File.Move(pair.Item2, pair.Item1);
                File.Move(pair.Item2 + ".meta", pair.Item1 + ".meta");
            }
            _movedFiles.Clear();
            AssetDatabase.Refresh();
        }

        private bool IsVT0Placeholder(string path)
        {
            return Path.GetDirectoryName(path).Replace('\\', '/') == VT0Drawer.PlaceholderLocation;
        }
    }
}
#endif
