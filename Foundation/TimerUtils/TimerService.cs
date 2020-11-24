using System;
using System.Collections.Generic;
using Foundation.MonoUtils;
using Foundation.ServicesResolver;

namespace Foundation.TimerUtils
{
    public class TimerService : BaseService, ITimerService
    {
        public enum Mode { Update, PerSecondUpdate, SlowUpdate}

        private List<Timer> _timers;
        private List<Timer> _availableTimers;
        private MonoService _monoService;

        protected override void Initialize()
        {
            int initialTimersPool = GetConfig<TimerConfig>().InitialTimersPool;
            _monoService = ServiceResolver.Resolve<MonoService>();

            _timers = new List<Timer>(initialTimersPool);
            _availableTimers = new List<Timer>(initialTimersPool);
            for (int i = 0; i < initialTimersPool; i++)
            {
                _timers.Add(new Timer(_monoService, OnTimerStateChanged));
            }

            _availableTimers.AddRange(_timers);
        }

        #region Timers API (syntatic sugar for quickly starting a timer)

        /// <summary>
        /// Start a recurring timer to repeatedly execute an action every specified duration
        /// </summary>
        /// <param name="duration">The amount of time between every time the action is triggered</param>
        /// <param name="onComplete"></param>
        /// <param name="slowUpdate"></param>
        /// <param name="onTick"></param>
        /// <returns>A reference to the timer used</returns>
        public Timer StartRecurringTimer(float duration, Action onComplete, bool slowUpdate = false, Action<float> onTick = null)
        {
            return StartAvailableTimer(duration, onComplete,  GetTimerUpdateMethod(slowUpdate), onTick, true);
        }

        public Timer StartRecurringTimerMs(long duration, Action onComplete, bool slowUpdate = false, Action<float> onTick = null)
        {
            float seconds = duration * 0.001f;
            return StartAvailableTimer(seconds, onComplete,  GetTimerUpdateMethod(slowUpdate), onTick, true);
        }

        /// <summary>
        /// Start a timer to execute an action after the wanted duration
        /// </summary>
        /// <param name="duration">The amount of time before the action should be triggered</param>
        /// <param name="onComplete"></param>
        /// <param name="slowUpdate"></param>
        /// <param name="onTick"></param>
        /// <returns>A reference to the timer used</returns>
        public Timer StartTimer(float duration, Action onComplete, bool slowUpdate = false, Action<float> onTick = null)
        {
            return StartAvailableTimer(duration, onComplete,  GetTimerUpdateMethod(slowUpdate), onTick, false);
        }

        public Timer StartTimerMs(long duration, Action onComplete, bool slowUpdate = false, Action<float> onTick = null)
        {
            float seconds = duration * 0.001f;
            return StartAvailableTimer(seconds, onComplete,  GetTimerUpdateMethod(slowUpdate), onTick, false);
        }

        public Timer StartPerSecondTimer(float timerDuration, Action onTimerFinished, Action<float> onTimerTick = null, bool recurring = false)
        {
            return StartAvailableTimer(timerDuration, onTimerFinished, Mode.PerSecondUpdate, onTimerTick, recurring);
        }

        private Timer StartAvailableTimer(float timerDuration, Action onTimerFinished, Mode updateMethod, Action<float> onTimerTick, bool recurring)
        {
            Timer timer = GetAvailableTimer();
            timer.StartTimer(timerDuration, onTimerFinished, updateMethod, onTimerTick, recurring);

            return timer;
        }

        /// <summary>
        /// Stops the given timer
        /// </summary>
        /// <param name="timer"></param>
        public void StopTimer(Timer timer)
        {
            timer.StopTimer();
        }

        #endregion

        #region Helper Methods

        private Mode GetTimerUpdateMethod(bool slowUpdate)
        {
            return slowUpdate ? Mode.SlowUpdate : Mode.Update;
        }

        /// <summary>
        /// Used to monitor timer activity and return them to the pool when they are stopped by any source
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="active"></param>
        private void OnTimerStateChanged(Timer timer, bool active)
        {
            if (active) { _availableTimers.Remove(timer); }
            else { _availableTimers.Add(timer); }
        }

        private Timer GetAvailableTimer()
        {
            Timer availableTimer;
            if (_availableTimers.Count > 0)
            {
                availableTimer = _availableTimers[0];
                _availableTimers.Remove(availableTimer);
            }
            else
            {
                availableTimer = new Timer(_monoService, OnTimerStateChanged);
                _timers.Add(availableTimer);
            }

            return availableTimer;
        }

        public override void Dispose()
        {
            foreach (Timer timer in _timers)
            {
                timer.StopTimer();
            }

            _timers.Clear();
            _availableTimers.Clear();
        }

        #endregion
    }
}
