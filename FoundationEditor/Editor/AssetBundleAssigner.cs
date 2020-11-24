using System.IO;
using Foundation.Logger;
using UnityEditor;

namespace FoundationEditor.Enforcers.Editor
{
    internal class AssetBundleAssigner : AssetPostprocessor
    {
        private const string _assetBundleFolder = "AssetBundles";
        private const string _assetsFolder = "Assets";
        private const char _pathSeparator = '/';
        private const string _bundlesPathSeparator = "_";
        private const int _maxRelevantPathLength = 4;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {

            for (int i = 0; i < importedAssets.Length; i++)
            {
                SetAssetBundleByDirectory(importedAssets[i]);
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                SetAssetBundleByDirectory(movedAssets[i]);
            }
        }

        private static void SetAssetBundleByDirectory(string filePath)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            if (directoryName.Contains(_bundlesPathSeparator))
            {
                Debugger.LogError(null, "Bundle path cannot contain \"_\" in the folder name!");
                EditorUtility.DisplayDialog("Asset cannot be assigned to bundle",
                    "Bundles folders cannot include \"_\" in their names!", "Ok");
                return;
            }

            string[] pathParts = filePath.Split(_pathSeparator);
            if (pathParts.Length >= _maxRelevantPathLength && pathParts[0].Equals(_assetsFolder) &&
                pathParts[1].Equals(_assetBundleFolder))
            {
                string assetBundleName = pathParts[2];
                // Element 2 of the path will contain the main bundle name
                // if there are more than 4 elements in the path element 3 will contain the secondary bundle name
                // Example: Assets/AssetBundles/TestBundle/Variant/FileName.png
                if (pathParts.Length > _maxRelevantPathLength)
                {
                    assetBundleName += _bundlesPathSeparator + pathParts[3];
                }

                AssetImporter.GetAtPath(filePath).SetAssetBundleNameAndVariant(assetBundleName, string.Empty);
            }
        }
    }
}
