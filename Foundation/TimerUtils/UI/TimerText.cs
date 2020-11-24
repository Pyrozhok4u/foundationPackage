using UnityEngine;
using UnityEngine.UI;

namespace Foundation.TimerUtils.UI
{
	[RequireComponent(typeof(Text))]
	public class TimerText : TimerUI
	{
		[SerializeField] private Text _textComponent;

		private void OnValidate()
		{
			if (_textComponent == null)
			{
				_textComponent = GetComponent<Text>();
			}
		}

		protected override void SetTimeToComponent(string formattedTime)
		{
			_textComponent.text = formattedTime;
		}
	}
}
