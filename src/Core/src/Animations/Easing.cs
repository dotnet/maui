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
		/// Holds the start value for the whole segment, then jumps to the end value as the segment ends.
		/// </summary>
		/// <remarks>In a keyframe animation, the value changes at the keyframe whose easing is <see cref="Hold"/>.</remarks>
		public static readonly Easing Hold = new(x => x >= 1.0 ? 1.0 : 0.0) { _syntax = nameof(Hold) };

		readonly Func<double, double> _easingFunc;

		// The canonical text of a curve built by a factory, so it round-trips through the type converter.
		string? _syntax;

		internal string? Syntax => _syntax;

		/// <summary>
		/// Creates a CSS-compatible cubic Bézier curve from (0, 0) to (1, 1) with the control points
		/// (<paramref name="x1"/>, <paramref name="y1"/>) and (<paramref name="x2"/>, <paramref name="y2"/>).
		/// </summary>
		/// <param name="x1">The time component of the first control point, between 0 and 1.</param>
		/// <param name="y1">The progress component of the first control point. Values outside [0, 1] overshoot.</param>
		/// <param name="x2">The time component of the second control point, between 0 and 1.</param>
		/// <param name="y2">The progress component of the second control point. Values outside [0, 1] overshoot.</param>
		/// <returns>The curve. A curve copied from CSS, a design tool or browser developer tools looks the same.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x1"/> or <paramref name="x2"/> is outside [0, 1].</exception>
		public static Easing CubicBezier(double x1, double y1, double x2, double y2)
		{
			if (double.IsNaN(x1) || x1 < 0 || x1 > 1)
				throw new ArgumentOutOfRangeException(nameof(x1), x1, "x1 must be between 0 and 1.");
			if (double.IsNaN(x2) || x2 < 0 || x2 > 1)
				throw new ArgumentOutOfRangeException(nameof(x2), x2, "x2 must be between 0 and 1.");
			if (double.IsNaN(y1) || double.IsInfinity(y1))
				throw new ArgumentOutOfRangeException(nameof(y1), y1, "y1 must be a finite number.");
			if (double.IsNaN(y2) || double.IsInfinity(y2))
				throw new ArgumentOutOfRangeException(nameof(y2), y2, "y2 must be a finite number.");

			var curve = new CubicBezierCurve(x1, y1, x2, y2);
			return new Easing(curve.Ease) { _syntax = EasingSyntax.FormatCubicBezier(x1, y1, x2, y2) };
		}

		/// <summary>
		/// Creates a spring-shaped curve: the step response of a damped oscillator, settled at the end of the segment.
		/// </summary>
		/// <param name="bounce">How much the value overshoots, from 0 (critically damped, no overshoot) to 0.95.</param>
		/// <returns>The curve. Its damping ratio is <c>1 - bounce</c>; it reaches exactly 1 at the end.</returns>
		/// <remarks>This is a spring-shaped easing, not a physics simulation: velocity is not carried across an
		/// interruption. It is unrelated to <see cref="SpringIn"/> and <see cref="SpringOut"/>, which are back easings.</remarks>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bounce"/> is outside [0, 0.95].</exception>
		public static Easing Spring(double bounce = EasingSyntax.DefaultSpringBounce)
		{
			if (double.IsNaN(bounce) || bounce < 0 || bounce > 0.95)
				throw new ArgumentOutOfRangeException(nameof(bounce), bounce, "bounce must be between 0 and 0.95.");

			return new Easing(CreateSpring(bounce)) { _syntax = EasingSyntax.FormatSpring(bounce) };
		}

		/// <summary>
		/// Creates a staircase curve with <paramref name="count"/> equal steps (CSS <c>steps()</c>).
		/// </summary>
		/// <param name="count">The number of steps. Must be at least 1.</param>
		/// <param name="jumpAtStart"><see langword="true"/> to jump at the start of each step (CSS <c>jump-start</c>);
		/// <see langword="false"/> to jump at its end (CSS <c>jump-end</c>, the default).</param>
		/// <returns>The curve.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1.</exception>
		public static Easing Steps(int count, bool jumpAtStart = false)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count), count, "count must be at least 1.");

			Func<double, double> steps = jumpAtStart
				? x => x < 0 ? 0 : x >= 1 ? 1 : Math.Min(1.0, Math.Floor(x * count + 1) / count)
				: x => x <= 0 ? 0 : x >= 1 ? 1 : Math.Floor(x * count) / count;

			return new Easing(steps) { _syntax = EasingSyntax.FormatSteps(count, jumpAtStart) };
		}

		static Func<double, double> CreateSpring(double bounce)
		{
			// Below 0.01 the settling criterion diverges (omega grows with ln(1/sqrt(1 - zeta^2))), so treat it as
			// critically damped, which keeps the family continuous to within about 3%.
			if (bounce < 0.01)
			{
				const double criticalOmega = 9.2335; // root of e^-w (1 + w) = 0.001
				return t => t <= 0 ? 0 : t >= 1 ? 1 : 1 - Math.Exp(-criticalOmega * t) * (1 + criticalOmega * t);
			}

			var zeta = 1 - bounce;
			// sqrt(1 - zeta^2) computed without cancellation: 1 - (1 - b)^2 = b (2 - b).
			var root = Math.Sqrt(bounce * (2 - bounce));
			var omega = Math.Log(1000 / root) / zeta;
			var dampedOmega = omega * root;
			var ratio = zeta * omega / dampedOmega;

			return t => t <= 0 ? 0 : t >= 1 ? 1
				: 1 - Math.Exp(-zeta * omega * t) * (Math.Cos(dampedOmega * t) + ratio * Math.Sin(dampedOmega * t));
		}

		sealed class CubicBezierCurve
		{
			const int NewtonIterations = 8;
			const double NewtonMinSlope = 1e-6;
			const double Epsilon = 1e-9;
			const int MaxBisections = 64;

			readonly double _ax, _bx, _cx, _ay, _by, _cy;
			readonly bool _isLinear;

			public CubicBezierCurve(double x1, double y1, double x2, double y2)
			{
				// Polynomial form of the Bezier with P0 = (0, 0) and P3 = (1, 1).
				_cx = 3 * x1;
				_bx = 3 * (x2 - x1) - _cx;
				_ax = 1 - _cx - _bx;
				_cy = 3 * y1;
				_by = 3 * (y2 - y1) - _cy;
				_ay = 1 - _cy - _by;
				_isLinear = x1 == y1 && x2 == y2;
			}

			public double Ease(double x)
			{
				if (x <= 0)
					return 0;
				if (x >= 1)
					return 1;
				if (_isLinear)
					return x;

				return SampleY(SolveForParameter(x));
			}

			double SampleX(double t) => ((_ax * t + _bx) * t + _cx) * t;
			double SampleY(double t) => ((_ay * t + _by) * t + _cy) * t;
			double SampleSlopeX(double t) => (3 * _ax * t + 2 * _bx) * t + _cx;

			// Convergence is judged on the parameter, not on x: near a flat spot in x a tiny error in x is a large
			// error in the output. Newton first; bisection, which always converges because x is monotonic, otherwise.
			double SolveForParameter(double x)
			{
				var t = x;
				for (var i = 0; i < NewtonIterations; i++)
				{
					var slope = SampleSlopeX(t);
					if (Math.Abs(slope) < NewtonMinSlope)
						break;

					var next = t - (SampleX(t) - x) / slope;
					if (next < 0 || next > 1)
						break;

					if (Math.Abs(next - t) < Epsilon)
						return next;

					t = next;
				}

				double lo = 0, hi = 1;
				t = x;
				for (var i = 0; i < MaxBisections && hi - lo > Epsilon; i++)
				{
					if (SampleX(t) < x)
						lo = t;
					else
						hi = t;

					t = (lo + hi) / 2;
				}

				return t;
			}
		}

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