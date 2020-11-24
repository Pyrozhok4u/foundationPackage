using Foundation.TimerUtils.UI;
using UnityEditor;
using UnityEngine;

namespace FoundationEditor.UI.Time.Editor
{
	[CustomEditor(typeof(TimerText))]
	public class TimerTextInspector : TimerUIInspector
	{
		private SerializedProperty _component;

		private void OnEnable()
		{
			_timer = (TimerUI)target;
			_component = serializedObject.FindProperty("_textComponent");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(_component);
			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
			base.OnInspectorGUI();
		}
	}
}
