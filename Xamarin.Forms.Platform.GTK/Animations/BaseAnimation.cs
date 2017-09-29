using System;
using System.Threading.Tasks;
using System.Timers;

namespace Xamarin.Forms.Platform.GTK.Animations
{
    internal abstract class BaseAnimation
    {
        private const double AnimationInterval = 1000 / 60.0;

        private TimeSpan _totalTime;
        private Task _animTask;
        private Timer _timer;
        private DateTime _startTime;
        private bool _isEased;
        private double _elapsed;
        private double _lerp;

        public BaseAnimation(TimeSpan time, bool isEased)
        {
            _totalTime = time;
            _isEased = isEased;

            _timer = new Timer();
            _timer.Interval = AnimationInterval;
            _timer.Elapsed += OnTimerElapsed;
        }

        protected abstract void AnimationStep(double lerp);

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _elapsed = (e.SignalTime - _startTime).TotalMilliseconds;

            _lerp = _elapsed / _totalTime.TotalMilliseconds;

            if (_isEased)
            {
                _lerp = SmoothLerp(0, 1, (float)_lerp);
            }

            AnimationStep(_lerp);
        }

        protected static float SmoothLerp(float value1, float value2, float amount)
        {
            float num = Clamp(amount, 0f, 1f);

            return Lerp(value1, value2, (num * num) * (3f - (2f * num)));
        }

        protected static float Clamp(float value, float min, float max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;

            return value;
        }

        protected static float Lerp(float value1, float value2, float amount)
        {
            return value1 + ((value2 - value1) * amount);
        }

        public async Task Run()
        {
            if (_animTask != null)
            {
                // TODO: Cancel or throw exception
            }

            _startTime = DateTime.Now;
            _timer.Start();
            _animTask = Task.Delay(_totalTime);
            await _animTask;

            if (_animTask.Status == TaskStatus.RanToCompletion)
            {
                _timer.Stop();

                AnimationStep(1);
            }

            _animTask = null;
        }
    }
}
