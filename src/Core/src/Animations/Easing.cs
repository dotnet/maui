using System;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="Type[@FullName='Microsoft.Maui.Easing']/Docs" />
	public class Easing
	{
		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='Default']/Docs" />
		public static Easing Default => CubicInOut;
		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='Linear']/Docs" />
		public static readonly Easing Linear = new Easing(x => x);

		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='SinOut']/Docs" />
		public static readonly Easing SinOut = new Easing(x => Math.Sin(x * Math.PI * 0.5f));
		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='SinIn']/Docs" />
		public static readonly Easing SinIn = new Easing(x => 1.0f - Math.Cos(x * Math.PI * 0.5f));
		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='SinInOut']/Docs" />
		public static readonly Easing SinInOut = new Easing(x => -Math.Cos(Math.PI * x) / 2.0f + 0.5f);

		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='CubicIn']/Docs" />
		public static readonly Easing CubicIn = new Easing(x => x * x * x);
		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='CubicOut']/Docs" />
		public static readonly Easing CubicOut = new Easing(x => Math.Pow(x - 1.0f, 3.0f) + 1.0f);

		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='CubicInOut']/Docs" />
		public static readonly Easing CubicInOut = new Easing(x => x < 0.5f ? Math.Pow(x * 2.0f, 3.0f) / 2.0f : (Math.Pow((x - 1) * 2.0f, 3.0f) + 2.0f) / 2.0f);

		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='BounceOut']/Docs" />
		public static readonly Easing BounceOut;
		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='BounceIn']/Docs" />
		public static readonly Easing BounceIn;

		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='SpringIn']/Docs" />
		public static readonly Easing SpringIn = new Easing(x => x * x * ((1.70158f + 1) * x - 1.70158f));
		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='SpringOut']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public Easing(Func<double, double> easingFunc)
		{
			if (easingFunc == null)
				throw new ArgumentNullException("easingFunc");

			_easingFunc = easingFunc;
		}

		/// <include file="../../docs/Microsoft.Maui/Easing.xml" path="//Member[@MemberName='Ease']/Docs" />
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