using System.IO;
using Foundation.ClientService;
using UnityEditor;
using UnityEngine;
using Environment = Foundation.ConfigurationResolver.EnvironmentConfig.Environment;

namespace FoundationEditor.Utils.Editor.BuildUtils
{
    public static class EditorBuildUtils
    {
        
        /// <summary>
        /// Returns true if running in batch mode (i.e. Unity executed from command line)
        /// </summary>
        public static bool IsBatchMode => Application.isBatchMode;

        /// <summary>
        /// Get the local folder path for builds
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public static string GetClientLocalBuildPath(BuildTarget buildTarget)
        {
            return $"LocalBuilds/{buildTarget}/";
        }

        /// <summary>
        /// Creates the folder if doesn't exists.
        /// Use the "clearIfExists" flag to empty folder as well.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="clearIfExists"></param>
        public static void CreateFolder(string path, bool clearIfExists)
        {
            // Clear (delete) folder, files & sub-folders if needed
            if (clearIfExists && Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            
            // Verify folder exists...
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string GetBuildFileName(Environment environment)
        {
            string productName = PlayerSettings.productName.Replace(" ", "");
            int version = ClientVersionUtils.VersionCode;
            return $"{productName}_{version}_{environment}";
        }
    }
}
