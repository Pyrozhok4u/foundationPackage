using System;
using Foundation.ServicesResolver;
using Foundation.TimerUtils;

public interface ITimerService : IService
{
	Timer StartRecurringTimer(float duration, Action onComplete, bool slowUpdate = false, Action<float> onTick = null);
	Timer StartRecurringTimerMs(long duration, Action onComplete, bool slowUpdate = false, Action<float> onTick = null);
	Timer StartTimer(float duration, Action onComplete, bool slowUpdate = false, Action<float> onTick = null);
	Timer StartTimerMs(long duration, Action onComplete, bool slowUpdate = false, Action<float> onTick = null);
	Timer StartPerSecondTimer(float timerDuration, Action onTimerFinished, Action<float> onTimerTick = null, bool recurring = false);
	void StopTimer(Timer timer);
}
