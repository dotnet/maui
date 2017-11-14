using System;

namespace Xamarin.Forms.Platform.GTK.Animations
{
    internal class FloatAnimation : BaseAnimation
    {
        private float _from;
        private float _to;
        private Action<float> _callback;

        public FloatAnimation(float from, float to, TimeSpan time, bool isEased, Action<float> callback)
            : base(time, isEased)
        {
            _from = from;
            _to = to;
            _callback = callback;
        }

        protected override void AnimationStep(double lerp)
        {
            _callback(Lerp(_from, _to, (float)lerp));
        }
    }
}