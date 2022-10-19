using System;
using System.ComponentModel;
using Microsoft.Maui.Converters;

namespace Microsoft.Maui
{
	/// <summary>
	/// Functions that modify values non-linearly, generally used for animations.
	/// </summary>
	/// <remarks>
	/// Easing functions are applied to input values in the range [0,1]. The cubic easing functions are often considered to look most natural.
	/// If developers wish to use their own easing functions, they should return a value of 0 for an input of 0 and a value of 1 for an input of 1 or the animation will have a jump.
	/// </remarks>
	[TypeConverter(typeof(EasingTypeConverter))]
	public class Easing
	{
		/// <summary>
		/// The default easing function that is used.
		/// Defaults to <see cref="CubicInOut"/>.
		/// </summary>
		public static Easing Default => CubicInOut;

		/// <summary>
		/// Linear transformation.
		/// </summary>
		public static readonly Easing Linear = new(x => x);

		/// <summary>
		/// Smoothly decelerates.
		/// </summary>
		public static readonly Easing SinOut = new(x => Math.Sin(x * Math.PI * 0.5f));

		/// <summary>
		/// Smoothly accelerates.
		/// </summary>
		public static readonly Easing SinIn = new(x => 1.0f - Math.Cos(x * Math.PI * 0.5f));

		/// <summary>
		/// Accelerates in and decelerates out.
		/// </summary>
		public static readonly Easing SinInOut = new(x => -Math.Cos(Math.PI * x) / 2.0f + 0.5f);

		/// <summary>
		/// Starts slowly and accelerates.
		/// </summary>
		public static readonly Easing CubicIn = new(x => x * x * x);

		/// <summary>
		/// Starts quickly and the decelerates.
		/// </summary>
		public static readonly Easing CubicOut = new(x => Math.Pow(x - 1.0f, 3.0f) + 1.0f);

		/// <summary>
		/// Accelerates and decelerates. Often a natural-looking choice.
		/// </summary>
		public static readonly Easing CubicInOut = new(x => x < 0.5f ? Math.Pow(x * 2.0f, 3.0f) / 2.0f : (Math.Pow((x - 1) * 2.0f, 3.0f) + 2.0f) / 2.0f);

		/// <summary>
		/// Leaps to final values, bounces 3 times, and settles.
		/// </summary>
		public static readonly Easing BounceOut;

		/// <summary>
		/// Jumps towards, and then bounces as it settles at the final value.
		/// </summary>
		public static readonly Easing BounceIn;

		/// <summary>
		/// Moves away and then leaps toward the final value.
		/// </summary>
		public static readonly Easing SpringIn = new(x => x * x * ((1.70158f + 1) * x - 1.70158f));

		/// <summary>
		/// Overshoots and then returns.
		/// </summary>
		public static readonly Easing SpringOut = new(x => (x - 1) * (x - 1) * ((1.70158f + 1) * (x - 1) + 1.70158f) + 1);

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

		/// <summary>
		/// Creates a new <see cref="Easing" /> object with the <paramref name="easingFunc" /> function.
		/// </summary>
		/// <param name="easingFunc">A function that maps animation times.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="easingFunc"/> is <see langword="null"/>.</exception>
		public Easing(Func<double, double> easingFunc)
		{
			_easingFunc = easingFunc ?? throw new ArgumentNullException(nameof(easingFunc));
		}

		/// <summary>
		/// Applies the easing function to the specified value <paramref name="v" />.
		/// </summary>
		/// <param name="v">A value in the range [0,1] to which the easing function should be applied.</param>
		/// <returns>The value of the easing function when applied to the value <paramref name="v" />.</returns>
		public double Ease(double v)
		{
			return _easingFunc(v);
		}

		/// <summary>
		/// Converts a function into an <see cref="T:Microsoft.Maui.Easing" />.
		/// </summary>
		/// <param name="func">An easing function.</param>
		/// <remarks>An easing function should return a value of (or near) 0 at 0 and 1 (or near) for 1.</remarks>
		public static implicit operator Easing(Func<double, double> func)
		{
			return new Easing(func);
		}
	}
}