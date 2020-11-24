using Foundation.Utils.OperationUtils;
using UnityEngine;

namespace Foundation.ClientService
{
    public static class ClientVersionUtils
    {

        private static string _version;
        private static int _versionCode;

        public static string Version
        {
            get
            {
                #if UNITY_EDITOR
                // On editor - always update value
                _version = GetPaddedVersion();
                #endif
                return _version;
            }
        }

        public static int VersionCode
        {
            get
            {
                #if UNITY_EDITOR
                // On editor - always update value
                SetVersionCode();
                #endif
                return _versionCode;
            }
        }
    
        static ClientVersionUtils()
        {
            _version = GetPaddedVersion();
            SetVersionCode();
        }

        private static string GetPaddedVersion()
        {
            string[] components = Application.version.Split('.');
            string paddedVersion = components[0];
            for (int i = 1; i < components.Length; i++)
            {
                paddedVersion += components[i].PadLeft(2, '0');
            }

            return paddedVersion;
        }

        private static void SetVersionCode()
        {
            Result<int> result = GetVersionCode(_version);
            if(result.Success) { _versionCode = result.Data; }
        }
    
        public static Result<int> GetVersionCode(string version = null)
        {
            if(string.IsNullOrEmpty(version)) { version = Version; }
        
            Result<int> result = new Result<int>();
            if (int.TryParse(version, out int code))
            {
                result.Data = code;
            }
            else
            {
                result.SetFailure("Failed parsing version code: " + version);
            }
            return result;
        }
    }
}
