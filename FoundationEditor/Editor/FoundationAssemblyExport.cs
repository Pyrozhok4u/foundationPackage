using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Logger;
using Foundation.Utils.OperationUtils;
using FoundationEditor.Utils.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace FoundationEditor.AssemblyExporter.Editor
{
	public static class FoundationAssemblyExport
	{
		private static string _assemblyExtention = ".dll";
		private static char _separator = Path.AltDirectorySeparatorChar;
		private static string _editorPath = EditorApplication.applicationPath.Substring(0, EditorApplication.applicationPath.LastIndexOf(_separator));
		private static string _tempAssemblyPath = "Temp" + _separator + "MyAssembly";
		private static string _unityLibPath = _editorPath + LibraryPath;
		private static string _networkLibsPath = $"{Application.dataPath}{_separator}Foundation{_separator}Network{_separator}DLL{_separator}";
		
		// DOTween assembly related
		private const string _dotweenAssemblyName = "TweenerModules";
		private static string _dotweenAssemblyPath = "Assembly" + _separator + "Tweener";
		private static string _dotweenScriptsPath = "Assets" + _separator + "Demigiant" + _separator + "DOTween" + _separator + "Modules";

		// Foundation assembly related
		private const string _foundationAssemblyName = "AssemblyFoundation";
		private static string _foundationScriptPath = "Assets" + _separator + "Foundation";
		private static string _foundationAssemblyPath = "Assembly";

		// best http assembly related
		private static string _httpAssemblyName = "BestHTTP";
		private static string _httpScriptPath = "Assets" + _separator + "Best HTTP";
		private static string _httpAssemblyPath = "Assembly" + _separator + "Best HTTP";
		
		// apps flyer assembly related
		private static string _appsFlyerAssemblyName = "AppsFlyer";
		private static string _appsFlyerScriptPath = "Assets" + _separator + "AppsFlyer";
		private static string _appsFlyerAssemblyPath = "Assembly" + _separator + "AppsFlyer";

		private static string LibraryPath
		{
			get
			{
				switch (Application.platform)
				{
					case RuntimePlatform.OSXEditor:
						return _separator + "Unity.app" + _separator + "Contents" + _separator + "Managed" + _separator + "UnityEngine" + _separator;
					case RuntimePlatform.WindowsEditor:
						return _separator + "Data" + _separator + "Managed" + _separator + "UnityEngine" + _separator;
					default:
						return "";
				}
			}
		}
		
		[MenuItem("Assembly Export/ Build BestHTTP Assembly", false, 1)]
		public static void BuildBestHTTP()
		{
			ClearConsole();
			Debugger.LogError(null, "You need to build it manually. Open Rider Explorer window -> right click on the BestHttp assembly -> Build." +
			                        " When it's done you can get the dll inside 'Temp/bin/' folder by default.");
			return;
			EditorCoroutine.start(WaitForCompiler(() =>
				BuildBestHTTPAssembly(() => Debugger.Log(null, "BestHTTP build completed!"))));
		}
		
		[MenuItem("Assembly Export/ 1. Build AppsFlyer Assembly", false, 20)]
		public static void BuildAppsFlyer()
		{
			ClearConsole();
			EditorCoroutine.start(WaitForCompiler(() =>
				BuildAppsFlyerAssembly(() => Debugger.Log(null, "AppsFlyer build completed!"))));
		}
		
		[MenuItem("Assembly Export/ 2. Build Tween Assembly", false, 20)]
		public static void BuildTweener()
		{
			ClearConsole();
			EditorCoroutine.start(WaitForCompiler(() =>
				BuildTweenerAssembly(() => Debugger.Log(null, "Tween build completed!"))));
		}

		[MenuItem("Assembly Export/ 3. Build Foundation Assembly", false, 20)]
		public static void BuildFoundation()
		{
			ClearConsole();
			EditorCoroutine.start(WaitForCompiler(() =>
				BuildFoundationAssembly(() => Debugger.Log(null, "Foundation build completed!"))));
		}
		
		/// <summary>
		/// Builds all Assemblys one by one. Output located in the Assembly folder(above assets)
		/// </summary>
		[MenuItem("Assembly Export/ Build All Assemblies", false, 75)]
		public static void BuildALL()
		{
			ClearConsole();
			BuildALLAssemblies();
		}

		private class DelayedCall
		{
			private Func<bool> _predicate;
			private int _delay;
			private int _dt;
			
			public DelayedCall(Func<bool> predicate, float delay, float dt)
			{
				_predicate = predicate;
				_delay = Mathf.CeilToInt(delay * 1000);		// sec to ms conversion
				_dt = Mathf.CeilToInt(dt * 1000);
			}

			public async Task Run(Action cb)
			{
				while (_predicate())
				{
					await Task.Delay(_dt);
				}
				await Task.Delay(_delay);
				cb();
			}
		}
		
		private static void DisplayProgressBar(float progress)
		{
			EditorUtility.DisplayProgressBar("Building Foundation Assembly", "Wait till the process is ready", progress);
		}

		private static void OpenAssemblyFolder()
		{
#if UNITY_EDITOR_WIN
			string assemblyPath = Application.dataPath.Replace("Assets", _foundationAssemblyPath) + @"\" + _foundationAssemblyName + ".dll";
			string foundationPath = new DirectoryInfo(assemblyPath).FullName;
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
			{
				Arguments = "/select," + foundationPath,
				FileName = "explorer.exe"
			};
			System.Diagnostics.Process.Start(startInfo);
#endif
		}
		/// <summary>
		/// BuildALLAssembly will wait for Unity to finish compilation process before starting other Assembly
		/// </summary>
		private static void BuildALLAssemblies()
		{
			DisplayProgressBar(0f);
			
			float dt = Time.fixedDeltaTime;
			Func<bool> predicate = () => EditorApplication.isCompiling;
			float delay = dt;
			new DelayedCall(predicate, delay, dt).Run(() =>
			{
				BuildAppsFlyerAssembly(() =>
				{
					Debugger.Log(null, "AppsFlyer build completed!");
					DisplayProgressBar(0.3f);
				});
				
				new DelayedCall(predicate, delay, dt).Run(() =>
				{
					BuildTweenerAssembly(() =>
					{
						Debugger.Log(null, "Tween Build COMPLETED!");
						DisplayProgressBar(0.7f);
					});
					
					new DelayedCall(predicate, delay, dt).Run(() =>
					{
						BuildFoundationAssembly(() =>
						{
							Debugger.Log(null, "All assemblies have been built successfully. Check The Assembly folder.(above Assets folder)");
							EditorUtility.ClearProgressBar();
							OpenAssemblyFolder();
						});
					});
				});
			});
		}

		private static IEnumerator WaitForCompiler(Action onCompiled)
		{
			if (EditorApplication.isCompiling) { Debugger.Log(null, "Wait Editor Application isCompiling"); }
			yield return new WaitWhile(() => EditorApplication.isCompiling);
			onCompiled?.Invoke();
		}

		private static void BuildBestHTTPAssembly(Action onCompleted = null)
		{
			string[] references =
			{
				 _unityLibPath + "UnityEditor.dll",
				 _unityLibPath + "UnityEngine.CoreModule.dll",
				 _unityLibPath + "UnityEngine.ImageConversionModule.dll"
			};
			
			BuildAssembly(_httpAssemblyName, _httpAssemblyPath, _httpScriptPath, references,result =>
			{
				Debugger.Log(null, "BestHTTP Build ready ");
				onCompleted?.Invoke();
			});
		}
		
		private static void BuildAppsFlyerAssembly(Action onCompleted = null)
		{
			string[] references =
			{
				_unityLibPath + "UnityEngine.dll",
				_unityLibPath + "UnityEditor.dll",
				_unityLibPath + "UnityEngine.CoreModule.dll",
				_unityLibPath + "UnityEngine.IMGUIModule.dll",
			};
			
			BuildAssembly(_appsFlyerAssemblyName, _appsFlyerAssemblyPath, _appsFlyerScriptPath, references, result =>
			{
				Debugger.Log(null, "AppsFlyer Build ready ");
				onCompleted?.Invoke();
			});
		}

		/// <summary>
		/// Gathers all scripts and reference assemblies for a Tween Assembly
		/// </summary>
		/// <param name="onCompleted">Action triggered after assembly build completed</param>
		private static void BuildTweenerAssembly(Action onCompleted = null)
		{
			string[] references = new string[]
			{
				//add path to missing reference here if there is some
				_unityLibPath + "UnityEngine.dll",
				_unityLibPath + "UnityEngine.CoreModule.dll",
				_unityLibPath + "UnityEngine.AudioModule.dll",
				_unityLibPath + "UnityEngine.UIModule.dll"
			};
			
			BuildAssembly(_dotweenAssemblyName, _dotweenAssemblyPath, _dotweenScriptsPath, references, result =>
			{
				onCompleted?.Invoke();
			});
		}

		/// <summary>
		/// Gathers all scripts and reference assemblys for a Foundation Assembly
		/// </summary>
		/// <param name="onCompleted">Action triggered after asembly build compleated</param>
		private static void BuildFoundationAssembly(Action onCompleted = null)
		{
			string[] tweenerAssemblyFiles = Directory.GetFiles(_dotweenAssemblyPath, "*.dll", SearchOption.AllDirectories);
			string[] httpAssemblyFiles = Directory.GetFiles(_httpAssemblyPath, "*.dll", SearchOption.AllDirectories);
			string[] appsFlyerAssemblyFiles = Directory.GetFiles(_appsFlyerAssemblyPath, "*.dll", SearchOption.AllDirectories);
			
			string[] references =
			{
				//add path to missing reference here if there is some
				_unityLibPath + "UnityEngine.dll",
				_unityLibPath + "UnityEditor.dll",
				_unityLibPath + "UnityEngine.AudioModule.dll",
				_unityLibPath + "UnityEngine.AndroidJNIModule.dll",
				_unityLibPath + "UnityEngine.CoreModule.dll",
				_unityLibPath + "UnityEngine.IMGUIModule.dll",
				_unityLibPath + "UnityEngine.InputLegacyModule.dll",
				_unityLibPath + "UnityEngine.JSONSerializeModule.dll",
				_unityLibPath + "UnityEngine.TextRenderingModule.dll",
				_unityLibPath + "UnityEngine.UnityWebRequestWWWModule.dll",
				_unityLibPath + "UnityEngine.UnityWebRequestModule.dll",
				_unityLibPath + "UnityEngine.UIModule.dll",
				_unityLibPath + "UnityEngine.AssetBundleModule.dll",
				_unityLibPath + "UnityEngine.AnimationModule.dll",
				_networkLibsPath + "Google.Protobuf.dll",
				_networkLibsPath + "System.Memory.dll",
				tweenerAssemblyFiles.Length > 0? tweenerAssemblyFiles[0] : "",
				httpAssemblyFiles.Length > 0? httpAssemblyFiles[0] : "",
				appsFlyerAssemblyFiles.Length > 0? appsFlyerAssemblyFiles[0] : ""
			};
			
			BuildAssembly(_foundationAssemblyName, _foundationAssemblyPath, _foundationScriptPath, references, result =>
			{
				Debugger.Log(null, "Foundation build completed successfully: " + result.Success);
				onCompleted?.Invoke();
			});
		}

		/// <summary>
		/// Uses Unity Editor AssemblyBuilder class to build DLL from the input parameters
		/// </summary>
		/// <param name="assemblyName">Assembly name</param>
		/// <param name="assemblyPath">Path to the assembly</param>
		/// <param name="scriptsPath">Path to folder containing scripts for the assembly. Checks subfolders</param>
		/// <param name="references">References needed to build the assembly</param>
		/// <param name="onComplete"></param>
		private static void BuildAssembly(string assemblyName, string assemblyPath, string scriptsPath, string[] references, Action<Result> onComplete = null)
		{
			// Validate folders exists
			if (!Directory.Exists(_tempAssemblyPath)) { Directory.CreateDirectory(_tempAssemblyPath); }
			if (!Directory.Exists(assemblyPath)) { Directory.CreateDirectory(assemblyPath); }
			
			//Saving old Files for future & Dll will contain scripts from scriptsPath folder only
			string[] oldAssemblyFiles = GetOldAssemblyFiles(assemblyPath, assemblyName);
			string[] scripts = GetFilesForAssembly(scriptsPath);
			string outputAssembly = GetAssemblyProjectPath(_tempAssemblyPath, assemblyName);
			string assemblyProjectPath = GetAssemblyProjectPath(assemblyPath, assemblyName);
			if (scripts.Length == 0)
			{
				Debugger.LogError(null, "Scripts doesn't exist at path: " + assemblyPath);
				return;
			}
			
			AssemblyBuilder assemblyBuilder = new AssemblyBuilder(outputAssembly, scripts)
			{
				additionalReferences = references,
				// Exclude a reference to the copy of the assembly in the Assets folder, if any.
				excludeReferences = new string[] { assemblyProjectPath }
			};
			// Called on main thread
			assemblyBuilder.buildStarted += delegate (string newAssemblyPath)
			{
				Debugger.LogFormat(null, "Assembly build started for {0}", newAssemblyPath);
			};

			Result result = new Result();
			
			// Called on main thread
			assemblyBuilder.buildFinished += delegate (string newAssemblyPath, CompilerMessage[] compilerMessages)
			{
				var errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
				var warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);
				Debugger.LogFormat(null,"Assembly build finished for {0} Warnings: {1} - Errors: {2}", newAssemblyPath, warningCount, errorCount);
				if (errorCount == 0)
				{
					RemoveOldAssembly(oldAssemblyFiles, assemblyProjectPath);
					File.Copy(outputAssembly, assemblyProjectPath, true);
					AssetDatabase.ImportAsset(assemblyProjectPath);
				}
				else
				{
					foreach (CompilerMessage cm in compilerMessages)
					{
						string message = $"|{cm.type}| file: {cm.file}({cm.line}) = {cm.message}";
						Debugger.LogAssertion(null, message, cm.type != CompilerMessageType.Error);
						result.SetFailure(message);
					}
				}
				
				// Finally, invoke callback...
				onComplete?.Invoke(result);
			};
			
			// Start building of assembly
			if (!assemblyBuilder.Build())
			{
				string errorMessage = string.Format("Failed to start build of assembly {0}!", assemblyBuilder.assemblyPath);
				Debugger.LogErrorFormat(null,errorMessage);
				result.SetFailure(errorMessage);
				onComplete?.Invoke(result);
			}
		}

		#region Helpers

		private static string GetAssemblyProjectPath(string assemblyPath, string assemblyName)
		{
			return assemblyPath + _separator + assemblyName + _assemblyExtention;
		}
		
		private static void RemoveOldAssembly(string[] oldAssemblyFiles, string assemblyProjectPath)
		{
			for (int i = 0; i < oldAssemblyFiles.Length; i++)
			{
				if (File.Exists(oldAssemblyFiles[i]))
				{
					File.Delete(oldAssemblyFiles[i]);
				}
				string meta = oldAssemblyFiles[i] + ".meta";
				if (File.Exists(meta))
				{
					File.Delete(meta);
				}
			}
		}

		private static string[] GetFilesForAssembly(string path)
		{
			string[] res = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
			var scripts = new string[res.Length];
			if (res.Length == 0)
			{
				Debugger.Log(null, "No files .cs found  " + path);
			}
			else
			{
				for (int i = 0; i < res.Length; i++)
				{
					scripts[i] = res[i].Replace("\\", "/");
				}
			}
			return scripts;
		}

		private static string[] GetOldAssemblyFiles(string path, string assemblyName)
		{
			string[] allFiles = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
			var oldAssemblyFiles = new string[allFiles.Length*2];
			if (allFiles.Length == 0)
			{
				Debugger.Log(null, "old Assembly " + assemblyName + " Files was not found path " + path);
			}
			else
			{
				for (int i = 0; i < allFiles.Length; i++)
				{
					allFiles[i] = allFiles[i].Replace("\\", "/");
					string name = GetNameFromPath(allFiles[i]);
					string newPath = _tempAssemblyPath + name.Replace(assemblyName, "Old" + assemblyName);
					CopyFile(allFiles[i], newPath);
					oldAssemblyFiles[i] = newPath;
					oldAssemblyFiles[allFiles.Length + i] = allFiles[i];
				}
			}
			return oldAssemblyFiles;
		}

		private static string GetVersionSuffix()
		{
			return "(" + Application.version + ")";
		}

		private static void ClearConsole()
		{
			Type logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
			MethodInfo clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
			clearMethod?.Invoke(null, null);
		}

		private static void CopyFile(string originalPath, string duplicatePath)
		{
			if (!File.Exists(originalPath))
			{
				Debugger.LogError(null, "Path to file " + originalPath + " doesn't exist!");
				return;
			}

			if (File.Exists(duplicatePath))
			{
				File.Delete(duplicatePath);
			}
			File.Copy(originalPath, duplicatePath);
			Debugger.Log(null, "Copied " + originalPath + " to  = " + duplicatePath);
		}

		private static string GetNameFromPath(string path)
		{
			int index = path.LastIndexOf(_separator);
			return path.Substring(index, path.Length - index);
		}
		#endregion
	}
}
