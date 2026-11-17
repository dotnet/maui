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

		/// <summary>
		/// Holds the start value for the whole segment and jumps to the end value when the segment ends.
		/// </summary>
		public static Easing Hold { get; } = new(x => x >= 1 ? 1 : 0);

		/// <summary>
		/// Creates a cubic Bézier easing with CSS <c>cubic-bezier()</c> semantics. The curve starts at (0, 0) and ends at (1, 1).
		/// </summary>
		/// <param name="x1">The x coordinate of the first control point, in [0, 1].</param>
		/// <param name="y1">The y coordinate of the first control point. Values outside [0, 1] overshoot.</param>
		/// <param name="x2">The x coordinate of the second control point, in [0, 1].</param>
		/// <param name="y2">The y coordinate of the second control point. Values outside [0, 1] overshoot.</param>
		/// <returns>The easing.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x1"/> or <paramref name="x2"/> is outside [0, 1], or a value is not finite.</exception>
		public static Easing CubicBezier(double x1, double y1, double x2, double y2)
		{
			if (!(x1 >= 0 && x1 <= 1))
				throw new ArgumentOutOfRangeException(nameof(x1));
			if (!(x2 >= 0 && x2 <= 1))
				throw new ArgumentOutOfRangeException(nameof(x2));
			if (double.IsNaN(y1) || double.IsInfinity(y1))
				throw new ArgumentOutOfRangeException(nameof(y1));
			if (double.IsNaN(y2) || double.IsInfinity(y2))
				throw new ArgumentOutOfRangeException(nameof(y2));

			var curve = new CubicBezierCurve(x1, y1, x2, y2);
			return new Easing(curve.Evaluate);
		}

		/// <summary>
		/// Creates a spring-shaped easing: the step response of a damped oscillator that settles to within 0.1%
		/// of the end value exactly when the segment ends. It is not a physics simulation and carries no velocity.
		/// </summary>
		/// <param name="bounce">How much the curve overshoots, in [0, 0.95]. 0 settles without overshoot.</param>
		/// <returns>The easing.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bounce"/> is outside [0, 0.95].</exception>
		public static Easing Spring(double bounce = 0.2)
		{
			if (!(bounce >= 0 && bounce <= 0.95))
				throw new ArgumentOutOfRangeException(nameof(bounce));

			var zeta = 1 - bounce;
			if (bounce == 0)
			{
				// Critically damped: omega is the root of e^(-omega) * (1 + omega) = 0.001.
				const double omega = 9.2335;
				return new Easing(t => t >= 1 ? 1 : 1 - Math.Exp(-omega * t) * (1 + omega * t));
			}

			// sqrt(1 - zeta^2), computed without cancellation: for a tiny bounce, 1 - bounce rounds to 1.
			var damped = Math.Sqrt(bounce * (2 - bounce));
			var naturalFrequency = Math.Log(1000 / damped) / zeta;
			var dampedFrequency = naturalFrequency * damped;
			var sineFactor = zeta * naturalFrequency / dampedFrequency;

			return new Easing(t => t >= 1
				? 1
				: 1 - Math.Exp(-zeta * naturalFrequency * t) * (Math.Cos(dampedFrequency * t) + sineFactor * Math.Sin(dampedFrequency * t)));
		}

		/// <summary>
		/// Creates a stepped easing with CSS <c>steps()</c> semantics.
		/// </summary>
		/// <param name="count">The number of steps. Must be at least 1.</param>
		/// <param name="jumpAtStart"><see langword="true"/> for <c>jump-start</c>: the first step happens at the start. Otherwise <c>jump-end</c>.</param>
		/// <returns>The easing.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1.</exception>
		public static Easing Steps(int count, bool jumpAtStart = false)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count));

			return jumpAtStart
				? new Easing(t => t >= 1 ? 1 : Math.Min(1, Math.Floor(t * count + 1) / count))
				: new Easing(t => t >= 1 ? 1 : Math.Floor(t * count) / count);
		}

		sealed class CubicBezierCurve
		{
			readonly double _ax, _bx, _cx, _ay, _by, _cy;

			public CubicBezierCurve(double x1, double y1, double x2, double y2)
			{
				_cx = 3 * x1;
				_bx = 3 * (x2 - x1) - _cx;
				_ax = 1 - _cx - _bx;
				_cy = 3 * y1;
				_by = 3 * (y2 - y1) - _cy;
				_ay = 1 - _cy - _by;
			}

			public double Evaluate(double x)
			{
				if (x <= 0)
					return 0;
				if (x >= 1)
					return 1;

				return Sample(_ay, _by, _cy, SolveForT(x));
			}

			static double Sample(double a, double b, double c, double t) => ((a * t + b) * t + c) * t;

			double SolveForT(double x)
			{
				// Newton-Raphson converges in a few steps where x changes with t. Where the curve is nearly flat
				// in x, a tiny error in x is a large error in t and so in the output: convergence is judged on t,
				// and bisection, which x's monotonicity makes safe, takes over near flat spots.
				var t = x;
				for (var i = 0; i < 8; i++)
				{
					var slope = (3 * _ax * t + 2 * _bx) * t + _cx;
					if (Math.Abs(slope) < 1e-3)
						break;

					var step = (Sample(_ax, _bx, _cx, t) - x) / slope;
					t -= step;
					if (t < 0 || t > 1)
						break;

					if (Math.Abs(step) < 1e-12)
						return t;
				}

				double low = 0, high = 1;
				while (high - low > 1e-12)
				{
					t = (low + high) / 2;
					if (Sample(_ax, _bx, _cx, t) < x)
						low = t;
					else
						high = t;
				}

				return (low + high) / 2;
			}
		}

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