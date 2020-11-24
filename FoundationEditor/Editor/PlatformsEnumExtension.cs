using System;
using Foundation;
using Foundation.Logger;
using UnityEditor;

namespace FoundationEditor.Utils.Editor
{
    public static class PlatformsEnumExtension
    {
        
        /// <summary>
        /// Returns platform target group corresponding to the given build target
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public static BuildTargetGroup ToTargetGroup(this BuildTarget buildTarget)
        {
            BuildTargetGroup buildTargetGroup = BuildTargetGroup.Unknown;
            try
            {
                buildTargetGroup = (BuildTargetGroup) Enum.Parse(typeof(BuildTargetGroup), buildTarget.ToString());
            }
            catch (Exception e)
            {
                Debugger.LogError(null, "Cannot cast build target to build target group: " + buildTarget);
                Debugger.LogException(null, e);
            }

            return buildTargetGroup;
        }

        /// <summary>
        /// Returns a platform type corresponding to the given build target
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public static PlatformType ToPlatformType(this BuildTarget buildTarget)
        {
            PlatformType platformType = PlatformType.Unknown;
            try
            {
                platformType = (PlatformType) Enum.Parse(typeof(PlatformType), buildTarget.ToString());
            }
            catch (Exception e)
            {
                Debugger.LogError(null, "Cannot cast build target to platform type: " + buildTarget);
                Debugger.LogException(null, e);
            }

            return platformType;
        }
        
    }
}
