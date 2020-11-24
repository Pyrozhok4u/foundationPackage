using Foundation.TimerUtils.UI;
using UnityEditor;
using UnityEngine;

namespace FoundationEditor.UI.Time.Editor
{
	[CustomEditor(typeof(TimerUI))]
	public class TimerUIInspector : UnityEditor.Editor
	{
		protected TimerUI _timer;

		private void OnEnable()
		{
			_timer = (TimerUI)target;
		}

		public override void OnInspectorGUI()
		{
			DrawTimeUnits();
			DrawSufix();
		}

		private void DrawTimeUnits()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Time units", EditorStyles.boldLabel);
			GUILayout.EndHorizontal();

			if (_timer.AutoTrim)
			{
				GUILayout.BeginHorizontal();
				_timer.MinUnitsAmount = EditorGUILayout.IntSlider(
					new GUIContent("Min Units", "Minnimal time units ammount that will be always displayed. Set to 1 by default"),
					_timer.MinUnitsAmount, 1, _timer.MaxUnits);
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.BeginHorizontal();
				_timer.PreferredUnitsAmount = EditorGUILayout.IntSlider(
					new GUIContent("Prefered Units", "Time units ammount that will be always displayed"),
					_timer.PreferredUnitsAmount, 1, _timer.MaxUnits);
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal();
			_timer.AutoTrim = EditorGUILayout.Toggle(new GUIContent("Auto Trim", "Will crop timer up to Minimal Units ammount if enabled"), _timer.AutoTrim);
			GUILayout.EndHorizontal();
		}

		private void DrawSufix()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Sufix settings", "Use default or custom sufix for time units"), EditorStyles.boldLabel);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			_timer.UseSuffix = EditorGUILayout.Toggle(new GUIContent("Use sufix", "Enable to use sufix nex to time unit"), _timer.UseSuffix);
			GUILayout.Label("Default sufixes " + _timer.DefaultSuffix, GUILayout.Width(150));
			GUILayout.EndHorizontal();

			if (_timer.UseSuffix)
			{
				GUILayout.BeginHorizontal();
				_timer.UseCustomSuffix = !EditorGUILayout.Toggle(new GUIContent("Custom sufix set", "Use default or custom sufix for time units"), !_timer.UseCustomSuffix);
				GUILayout.EndHorizontal();
			}

			if (_timer.UseSuffix && !_timer.UseCustomSuffix)
			{
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				GUILayout.Label("Days", GUILayout.Width(40));
				_timer.DelimiterDays = EditorGUILayout.TextField(_timer.DelimiterDays);
				GUILayout.Space(5);
				GUILayout.Label("Hours", GUILayout.Width(40));
				_timer.DelimiterHours = EditorGUILayout.TextField(_timer.DelimiterHours);
				GUILayout.Space(5);
				GUILayout.Label("Min", GUILayout.Width(35));
				_timer.DelimiterMinutes = EditorGUILayout.TextField(_timer.DelimiterMinutes);
				GUILayout.Space(5);
				GUILayout.Label("Sec", GUILayout.Width(40));
				_timer.DelimiterSeconds = EditorGUILayout.TextField(_timer.DelimiterSeconds);
				GUILayout.EndHorizontal();
			}
		}
	}
}

