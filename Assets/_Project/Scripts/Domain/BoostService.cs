using System;

namespace Game.Domain
{
    public class BoostService
    {
        private bool _isActive;
        private float _remainingSeconds;
        private float _duration;
        private float _multiplier;
        private bool _enabled;

        public bool IsActive 
        { 
            get => _isActive; 
            private set 
            {
                _isActive = value;
                if (_isActive)
                    BoostStarted?.Invoke();
                else
                    BoostFinished?.Invoke();
            }
        }

        public float RemainingSeconds 
        { 
            get => _remainingSeconds; 
            private set => _remainingSeconds = value; 
        }

        public event Action BoostStarted;
        public event Action BoostFinished;

        public BoostService(float duration, float multiplier, bool enabled)
        {
            _duration = duration;
            _multiplier = multiplier;
            _enabled = enabled;
            _isActive = false;
            _remainingSeconds = 0f;
        }

        public bool TryActivate()
        {
            if (!_enabled || _isActive) 
                return false;

            IsActive = true;
            RemainingSeconds = _duration;
            return true;
        }

        public void Tick(double deltaTime)
        {
            if (!_isActive) return;

            RemainingSeconds -= (float)deltaTime;
            if (RemainingSeconds <= 0)
            {
                RemainingSeconds = 0;
                IsActive = false;
            }
        }

        public float GetCurrentMultiplier()
        {
            return _isActive ? _multiplier : 1.0f;
        }
    }
}