using System;
using System.Text;
using Foundation.Logger;
using Foundation.MonoUtils;
using UnityEngine;


namespace Foundation.TimerUtils.UI
{
	// ReSharper disable once InconsistentNaming
	public class TimerUI : BaseMonoBehaviour
	{
		
		public enum TimeUnit { Seconds = 0, Minutes = 1, Hours = 2, Days = 3 }
		
		//Public to be able to expose in Editor
		public bool UseSuffix, UseCustomSuffix, AutoTrim;
		public string DelimiterDays;
		public string DelimiterHours;
		public string DelimiterMinutes;
		public string DelimiterSeconds;
		public int PreferredUnitsAmount = 3;
		public int MinUnitsAmount = 1;
		public int MaxUnits = 4;

		private const string SuffixDefaultDays = "d";
		private const string SuffixDefaultHours = "h";
		private const string SuffixDefaultMinutes = "m";
		private const string SuffixDefaultSeconds = "s";
		private const string DefaultDelimiterDays = " ";
		private const string DelimiterTime = ":";
		private const string DaysFormat = "{0:D0}{1}{2}";
		private const string TimeFormat = "{0:D2}{1}{2}";
		
		private int _initialTimeInSec;
		private Action _onTimerEnd;

		public string DefaultSuffix => "( " + SuffixDefaultDays + " " +
		                              SuffixDefaultHours + " " +
		                              SuffixDefaultMinutes + " " +
		                              SuffixDefaultSeconds + " )";

		private ITimerService _timerService;
		
		private void Awake()
		{
			_timerService = Client.ServiceResolver.Resolve<ITimerService>();
		}

		public void SetTime(TimeSpan timeSpan, Action onTimerEnd = null)
		{
			SetTime(timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, onTimerEnd);
		}
		
		public void SetTime(int days, int hours, int minutes, int seconds, Action onTimerEnd = null)
		{
			_initialTimeInSec = days * 60 * 60 * 24 + hours * 60 * 60 + minutes * 60 + seconds;
			_onTimerEnd = onTimerEnd;
			_timerService.StartPerSecondTimer
			(
				timerDuration: _initialTimeInSec,
				onTimerFinished: OnTimerFinished,
				onTimerTick: (float time) => FormatTime((int)time)
			);
		}

		private void FormatTime(float timeLeft)
		{
			var time = TimeSpan.FromSeconds(timeLeft);
			SetTimeToComponent(GetFormatted(time));
		}

		private void OnTimerFinished()
		{
			this.Log("Timer Finished " + gameObject.name);
			_onTimerEnd?.Invoke();
		}

		/// <summary>
		/// Used to extract exact format
		/// </summary>
		/// <param name="duration">actual time on update</param>
		/// <returns></returns>
		private string GetFormatted(TimeSpan duration)
		{
			int unitsSelected = 0;
			StringBuilder myStringBuilder = new StringBuilder();

			if (duration.Days > 0)
			{
				unitsSelected++;
				bool isComposed = HasEnoughUnits(unitsSelected);
				myStringBuilder.AppendFormat(DaysFormat, duration.Days, GetSuffix(TimeUnit.Days), isComposed ? string.Empty : DefaultDelimiterDays);
				if (isComposed) { return myStringBuilder.ToString(); }
			}
			if (duration.Hours > 0 || duration.TotalHours >= 1)
			{
				unitsSelected++;
				bool isComposed = HasEnoughUnits(unitsSelected);
				myStringBuilder.AppendFormat(TimeFormat, duration.Hours, GetSuffix(TimeUnit.Hours), isComposed ? string.Empty : DelimiterTime);
				if (isComposed) { return myStringBuilder.ToString(); }
			}
			if (duration.Minutes > 0 || duration.TotalMinutes >= 1)
			{
				unitsSelected++;
				bool isComposed = HasEnoughUnits(unitsSelected);
				myStringBuilder.AppendFormat(TimeFormat, duration.Minutes, GetSuffix(TimeUnit.Minutes), isComposed ? string.Empty : DelimiterTime);
				if (isComposed) { return myStringBuilder.ToString(); }
			}
			if (duration.Seconds > 0 || duration.TotalSeconds >= 1)
			{
				unitsSelected++;
				myStringBuilder.AppendFormat(TimeFormat, duration.Seconds, GetSuffix(TimeUnit.Seconds), string.Empty);
				if (HasEnoughUnits(unitsSelected)) { return myStringBuilder.ToString(); }
			}

			//this.Log("Result builder = " + myStringBuilder + " selected " + unitsSelected + "/" + PreferredUnitsAmount);
			if (AutoTrim && unitsSelected >= MinUnitsAmount)
			{
				return myStringBuilder.ToString();
			}
			else
			{
				//if don't have enough units  -  we need to add nodes
				int left = (AutoTrim ? MinUnitsAmount : PreferredUnitsAmount) - unitsSelected;
				for (int i = 0; i < left; i++)
				{
					bool isLastReplacement = AutoTrim ? (i == MinUnitsAmount - 1) : (i == PreferredUnitsAmount - 1);  //is last append in empty timer
					bool isDays = (TimeUnit)(unitsSelected + i) == TimeUnit.Days;
					bool isSeconds = (TimeUnit)(i) == TimeUnit.Seconds;

					string appender = isDays ? "0" : "00";
					string suffix = GetSuffix((TimeUnit)(unitsSelected + i));
					string delimiter = unitsSelected > 0 || isLastReplacement ? DelimiterTime : string.Empty;
					//this.Log("On step " + i + " isDays = " + isDays + " isSeconds = " + isSeconds + " last = " + isLastReplacement + " delimiter = " + delimiter);
					if (isDays) { delimiter = DefaultDelimiterDays; }
					if (isLastReplacement && isSeconds) { delimiter = string.Empty; }
					myStringBuilder.Insert(0, appender + suffix + delimiter);
				}
				//this.Log("Result append = " + myStringBuilder);
				return myStringBuilder.ToString();
			}
		}

		private bool HasEnoughUnits(int unitsSelected)
		{
			return !AutoTrim && unitsSelected == PreferredUnitsAmount;
		}

		private string GetSuffix(TimeUnit timeUnit)
		{
			switch (timeUnit)
			{
				case TimeUnit.Seconds:
					return UseSuffix ? (!UseCustomSuffix ? DelimiterSeconds : SuffixDefaultSeconds) : string.Empty;
				case TimeUnit.Minutes:
					return UseSuffix ? (!UseCustomSuffix ? DelimiterMinutes : SuffixDefaultMinutes) : string.Empty;
				case TimeUnit.Hours:
					return UseSuffix ? (!UseCustomSuffix ? DelimiterHours : SuffixDefaultHours) : string.Empty;
				case TimeUnit.Days:
					return UseSuffix ? (!UseCustomSuffix ? DelimiterDays : SuffixDefaultDays) : string.Empty;
				default:
					{
						this.LogWarning("Suffix not found for " + timeUnit);
						return string.Empty;
					}
			}
		}

		protected virtual void SetTimeToComponent(string formattedTime) { }
	}
}
