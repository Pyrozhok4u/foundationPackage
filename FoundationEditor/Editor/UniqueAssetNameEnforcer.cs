using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FoundationEditor.ConfigurationResolver.Editor;
using UnityEditor;
using UnityEngine;

namespace FoundationEditor.Enforcers.Editor
{
    internal class UniqueAssetNameEnforcer : AssetPostprocessor
    {
        private static readonly string[] _ignoredFileExtensions = { ".meta", ".cs", ".h", ".dll", ""};
        private static readonly string[] _ignoredFileNames = { "Link.xml", "Manifest.xml", "AndroidManifest.xml", "License.txt"};
        private static readonly string[] _ignoredFolders = { ConfigResolverEditorService.ConfigurationResourcesRelativePath };

        private static Dictionary<string, AssetData> _existingAssetNames = new Dictionary<string, AssetData>();
        private static readonly List<string> NewAssetNames = new List<string>();

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // InitializeNameCollection();
            //
            // for (int i = 0; i < deletedAssets.Length; i++)
            // {
            //     string assetName = Path.GetFileNameWithoutExtension(deletedAssets[i]);
            //     _existingAssetNames.Remove(assetName);
            // }
            //
            // for (int i = 0; i < importedAssets.Length; i++)
            // {
            //     EnforceAssetName(importedAssets[i]);
            // }
            //
            // for (int i = 0; i < movedAssets.Length; i++)
            // {
            //     string previousAssetName = Path.GetFileNameWithoutExtension(movedFromAssetPaths[i])?.ToLower();
            //     if (_existingAssetNames.ContainsKey(previousAssetName) && _existingAssetNames[previousAssetName].AssetPath == movedFromAssetPaths[i])
            //     {
            //         _existingAssetNames.Remove(previousAssetName);
            //     }
            //     EnforceAssetName(movedAssets[i]);
            // }
            //
            // //In order to refresh the Editor after making changes we have to run this code after a delay
            // //This method is somewhat analogous to executing something on the next frame at runtime
            // EditorApplication.delayCall += () =>
            // {
            //     AssetDatabase.SaveAssets();
            //     AssetDatabase.Refresh();
            // };
        }

        private static void EnforceAssetName(string path)
        {
            if (!ValidateFile(path)) { return; }

            string assetName = Path.GetFileNameWithoutExtension(path);
            if(string.IsNullOrEmpty(assetName)) { return; }

            // Asset with this name is in the database
            if (_existingAssetNames.ContainsKey(assetName.ToLower()))
            {
                // New asset with name already in use, rename
                if (_existingAssetNames[assetName.ToLower()].AssetPath != path)
                {
                    string newAssetName = FindNewAssetName(assetName);
                    AssetDatabase.RenameAsset(path, newAssetName);
                }
            }
            // New asset
            else
            {
                _existingAssetNames.Add(assetName.ToLower(), new AssetData(path));
            }
        }

        private static string FindNewAssetName(string assetName)
        {
            string s = Regex.Match(assetName, @"\d+$").Value;
            int suffixIndex = 1;
            string newAssetName = assetName + "_" + suffixIndex;
            if (int.TryParse(s, out int lastIndex))
            {
                suffixIndex = lastIndex + 1;
                string partialName = assetName.Substring(0, assetName.LastIndexOf(lastIndex.ToString(), StringComparison.OrdinalIgnoreCase));
                        
                while (_existingAssetNames.ContainsKey((partialName + suffixIndex).ToLower())
                       || NewAssetNames.Contains((partialName + suffixIndex).ToLower()))
                {
                    suffixIndex++;
                }
                        
                newAssetName = partialName + suffixIndex;
            }

            NewAssetNames.Add(newAssetName.ToLower());

            return newAssetName;
        }

        private static bool ValidateFile(string filePath)
        {
            bool valid = true;
            if (string.IsNullOrEmpty(filePath))
            {
                valid = false;
                return valid;
            }

            // Validate relevant file types
            if (_ignoredFileExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase))
            {
                valid = false;
            }

            // Validate if specific file name is white listed (i.e. manifest.xml, link.xml etc...)
            if (_ignoredFileNames.Contains(Path.GetFileName(filePath), StringComparer.OrdinalIgnoreCase))
            {
                valid = false;
            }

            // Validate if file is part of ignored folders (i.e. config files can be duplicated across envs)
            foreach (string ignoredFolder in _ignoredFolders)
            {
                if (filePath.Contains(ignoredFolder))
                {
                    valid = false;
                    break;
                }
            }

            return valid;
        }

        private static void InitializeNameCollection()
        {
            _existingAssetNames.Clear();
            
            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                string assetName = Path.GetFileNameWithoutExtension(path)?.ToLower();
                if(string.IsNullOrEmpty(assetName)) { continue; }
            
                if (ValidateFile(path) && !path.Contains("Packages/") && !path.Contains("Plugins/"))
                {
                    if (_existingAssetNames.ContainsKey(assetName))
                    {
                        Debug.LogError($"A file with the name {assetName} already exists in the project. Paths are {path} and {_existingAssetNames[assetName].AssetPath}");
                        EnforceAssetName(path);
                    }
                    else
                    {
                        _existingAssetNames.Add(assetName, new AssetData(path));
                    }
                }
            }
        }

        private class AssetData
        {
            public readonly string AssetPath;
            public AssetData(string assetPath) => AssetPath = assetPath;
        }
    }
}
