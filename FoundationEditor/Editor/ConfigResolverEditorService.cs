using System;
using System.Collections.Generic;
using System.IO;
using Foundation.ConfigurationResolver;
using Foundation.Logger;
using Foundation.Utils.OperationUtils;
using Foundation.Utils.ReflectionUtils;
using UnityEditor;
using UnityEngine;

namespace FoundationEditor.ConfigurationResolver.Editor
{
    public static class ConfigResolverEditorService
    {
        public static string ConfigurationResourcesRelativePath = "/FoundationConfig/Resources/";
        private const string ConfigurationMenuPrefix = "Foundation/Configurations/";
        private static readonly Type EnvironmentType = typeof(EnvironmentConfig);
    
        [MenuItem(ConfigurationMenuPrefix + "Create Missing Configurations")]
        public static void CreateMissingEnvironmentConfig()
        {
            Result result = new Result();
            bool createdConfiguration = false;
            EnvironmentConfig environmentConfig = EnvironmentConfig.LoadConfig();
            if (environmentConfig == null)
            {
                CreateConfig(EnvironmentType, ConfigurationResourcesRelativePath);
                result.AddMessage(EnvironmentType.Name);
                createdConfiguration = true;
            }
        
            List<Type> missingConfigs = GetMissingConfigurations();
            foreach (Type type in missingConfigs)
            {
                CreateConfig(type, ConfigurationResourcesRelativePath + environmentConfig.GetEnvironment() + "/");
                result.AddMessage(type.Name);
                createdConfiguration = true;
            }
        
            if (createdConfiguration) { result.SetSubTitle("Created configurations:"); }

            ShowResultPopup("Create missing configurations", result);
        }

        private static void CreateConfig(Type type, string folderPath)
        {
            ScriptableObject so = ScriptableObject.CreateInstance(type);

            string path = "Assets/" + folderPath + type.Name + ".asset";
            CreateFolder(folderPath);
            AssetDatabase.CreateAsset(so, path);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = so;
        }

        private static void CreateFolder(string relativeFolderPath)
        {
            string finalPath = Application.dataPath + relativeFolderPath;
            if (!Directory.Exists(finalPath))
            {
                Directory.CreateDirectory(finalPath);
            }
        }
    
        [MenuItem(ConfigurationMenuPrefix + "Validate Environment Configurations")]
        public static void ValidateAllConfigurations()
        {
            Result result = new Result();
            List<Type> missingConfigs = GetMissingConfigurations();
            EnvironmentConfig environmentConfig = EnvironmentConfig.LoadConfig();
            if (environmentConfig == null)
            {
                result.SetFailure(EnvironmentType.Name);
            }
            else if (missingConfigs.Count > 0)
            {
                foreach (Type type in missingConfigs)
                {
                    result.SetFailure(type.Name);
                }
            }

            if (!result.Success) { result.SetSubTitle("Missing configurations:"); }
        
            Debugger.LogAssertion(null, "Validate all configurations: " + result.Success, result.Success);
            ShowResultPopup("Validate all configurations", result);
        }
    
        public static List<Type> GetMissingConfigurations()
        {
            List<Type> configTypes = ReflectionUtils.GetConcreteDerivedTypes<BaseConfig>();
            ConfigResolver configResolver = new ConfigResolver();
            List<Type> missingConfigs = new List<Type>();
            foreach (Type type in configTypes)
            {
                BaseConfig config = configResolver.LoadConfig(type);
                if (config == null)
                {
                    missingConfigs.Add(type);
                }
            }

            return missingConfigs;
        }

        public static void SetEnvironment(EnvironmentConfig.Environment environment)
        {
            EnvironmentConfig config = EnvironmentConfig.LoadConfig();
            config.SetEnvironment(environment);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ShowResultPopup(string title, Result result)
        {
            if(Application.isBatchMode) { return; }
        
            EditorUtility.DisplayDialog(title, result.ToString(), "ok");
        }
    }
}
