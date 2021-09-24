using System;

namespace Microsoft.Maui
{
	public class Easing
	{
		public static Easing Default => CubicInOut;
		public static readonly Easing Linear = new Easing(x => x);

		public static readonly Easing SinOut = new Easing(x => Math.Sin(x * Math.PI * 0.5f));
		public static readonly Easing SinIn = new Easing(x => 1.0f - Math.Cos(x * Math.PI * 0.5f));
		public static readonly Easing SinInOut = new Easing(x => -Math.Cos(Math.PI * x) / 2.0f + 0.5f);

		public static readonly Easing CubicIn = new Easing(x => x * x * x);
		public static readonly Easing CubicOut = new Easing(x => Math.Pow(x - 1.0f, 3.0f) + 1.0f);

		public static readonly Easing CubicInOut = new Easing(x => x < 0.5f ? Math.Pow(x * 2.0f, 3.0f) / 2.0f : (Math.Pow((x - 1) * 2.0f, 3.0f) + 2.0f) / 2.0f);

		public static readonly Easing BounceOut;
		public static readonly Easing BounceIn;

		public static readonly Easing SpringIn = new Easing(x => x * x * ((1.70158f + 1) * x - 1.70158f));
		public static readonly Easing SpringOut = new Easing(x => (x - 1) * (x - 1) * ((1.70158f + 1) * (x - 1) + 1.70158f) + 1);

		readonly Func<double, double> _easingFunc;

		static Easing()
		{
			BounceOut = new Easing(p =>
			{
				if (p < 1 / 2.75f)
				{
					return 7.5625f * p * p;
				}
				if (p < 2 / 2.75f)
				{
					p -= 1.5f / 2.75f;

					return 7.5625f * p * p + .75f;
				}
				if (p < 2.5f / 2.75f)
				{
					p -= 2.25f / 2.75f;

					return 7.5625f * p * p + .9375f;
				}
				p -= 2.625f / 2.75f;

				return 7.5625f * p * p + .984375f;
			});

			BounceIn = new Easing(p => 1.0f - BounceOut.Ease(1 - p));
		}

		public Easing(Func<double, double> easingFunc)
		{
			if (easingFunc == null)
				throw new ArgumentNullException("easingFunc");

			_easingFunc = easingFunc;
		}

		public double Ease(double v)
		{
			return _easingFunc(v);
		}

		public static implicit operator Easing(Func<double, double> func)
		{
			return new Easing(func);
		}
	}
}