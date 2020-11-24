using System.Collections.Generic;
using Foundation.AssetBundles;
using UnityEditor;
using UnityEngine;
using BundlesMode = Foundation.AssetBundles.AssetBundlesService.BundlesMode;

namespace FoundationEditor.AssetBundles.Editor
{
    public class AssetBundleModeSelectionMenu : UnityEditor.Editor
    {
        private const string MenuPrefix = "Foundation/Asset Bundle Mode/";
        private const string LocalModeMenu = MenuPrefix + "Local";
        private const string RemoteModeMenu = MenuPrefix + "Remote";
        
        private static readonly Dictionary<BundlesMode, string> MenuMapper;
        
        static AssetBundleModeSelectionMenu()
        {
            MenuMapper = new Dictionary<BundlesMode, string>();
            MenuMapper.Add(BundlesMode.Local, LocalModeMenu);
            MenuMapper.Add(BundlesMode.Remote, RemoteModeMenu);
        }
        
        [MenuItem(LocalModeMenu)]
        public static void LocalBundlesModes()
        {
            PlayerPrefs.SetInt(AssetBundlesService.BundlesModeKey, (int)BundlesMode.Local);
        }
        
        [MenuItem(RemoteModeMenu)]
        public static void RemoteBundlesModes()
        {
            PlayerPrefs.SetInt(AssetBundlesService.BundlesModeKey, (int)BundlesMode.Remote);
        }
        
        [MenuItem(LocalModeMenu, true)]
        public static bool UpdateSelectedBundlesMode()
        {
            BundlesMode mode = (BundlesMode)PlayerPrefs.GetInt(AssetBundlesService.BundlesModeKey, (int)BundlesMode.Local);
            SetSelectedMenu(mode);
            return true;
        }
        
        /// <summary>
        /// Updates the currently selected menu
        /// </summary>
        private static void SetSelectedMenu(BundlesMode mode)
        {
            foreach (KeyValuePair<BundlesMode,string> pair in MenuMapper)
            {
                bool isSelected = mode == pair.Key;
                Menu.SetChecked(pair.Value, isSelected);
            }
        }
    }
}
