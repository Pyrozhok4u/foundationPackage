using System;
using System.Collections.Generic;
using System.Text;
using Foundation.Logger;
using Foundation.Utils.OperationUtils;

namespace Foundation.Utils.CommandLineUtils
{
    /// <summary>
    /// Helper class to receive & parse command line args
    /// Main usage is designed for the automatic build pipeline
    /// </summary>
    public class CommandLineArgsUtils
    {
        private Dictionary<string, string> Args;
        private const char KeyPrefix = '-';

        public Result Initialize()
        {
            Result result = new Result();
            try
            {
                Args = GetCommandLineArgs();
            }
            catch (Exception e)
            {
                this.LogException(e);
                result.SetFailure("Failed parsing command line args! See logs for more details...");
            }
            return result;
        }

        /// <summary>
        /// Returns a mapping of all command line arguments
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetCommandLineArgs()
        {
            // Args example:
            // /opt/Unity/Editor/Unity -batchmode -logfile /dev/stdout -quit -projectPath /github/workspace
            // -buildTarget WebGL -buildType debug -buildPath /github/workspace/Build/WebGL
            // -executeMethod Foundation.BuildPipelineService.Editor.BuildService.Build -version
            string[] rawArgs = Environment.GetCommandLineArgs();
            Dictionary<string, string> args = new Dictionary<string, string>();

            PrintCommandLineArgs(rawArgs);

            for (int i = 0; i < rawArgs.Length - 1; i++)
            {
                string potentialKey = rawArgs[i];
                this.Log("Parse command line arg potential key: " + potentialKey);
                if(string.IsNullOrEmpty(potentialKey)) { continue; }

                // For each args that start with "-", add the key value pair without the "-" prefix
                // i.e. "-buildTarget target" will be added as key: "buildTarget", value: "target"
                if (potentialKey[0] == KeyPrefix)
                {
                    string value = rawArgs[i + 1];
                    if(string.IsNullOrEmpty(value)) { continue; }
                    // Make sure next arg is a value and not a key (it's possible to have two subsequent args without values)
                    if(value[0] == KeyPrefix) { continue; }

                    string key = rawArgs[i].Substring(1);
                    args.Add(key, value);
                    this.Log("Add command line key / value pair: " + key + ": " + value);
                    // Skip next iteration because we already parsed the key / value pair
                    i++;
                }
            }

            return args;
        }

        #region Parsers helpers

        /// <summary>
        /// // For debug, Cconcat all args for a nice log...
        /// </summary>
        /// <param name="rawArgs"></param>
        private void PrintCommandLineArgs(string[] rawArgs)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rawArgs.Length - 1; i++)
            {
                sb.Append(rawArgs[i] + " ");
            }
            this.Log("Command line args: " + sb);
        }

        public T GetEnumValue<T>(string key) where T : Enum
        {
            this.Log("Try get enum value for key: " + key);
            return (T) Enum.Parse(typeof(T), Args[key], true);
        }

        public bool GetBoolValue(string key)
        {
            this.Log("Try get bool value for key: " + key);
            return bool.Parse(Args[key]);
        }

        public string GetStringValue(string key)
        {
            this.Log("Try get string value for key: " + key);
            return Args[key];
        }

        public int GetIntNumber(string key)
        {
            this.Log("Try get int value for key: " + key);
            return int.Parse(Args[key]);
        }

        #endregion
    }
}
