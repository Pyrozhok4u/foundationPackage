using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Amazon;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using Amazon.Util.Internal;
using Foundation.Logger;
using UnityEditor;
using UnityEngine;

namespace FoundationEditor.AWSUtils.Editor
{
    public class S3BatchModeService : S3Service
    {

        private MethodInfo _updateMethod;
        private UnityMainThreadDispatcher _unityMainThreadDispatcher;
        private object[] _methodArgs;

        public void SetOverrideConfig(S3Config overrideConfig)
        {
            _config = overrideConfig;
            _identityPoolId = overrideConfig.IdentityPoolId;
            _bucketName = overrideConfig.BucketName;
            _localFolderName = overrideConfig.LocalFolderName;
        }

        /// <summary>
        /// Adds support to running AWS in batch mode by initializing AWS internals & hooking
        /// the mono update to an editor update during batch mode builds
        /// </summary>
        protected override void CreateUnityInitializer()
        {
            // if (!Application.isBatchMode)
            // {
            //     this.LogError("S3BatchModeService can only be used in batch mode!");
            //     return;
            // }
            
            DestroyExistingInstances();
            
            base.CreateUnityInitializer();
            
            // Use reflection to manually simulate the awake function work + inject update calls for it...
            UnityInitializer unityInitializer = Initializer.GetComponent<UnityInitializer>();
            Type type = unityInitializer.GetType();
            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance;
			
            FieldInfo fieldInfo = type.GetField("_instance", bindingFlags);
            fieldInfo.SetValue(null, unityInitializer);
			
            fieldInfo = type.GetField("_mainThread", bindingFlags);
            fieldInfo.SetValue(null, Thread.CurrentThread);

            // Internal code copied from UnityInitializer Awake()
            AmazonHookedPlatformInfo.Instance.Init();
            AWSConfigs.AddTraceListener("Amazon", (TraceListener) new UnityDebugTraceListener("UnityDebug"));
            Initializer.AddComponent<UnityMainThreadDispatcher>();

            // Finally, get the main thread dispatcher update method to call it "manually" using the editor's update loop.
            _unityMainThreadDispatcher = Initializer.GetComponent<UnityMainThreadDispatcher>();
            _updateMethod = _unityMainThreadDispatcher.GetType().GetMethod("Update", bindingFlags);
            _methodArgs = new object[] {};
            EditorApplication.update += OnEditorUpdate;
        }

        private void DestroyExistingInstances()
        {
            // Make sure no other initializer already exists
            UnityInitializer previousInitializer = GameObject.FindObjectOfType<UnityInitializer>();
            if (previousInitializer != null)
            {
                GameObject.DestroyImmediate(previousInitializer);
            }
            UnityMainThreadDispatcher previousDispatcher = GameObject.FindObjectOfType<UnityMainThreadDispatcher>();
            if (previousDispatcher != null)
            {
                GameObject.DestroyImmediate(previousDispatcher);
            }
        }
        
        private void OnEditorUpdate()
        {
            if (_unityMainThreadDispatcher == null)
            {
                _unityMainThreadDispatcher = Initializer.GetComponent<UnityMainThreadDispatcher>();
            }

            _updateMethod?.Invoke(_unityMainThreadDispatcher, _methodArgs);
        }
        
        public override void Dispose()
        {
            // On batch mode we should destroy immediate..
            if (Initializer != null)
            {
                GameObject.DestroyImmediate(Initializer);
                Initializer = null;
            }
            EditorApplication.update -= OnEditorUpdate;
            
            // Finally, call base dispose
            base.Dispose();
        }
        
    }
}
