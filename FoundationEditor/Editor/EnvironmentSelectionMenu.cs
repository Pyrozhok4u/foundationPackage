using System.Collections.Generic;
using Foundation.ConfigurationResolver;
using UnityEditor;
using Environment = Foundation.ConfigurationResolver.EnvironmentConfig.Environment;

namespace FoundationEditor.ConfigurationResolver.Editor
{
    public static class EnvironmentSelectionMenu
    {

        private const string MenuPrefix = "Foundation/Environment/";
        private const string DevMenu = MenuPrefix + "Dev";
        private const string StageMenu = MenuPrefix + "Stage";
        private const string ProdMenu = MenuPrefix + "Prod";

        private static readonly Dictionary<Environment, string> EnvironmentMenuMapper;
        
        static EnvironmentSelectionMenu()
        {
            EnvironmentMenuMapper = new Dictionary<Environment, string>();
            EnvironmentMenuMapper.Add(Environment.Dev, DevMenu);
            EnvironmentMenuMapper.Add(Environment.Stage, StageMenu);
            EnvironmentMenuMapper.Add(Environment.Prod, ProdMenu);
        }
        
        #region Envrionment Selection Menu
        
        [MenuItem(DevMenu)]
        public static void SetDevEnvironment()
        {
            SetEnvironment(Environment.Dev);
        }
    
        [MenuItem(StageMenu)]
        public static void SetStageEnvironment()
        {
            SetEnvironment(Environment.Stage);
        }
    
        [MenuItem(ProdMenu)]
        public static void SetProdEnvironment()
        {
            SetEnvironment(Environment.Prod);
        }
        
        #endregion
        
        #region Set Selected Envrionment

        private static void SetEnvironment(Environment environment)
        {
            ConfigResolverEditorService.SetEnvironment(environment);
            SetSelectedMenu(environment);
        }
        
        /// <summary>
        /// Validate function used to trigger the env selection whenever the env menu is opened.
        /// </summary>
        [MenuItem(DevMenu, true)]
        public static bool UpdateSelectedEnvironment()
        {
            Environment environment = EnvironmentConfig.LoadConfig().GetEnvironment();
            SetSelectedMenu(environment);
            return true;
        }

        /// <summary>
        /// Updates the currently selected environment in Unity's menu
        /// </summary>
        private static void SetSelectedMenu(Environment environment)
        {
            foreach (KeyValuePair<Environment,string> pair in EnvironmentMenuMapper)
            {
                bool isSelected = environment == pair.Key;
                Menu.SetChecked(pair.Value, isSelected);
            }
        }
        
        #endregion
    }
}
