using System;
using Foundation.Logger;
using Foundation.MonoUtils;
using Mode = Foundation.TimerUtils.TimerService.Mode;

namespace Foundation.TimerUtils
{
    public class Timer
    {
        private MonoService _monoService;
        private float _timerDuration;
        private float _elapsedTime;
        private Action _onComplete;
        private Action<float> _onTick;
        private Mode _timerMode;
        private bool _recurring;
        private bool _isActive;
        private readonly Action<Timer, bool> _stateChangedAction;

        internal Timer(MonoService monoService, Action<Timer, bool> timerStateChanged)
        {
            _monoService = monoService;
            _stateChangedAction = timerStateChanged;
        }

        internal void StartTimer(float duration, Action onComplete, Mode mode, Action<float> onTick, bool recurring)
        {
            _timerDuration = duration;
            _onComplete = onComplete;
            _onTick = onTick;
            _timerMode = mode;
            _recurring = recurring;
            _elapsedTime = 0f;
            _isActive = true;
            _stateChangedAction.Invoke(this, true);
            
            // Finally, start the timer by registering to some update method
            UnpauseTimer();
        }

        internal void StopTimer()
        {
            if (!_isActive) { return; }

            _isActive = false;
            _elapsedTime = 0f;
            _onComplete = null;
            _onTick = null;
            PauseTimer();
            _stateChangedAction.Invoke(this, false);
        }

        /// <summary>
        /// Registers the timer to updates, un-pausing or starting it
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void UnpauseTimer()
        {
            switch (_timerMode)
            {
                case Mode.Update:
                    _monoService.OnUpdate.AddListener(UpdateTimer);
                    break;
                case Mode.PerSecondUpdate:
                    _monoService.OnPerSecondUpdate.AddListener(UpdateTimer);
                    break;
                case Mode.SlowUpdate:
                    _monoService.RegisterSlowUpdate(UpdateTimer);
                    break;
                default:
                    this.LogError("Mode is not supported: " + _timerMode);
                    break;
            }
        }

        /// <summary>
        /// Unregisters the timer from updates, pausing it
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void PauseTimer()
        {
            switch (_timerMode)
            {
                case Mode.Update:
                    _monoService.OnUpdate.RemoveListener(UpdateTimer);
                    break;
                case Mode.PerSecondUpdate:
                    _monoService.OnPerSecondUpdate.RemoveListener(UpdateTimer);
                    break;
                case Mode.SlowUpdate:
                    _monoService.UnregisterSlowUpdate(UpdateTimer);
                    break;
                default:
                    this.LogError("Mode is not supported: " + _timerMode);
                    break;
            }
        }

        private void UpdateTimer(float deltaTime)
        {
            // Update time passed
            _elapsedTime += deltaTime;
            _onTick?.Invoke(_timerDuration - _elapsedTime);
            
            // Check if timer is completed
            if (!(_elapsedTime >= _timerDuration)) return;
            _onComplete.Invoke();
            
            // reset if recurring or stop timer
            if (_recurring) { _elapsedTime -= _timerDuration; }
            else { StopTimer(); }
        }
    }
}
