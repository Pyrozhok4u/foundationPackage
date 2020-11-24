using TMPro;
using UnityEngine;

namespace Foundation.TimerUtils.UI
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class TimerTextMesh : TimerUI
	{
		[SerializeField] private TextMeshProUGUI _textComponent;

		private void OnValidate()
		{
			if (_textComponent == null)
			{
				_textComponent = GetComponent<TextMeshProUGUI>();
			}
		}

		protected override void SetTimeToComponent(string formattedTime)
		{
			_textComponent.text = formattedTime;
		}
	}
}
