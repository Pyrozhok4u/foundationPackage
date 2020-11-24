using System;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;

namespace FoundationEditor.Network.Editor
{
	public class ProtobufBuilderTool : EditorWindow
	{
		private string _protoPath;
		private string _csharpOut;
		private string _protoToolPath;
		private ProtoCompilers _protoCompiler = ProtoCompilers.MacosxX64;

		private enum ProtoCompilers
		{
			MacosxX64,
			WindowsX64,
		}

		[MenuItem("Protobuf/Generate C# Class")]
		public static void Init()
		{
			ProtobufBuilderTool window = (ProtobufBuilderTool)GetWindow(typeof(ProtobufBuilderTool));
			window.titleContent = new GUIContent("Protobuf Tool");
			window.Show();
		}

		private void OnGUI()
		{
			GUILayout.Label("Proto Builder", EditorStyles.boldLabel);

			EditorGUILayout.BeginHorizontal();
			_protoPath = EditorGUILayout.TextField("Proto Path", _protoPath);
			if (GUILayout.Button("select", GUILayout.Width(50)))
			{
				_protoPath = EditorUtility.OpenFilePanel("Proto Path", string.Empty, "proto");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			_csharpOut = EditorGUILayout.TextField("CSharp Out", _csharpOut);
			if (GUILayout.Button("select", GUILayout.Width(50)))
			{
				_csharpOut = EditorUtility.OpenFolderPanel("CSharp Out", string.Empty, string.Empty);
			}
			EditorGUILayout.EndHorizontal();

			_protoCompiler = (ProtoCompilers)EditorGUILayout.EnumPopup("Proto Compiler", _protoCompiler);

			if (GUILayout.Button("Generate C# Class"))
			{
				CreateProto();
			}
		}

		private void CreateProto()
		{
			string protoToolPath = Application.dataPath + "/ProtobufCompiler/google.protobuf.tools.3.12.3";
			switch (_protoCompiler)
			{
				case ProtoCompilers.MacosxX64:
					protoToolPath += "/macosx_x64/protoc";
					break;
				case ProtoCompilers.WindowsX64:
					protoToolPath += "/windows_x64/protoc_1.exe";
					break;
			}
			if (!File.Exists(protoToolPath))
			{
				EditorUtility.DisplayDialog("Protobuf Compiler Tool Not Provided",
				"Please make sure that Assets contains ProtobufCompiler/google.protobuf.tools for specified OS",
					"Ok");
				return;
			}
			ProcessStartInfo processInfo = new ProcessStartInfo(
				protoToolPath,
				$"--proto_path={SubstringProtoFolderPath()} --csharp_out={_csharpOut} {SubstringProtoFilePath()}"
			) {CreateNoWindow = false, UseShellExecute = false};
			Process process = Process.Start(processInfo);
			process?.WaitForExit();
			process?.Close();
			EditorUtility.DisplayDialog("C# File Generated Successfully", String.Empty, "Ok");
		}

		private string SubstringProtoFilePath()
		{
			return _protoPath.Substring(_protoPath.LastIndexOf("/", StringComparison.Ordinal)).Replace("/", "");
		}

		private string SubstringProtoFolderPath()
		{
			return _protoPath.Substring(0, _protoPath.LastIndexOf("/", StringComparison.Ordinal));
		}
	}
}
