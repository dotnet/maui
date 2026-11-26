#nullable enable
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Where a <see cref="KeyFrame"/> sits in its storyboard: a fraction of the duration (<c>"60%"</c> or
	/// <c>"0.6"</c>), a time (<c>"0:0:0.24"</c>, resolved against the duration when the player is created), or
	/// unspecified, in which case the engine places it.
	/// </summary>
	[TypeConverter(typeof(CueTypeConverter))]
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public readonly struct Cue : IEquatable<Cue>
	{
		readonly byte _kind; // 0 unspecified, 1 fraction, 2 time
		readonly double _fraction;
		readonly TimeSpan _time;

		Cue(byte kind, double fraction, TimeSpan time)
		{
			_kind = kind;
			_fraction = fraction;
			_time = time;
		}

		/// <summary>Lets the engine place the keyframe (spread evenly between its neighbours).</summary>
		public static Cue Unspecified => default;

		/// <summary>A cue at <paramref name="value"/> of the duration, from 0 to 1.</summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is outside [0, 1].</exception>
		public static Cue FromFraction(double value)
		{
			if (double.IsNaN(value) || value < 0 || value > 1)
				throw new ArgumentOutOfRangeException(nameof(value), value, "A cue must be between 0 and 1 (MAUIX2301).");

			return new Cue(1, value, default);
		}

		/// <summary>A cue at a time from the start of the storyboard.</summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
		public static Cue FromTimeSpan(TimeSpan value)
		{
			if (value < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), value, "A cue can't be negative (MAUIX2301).");

			return new Cue(2, 0, value);
		}

		/// <summary>Whether the engine places this keyframe.</summary>
		public bool IsUnspecified => _kind == 0;

		/// <summary>Whether the cue is a time rather than a fraction.</summary>
		public bool HasTimeSpan => _kind == 2;

		/// <summary>The fraction of the duration.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the cue is a time or unspecified.</exception>
		public double Fraction => _kind == 1 ? _fraction : throw new InvalidOperationException("This cue is not a fraction.");

		/// <summary>The time from the start of the storyboard.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the cue is not a time.</exception>
		public TimeSpan TimeSpan => _kind == 2 ? _time : throw new InvalidOperationException("This cue is not a time.");

		/// <summary>Parses <c>"60%"</c>, <c>"0.6"</c> or a time such as <c>"0:0:0.24"</c>, invariantly.</summary>
		public static bool TryParse(string? s, IFormatProvider? provider, out Cue result)
		{
			result = default;
			if (s is null)
				return false;

			var text = s.Trim();
			if (text.Length == 0)
				return false;

			if (text[text.Length - 1] == '%')
			{
				if (!double.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var percent)
					|| double.IsNaN(percent) || percent < 0 || percent > 100)
					return false;

				result = new Cue(1, percent / 100.0, default);
				return true;
			}

			if (text.IndexOf(":", StringComparison.Ordinal) >= 0)
			{
				if (!TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var time) || time < TimeSpan.Zero)
					return false;

				result = new Cue(2, 0, time);
				return true;
			}

			if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var fraction)
				|| double.IsNaN(fraction) || fraction < 0 || fraction > 1)
				return false;

			result = new Cue(1, fraction, default);
			return true;
		}

		/// <summary>The invariant text that parses back to this exact cue: the fraction itself (a percentage would not
		/// survive the multiply and divide by 100 exactly), a time, or empty when unspecified.</summary>
		public override string ToString() => _kind switch
		{
			1 => _fraction.ToString("R", CultureInfo.InvariantCulture),
			2 => _time.ToString("c", CultureInfo.InvariantCulture),
			_ => string.Empty,
		};

		/// <inheritdoc/>
		public bool Equals(Cue other) => _kind == other._kind && _fraction.Equals(other._fraction) && _time == other._time;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => obj is Cue other && Equals(other);

		/// <inheritdoc/>
		public override int GetHashCode() => (_kind, _fraction, _time).GetHashCode();

		/// <summary>Whether two cues are equal.</summary>
		public static bool operator ==(Cue left, Cue right) => left.Equals(right);

		/// <summary>Whether two cues differ.</summary>
		public static bool operator !=(Cue left, Cue right) => !left.Equals(right);

		internal double Resolve(TimeSpan duration)
			=> _kind == 2 ? _time.Ticks / (double)duration.Ticks : _fraction;
	}

	/// <summary>Converts <c>"60%"</c>, <c>"0.6"</c> and <c>"0:0:0.24"</c> to a <see cref="Cue"/>.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class CueTypeConverter : TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);

		/// <inheritdoc/>
		public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var text = value as string ?? value?.ToString();
			if (string.IsNullOrWhiteSpace(text))
				return Cue.Unspecified;

			if (Cue.TryParse(text, CultureInfo.InvariantCulture, out var cue))
				return cue;

			throw new FormatException($"Cannot convert \"{text}\" into {typeof(Cue)}. Use a percentage (\"60%\"), a fraction between 0 and 1 (\"0.6\") or a time (\"0:0:0.24\").");
		}

		/// <inheritdoc/>
		public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
			=> value is Cue cue ? cue.ToString() : throw new NotSupportedException();
	}
}
