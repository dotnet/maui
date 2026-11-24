#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>Blends two values of <typeparamref name="T"/>, for animating properties of that type.</summary>
	/// <typeparam name="T">The property type.</typeparam>
	/// <remarks>Register one with <see cref="AnimationServiceCollectionExtensions.AddAnimationInterpolator{T}"/>.
	/// Types with no interpolator change at the keyframe instead of blending.</remarks>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public interface IValueInterpolator<T>
	{
		/// <summary>Returns the value <paramref name="progress"/> of the way from <paramref name="from"/> to
		/// <paramref name="to"/>. Progress can leave [0, 1] under an overshooting easing.</summary>
		T Interpolate(T from, T to, double progress);
	}

	/// <summary>Registers interpolators for custom property types.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public static class AnimationServiceCollectionExtensions
	{
		/// <summary>Makes properties of type <typeparamref name="T"/> blend with <paramref name="interpolator"/>.</summary>
		/// <remarks>The registry is read once, the first time a storyboard compiles in the app, and is immutable
		/// afterwards. A registration replaces a built-in one for the same type.</remarks>
		public static IServiceCollection AddAnimationInterpolator<T>(this IServiceCollection services, IValueInterpolator<T> interpolator)
		{
			if (services is null)
				throw new ArgumentNullException(nameof(services));
			if (interpolator is null)
				throw new ArgumentNullException(nameof(interpolator));

			// The closed generic factory is captured here, where T is statically known; lookup later is by Type.
			services.AddSingleton(new AnimationInterpolatorRegistration(typeof(T), new InterpolatorEntry<T>(interpolator)));
			return services;
		}
	}

	internal sealed class AnimationInterpolatorRegistration
	{
		public AnimationInterpolatorRegistration(Type type, InterpolatorEntry entry)
		{
			Type = type;
			Entry = entry;
		}

		public Type Type { get; }

		public InterpolatorEntry Entry { get; }
	}

	/// <summary>A closed generic track factory for one property type.</summary>
	internal abstract class InterpolatorEntry
	{
		public abstract CompiledTrack CreateTrack(BindableObject target, BindableProperty property, IReadOnlyList<TrackSample> samples, Easing? defaultEasing);

		/// <summary>Whether values blend (false for the hold fallback).</summary>
		public virtual bool Blends => true;
	}

	internal sealed class InterpolatorEntry<T> : InterpolatorEntry
	{
		readonly IValueInterpolator<T> _interpolator;
		readonly bool _blends;

		public InterpolatorEntry(IValueInterpolator<T> interpolator, bool blends = true)
		{
			_interpolator = interpolator;
			_blends = blends;
		}

		public override bool Blends => _blends;

		public override CompiledTrack CreateTrack(BindableObject target, BindableProperty property, IReadOnlyList<TrackSample> samples, Easing? defaultEasing)
			=> new CompiledTrack<T>(target, property, samples, defaultEasing, _interpolator);
	}

	/// <summary>The interpolators the engine uses, looked up by the property's return type.</summary>
	internal static class AnimationInterpolators
	{
		static readonly Dictionary<Type, InterpolatorEntry> s_builtIn = CreateBuiltIn();
		static readonly InterpolatorEntry s_hold = new InterpolatorEntry<object?>(HoldInterpolator<object?>.Instance, blends: false);

		static IServiceProvider? s_registeredFrom;
		static Dictionary<Type, InterpolatorEntry>? s_registered;

		public static InterpolatorEntry Get(Type type, IServiceProvider? services)
		{
			var registered = GetRegistered(services);
			if (registered is not null && registered.TryGetValue(type, out var custom))
				return custom;

			return s_builtIn.TryGetValue(type, out var entry) ? entry : s_hold;
		}

		static Dictionary<Type, InterpolatorEntry>? GetRegistered(IServiceProvider? services)
		{
			if (services is null)
				return null;

			if (ReferenceEquals(services, s_registeredFrom))
				return s_registered;

			Dictionary<Type, InterpolatorEntry>? registered = null;
			foreach (var registration in services.GetServices<AnimationInterpolatorRegistration>())
			{
				registered ??= new Dictionary<Type, InterpolatorEntry>();
				registered[registration.Type] = registration.Entry;
			}

			s_registeredFrom = services;
			s_registered = registered;
			return registered;
		}

		static Dictionary<Type, InterpolatorEntry> CreateBuiltIn() => new()
		{
			[typeof(double)] = new InterpolatorEntry<double>(Numeric.Instance),
			[typeof(float)] = new InterpolatorEntry<float>(Numeric.Instance),
			[typeof(int)] = new InterpolatorEntry<int>(Numeric.Instance),
			[typeof(long)] = new InterpolatorEntry<long>(Numeric.Instance),
			[typeof(short)] = new InterpolatorEntry<short>(Numeric.Instance),
			[typeof(byte)] = new InterpolatorEntry<byte>(Numeric.Instance),
			[typeof(uint)] = new InterpolatorEntry<uint>(Numeric.Instance),
			[typeof(TimeSpan)] = new InterpolatorEntry<TimeSpan>(Numeric.Instance),
			[typeof(Color)] = new InterpolatorEntry<Color?>(ColorInterpolator.Instance),
			[typeof(Point)] = new InterpolatorEntry<Point>(Geometry.Instance),
			[typeof(PointF)] = new InterpolatorEntry<PointF>(Geometry.Instance),
			[typeof(Size)] = new InterpolatorEntry<Size>(Geometry.Instance),
			[typeof(SizeF)] = new InterpolatorEntry<SizeF>(Geometry.Instance),
			[typeof(Rect)] = new InterpolatorEntry<Rect>(Geometry.Instance),
			[typeof(RectF)] = new InterpolatorEntry<RectF>(Geometry.Instance),
			[typeof(Thickness)] = new InterpolatorEntry<Thickness>(Geometry.Instance),
			[typeof(CornerRadius)] = new InterpolatorEntry<CornerRadius>(Geometry.Instance),
			[typeof(GridLength)] = new InterpolatorEntry<GridLength>(Geometry.Instance),
			[typeof(Brush)] = new InterpolatorEntry<Brush?>(BrushInterpolator.Instance),
			[typeof(SolidColorBrush)] = new InterpolatorEntry<SolidColorBrush?>(BrushInterpolator.Instance),
			[typeof(DoubleCollection)] = new InterpolatorEntry<DoubleCollection?>(DoubleCollectionInterpolator.Instance),
		};

		static double Lerp(double from, double to, double progress) => from + (to - from) * progress;

		// Integers round half away from zero and clamp to the type's range: overshooting easings leave the endpoints.
		static long Round(double value, long min, long max)
		{
			var rounded = Math.Round(value, MidpointRounding.AwayFromZero);
			if (double.IsNaN(rounded))
				return 0;
			if (rounded <= min)
				return min;
			if (rounded >= max)
				return max;
			return (long)rounded;
		}

		// 64-bit values lose precision in a double above 2^53, so they interpolate in decimal (28-29 significant digits,
		// enough for any long difference), rounding half away from zero and clamping to the type's range.
		static long Lerp64(long from, long to, double progress)
		{
			if (double.IsNaN(progress))
				return from;

			decimal value;
			try
			{
				value = from + ((decimal)to - from) * (decimal)progress;
			}
			catch (OverflowException)
			{
				return progress < 0 ? (to >= from ? long.MinValue : long.MaxValue) : (to >= from ? long.MaxValue : long.MinValue);
			}

			value = Math.Round(value, MidpointRounding.AwayFromZero);
			if (value <= long.MinValue)
				return long.MinValue;
			if (value >= long.MaxValue)
				return long.MaxValue;
			return (long)value;
		}

		sealed class Numeric : IValueInterpolator<double>, IValueInterpolator<float>, IValueInterpolator<int>, IValueInterpolator<long>,
			IValueInterpolator<short>, IValueInterpolator<byte>, IValueInterpolator<uint>, IValueInterpolator<TimeSpan>
		{
			public static readonly Numeric Instance = new();

			public double Interpolate(double from, double to, double progress) => Lerp(from, to, progress);

			public float Interpolate(float from, float to, double progress) => (float)Lerp(from, to, progress);

			public int Interpolate(int from, int to, double progress) => (int)Round(Lerp(from, to, progress), int.MinValue, int.MaxValue);

			public long Interpolate(long from, long to, double progress) => Lerp64(from, to, progress);

			public short Interpolate(short from, short to, double progress) => (short)Round(Lerp(from, to, progress), short.MinValue, short.MaxValue);

			public byte Interpolate(byte from, byte to, double progress) => (byte)Round(Lerp(from, to, progress), byte.MinValue, byte.MaxValue);

			public uint Interpolate(uint from, uint to, double progress) => (uint)Round(Lerp(from, to, progress), uint.MinValue, uint.MaxValue);

			public TimeSpan Interpolate(TimeSpan from, TimeSpan to, double progress)
				=> TimeSpan.FromTicks(Lerp64(from.Ticks, to.Ticks, progress));
		}

		sealed class Geometry : IValueInterpolator<Point>, IValueInterpolator<PointF>, IValueInterpolator<Size>, IValueInterpolator<SizeF>,
			IValueInterpolator<Rect>, IValueInterpolator<RectF>, IValueInterpolator<Thickness>, IValueInterpolator<CornerRadius>, IValueInterpolator<GridLength>
		{
			public static readonly Geometry Instance = new();

			public Point Interpolate(Point from, Point to, double p) => new(Lerp(from.X, to.X, p), Lerp(from.Y, to.Y, p));

			public PointF Interpolate(PointF from, PointF to, double p) => new((float)Lerp(from.X, to.X, p), (float)Lerp(from.Y, to.Y, p));

			public Size Interpolate(Size from, Size to, double p) => new(Lerp(from.Width, to.Width, p), Lerp(from.Height, to.Height, p));

			public SizeF Interpolate(SizeF from, SizeF to, double p) => new((float)Lerp(from.Width, to.Width, p), (float)Lerp(from.Height, to.Height, p));

			public Rect Interpolate(Rect from, Rect to, double p)
				=> new(Lerp(from.X, to.X, p), Lerp(from.Y, to.Y, p), Lerp(from.Width, to.Width, p), Lerp(from.Height, to.Height, p));

			public RectF Interpolate(RectF from, RectF to, double p)
				=> new((float)Lerp(from.X, to.X, p), (float)Lerp(from.Y, to.Y, p), (float)Lerp(from.Width, to.Width, p), (float)Lerp(from.Height, to.Height, p));

			public Thickness Interpolate(Thickness from, Thickness to, double p)
				=> new(Lerp(from.Left, to.Left, p), Lerp(from.Top, to.Top, p), Lerp(from.Right, to.Right, p), Lerp(from.Bottom, to.Bottom, p));

			public CornerRadius Interpolate(CornerRadius from, CornerRadius to, double p)
				=> new(Lerp(from.TopLeft, to.TopLeft, p), Lerp(from.TopRight, to.TopRight, p), Lerp(from.BottomLeft, to.BottomLeft, p), Lerp(from.BottomRight, to.BottomRight, p));

			// Same unit only; otherwise the value holds until the keyframe.
			public GridLength Interpolate(GridLength from, GridLength to, double p)
			{
				if (from.GridUnitType != to.GridUnitType || from.IsAuto)
					return from;

				return new GridLength(Math.Max(0, Lerp(from.Value, to.Value, p)), from.GridUnitType);
			}
		}

		/// <summary>Component-wise in premultiplied-alpha sRGB, so Transparent → White doesn't pass through grey.</summary>
		internal sealed class ColorInterpolator : IValueInterpolator<Color?>
		{
			public static readonly ColorInterpolator Instance = new();

			public Color? Interpolate(Color? from, Color? to, double p)
			{
				if (from is null || to is null)
					return from;

				var alpha = Lerp(from.Alpha, to.Alpha, p);
				if (alpha <= 0)
					return new Color(to.Red, to.Green, to.Blue, 0f);

				var red = Lerp(from.Red * from.Alpha, to.Red * to.Alpha, p) / alpha;
				var green = Lerp(from.Green * from.Alpha, to.Green * to.Alpha, p) / alpha;
				var blue = Lerp(from.Blue * from.Alpha, to.Blue * to.Alpha, p) / alpha;

				return new Color((float)red, (float)green, (float)blue, (float)Math.Min(1.0, alpha));
			}
		}

		/// <summary>Two solid brushes blend their colors (one new brush per frame); anything else holds.</summary>
		sealed class BrushInterpolator : IValueInterpolator<Brush?>, IValueInterpolator<SolidColorBrush?>
		{
			public static readonly BrushInterpolator Instance = new();

			public Brush? Interpolate(Brush? from, Brush? to, double p)
			{
				if (from is SolidColorBrush fromSolid && to is SolidColorBrush toSolid)
					return Interpolate(fromSolid, toSolid, p);

				return from;
			}

			public SolidColorBrush? Interpolate(SolidColorBrush? from, SolidColorBrush? to, double p)
			{
				if (from is null || to is null)
					return from;

				return new SolidColorBrush(ColorInterpolator.Instance.Interpolate(from.Color, to.Color, p));
			}
		}

		/// <summary>Element-wise when the lengths match (for example StrokeDashArray); otherwise holds.</summary>
		sealed class DoubleCollectionInterpolator : IValueInterpolator<DoubleCollection?>
		{
			public static readonly DoubleCollectionInterpolator Instance = new();

			public DoubleCollection? Interpolate(DoubleCollection? from, DoubleCollection? to, double p)
			{
				if (from is null || to is null || from.Count != to.Count)
					return from;

				var values = new double[from.Count];
				for (var i = 0; i < values.Length; i++)
					values[i] = Lerp(from[i], to[i], p);

				return new DoubleCollection(values);
			}
		}
	}

	/// <summary>
	/// Values that don't blend change at the keyframe. A hold always returns the segment's start value: the track
	/// writes the destination itself once the keyframe is reached, so an overshooting easing can't switch it early.
	/// The conditional holds above (mismatched units, non-solid brushes, collections of different lengths) follow the
	/// same rule.
	/// </summary>
	internal sealed class HoldInterpolator<T> : IValueInterpolator<T>
	{
		public static readonly HoldInterpolator<T> Instance = new();

		public T Interpolate(T from, T to, double progress) => from;
	}
}
