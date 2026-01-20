using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace InterwovenCode
{
    public static class Utils
    {
        #region Selecion

        /// <summary>
        /// 获取全部选中对象RectTransform
        /// </summary>
        /// <returns>返回列表</returns>
        public static List<RectTransform> GetAllSelectionRectTransform ()
        {
            List<RectTransform> rects = new List<RectTransform>();
            GameObject[] objects = Selection.gameObjects;
            foreach (var obj in objects)
            {
                RectTransform rect = obj.GetComponent<RectTransform>();
                if (rect != null)
                    rects.Add(rect);
            }

            return rects;
        }

        #endregion

        #region Path

        /// <summary>
        /// Replaces backslashes with forward slashes.
        /// </summary>
        public static string FormatPath (string path)
        {
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// Invokes <see cref="Path.Combine(string[])"/> and replaces backslashes with forward slashes on the result.
        /// </summary>
        public static string Combine (params string[] paths)
        {
            return FormatPath(Path.Combine(paths));
        }

        /// <summary>
        /// Given an absolute path inside current Unity project (eg, 'C:\UnityProject\Assets\FooAsset.asset'),
        /// transforms it to a relative project asset path (eg, 'Assets/FooAsset.asset'); returns null when
        /// specified path is not inside current Unity project (doesn't start with <see cref="Application.dataPath"/>).
        /// </summary>
        public static string AbsoluteToAssetPath (string absolutePath)
        {
            absolutePath = FormatPath(absolutePath);
            if (!absolutePath.StartsWith(Application.dataPath, StringComparison.Ordinal)) return null;
            return absolutePath.Replace(Application.dataPath, "Assets");
        }

        public static string PackageRootPath => GetPackageRootPath();

        // public static string EditorResourcesPath => PathUtils.Combine(PackageRootPath, "Editor/Resources");
        public static string EditorResourcesPath => Combine(PackageRootPath, "Editor/Res");

        private static string cachedPackagePath;

        /// <summary>
        /// Resets the cached paths and resolves them again.
        /// </summary>
        public static void Refresh ()
        {
            cachedPackagePath = null;
            _ = PackageRootPath;
        }

        private static string GetPackageRootPath ()
        {
            const string beacon = "RectTransformAlignExtension.Editor.asmdef";
            if (string.IsNullOrEmpty(cachedPackagePath) || !Directory.Exists(cachedPackagePath))
                cachedPackagePath = FindInPackages() ?? FindInAssets();
            return cachedPackagePath ?? throw new Exception("Failed to locate plugin package directory.");

            static string FindInPackages ()
            {
                // Even when package is installed as immutable (eg, local or git) and only physically
                // exists under Library/PackageCache/…, Unity will still symlink it to Packages/….
                const string dir = "Packages/com.interwovencode.recttransformalignextension";
                return Directory.Exists(dir) ? dir : null;
            }

            static string FindInAssets ()
            {
                var options = new EnumerationOptions {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true,
                    ReturnSpecialDirectories = false,
                    AttributesToSkip = FileAttributes.System
                };
                foreach (var path in Directory.EnumerateFiles(Application.dataPath, beacon, options))
                    return AbsoluteToAssetPath(Path.GetDirectoryName(Path.GetDirectoryName(path)));
                return null;
            }

            #endregion
        }
    }
}
