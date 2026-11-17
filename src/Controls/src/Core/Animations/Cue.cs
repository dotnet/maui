#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// The position of a <see cref="KeyFrame"/> on its storyboard's timeline: a percentage (<c>"60%"</c>),
	/// a fraction (<c>"0.6"</c>) or a time (<c>"0:0:0.24"</c>), or unspecified to let the engine place it.
	/// </summary>
	[TypeConverter(typeof(CueTypeConverter))]
	public readonly struct Cue : IEquatable<Cue>
	{
		const byte KindUnspecified = 0;
		const byte KindFraction = 1;
		const byte KindTimeSpan = 2;

		readonly byte _kind;
		readonly double _fraction;
		readonly TimeSpan _timeSpan;

		Cue(byte kind, double fraction, TimeSpan timeSpan)
		{
			_kind = kind;
			_fraction = fraction;
			_timeSpan = timeSpan;
		}

		/// <summary>Gets a cue the engine places: evenly between its neighbors.</summary>
		public static Cue Unspecified => default;

		/// <summary>Creates a cue at a fraction of the duration, where 0 is the start and 1 the end.</summary>
		/// <param name="value">The fraction.</param>
		/// <returns>The cue.</returns>
		public static Cue FromFraction(double value)
		{
			if (double.IsNaN(value) || double.IsInfinity(value))
				throw new ArgumentOutOfRangeException(nameof(value));

			return new Cue(KindFraction, value, TimeSpan.Zero);
		}

		/// <summary>Creates a cue at a time from the start, resolved against the storyboard's duration.</summary>
		/// <param name="value">The time.</param>
		/// <returns>The cue.</returns>
		public static Cue FromTimeSpan(TimeSpan value) => new(KindTimeSpan, 0, value);

		/// <summary>Gets whether the engine places this cue.</summary>
		public bool IsUnspecified => _kind == KindUnspecified;

		/// <summary>Gets whether this cue is a time rather than a fraction.</summary>
		public bool HasTimeSpan => _kind == KindTimeSpan;

		/// <summary>Gets the fraction. Throws when the cue is a time or unspecified.</summary>
		public double Fraction => _kind == KindFraction ? _fraction : throw new InvalidOperationException("This cue is not a fraction.");

		/// <summary>Gets the time. Throws when the cue is a fraction or unspecified.</summary>
		public TimeSpan TimeSpan => _kind == KindTimeSpan ? _timeSpan : throw new InvalidOperationException("This cue is not a time.");

		/// <summary>Parses a percentage, a fraction or a time.</summary>
		/// <param name="s">The text to parse.</param>
		/// <param name="provider">The format provider; <see langword="null"/> means the invariant culture.</param>
		/// <param name="result">The parsed cue.</param>
		/// <returns><see langword="true"/> when <paramref name="s"/> was parsed.</returns>
		public static bool TryParse(string? s, IFormatProvider? provider, out Cue result)
		{
			result = default;
			if (s is null || string.IsNullOrWhiteSpace(s))
				return false;

			provider ??= CultureInfo.InvariantCulture;
			var text = s.Trim();

			if (text[text.Length - 1] == '%')
			{
				if (!double.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Float, provider, out var percent) || double.IsNaN(percent) || double.IsInfinity(percent))
					return false;

				result = FromFraction(percent / 100);
				return true;
			}

#if !NETSTANDARD2_0
			if (text.IndexOf(':', StringComparison.Ordinal) >= 0)
#else
			if (text.IndexOf(":", StringComparison.Ordinal) >= 0)
#endif
			{
				if (!TimeSpan.TryParse(text, provider, out var time))
					return false;

				result = FromTimeSpan(time);
				return true;
			}

			if (!double.TryParse(text, NumberStyles.Float, provider, out var fraction) || double.IsNaN(fraction) || double.IsInfinity(fraction))
				return false;

			result = FromFraction(fraction);
			return true;
		}

		/// <inheritdoc/>
		public override string ToString() => _kind switch
		{
			KindFraction => _fraction.ToString("R", CultureInfo.InvariantCulture),
			KindTimeSpan => _timeSpan.ToString("c", CultureInfo.InvariantCulture),
			_ => string.Empty,
		};

		/// <inheritdoc/>
		public bool Equals(Cue other) => _kind == other._kind && _fraction.Equals(other._fraction) && _timeSpan == other._timeSpan;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => obj is Cue other && Equals(other);

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				return (_kind * 397) ^ _fraction.GetHashCode() ^ _timeSpan.GetHashCode();
			}
		}

		/// <summary>Compares two cues for equality.</summary>
		/// <param name="left">The first cue.</param>
		/// <param name="right">The second cue.</param>
		/// <returns><see langword="true"/> when the cues are equal.</returns>
		public static bool operator ==(Cue left, Cue right) => left.Equals(right);

		/// <summary>Compares two cues for inequality.</summary>
		/// <param name="left">The first cue.</param>
		/// <param name="right">The second cue.</param>
		/// <returns><see langword="true"/> when the cues differ.</returns>
		public static bool operator !=(Cue left, Cue right) => !left.Equals(right);
	}

	/// <summary>
	/// Converts strings such as <c>"60%"</c>, <c>"0.6"</c> and <c>"0:0:0.24"</c> to a <see cref="Cue"/>.
	/// </summary>
	public sealed class CueTypeConverter : TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string) || sourceType == typeof(double);

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		/// <inheritdoc/>
		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			if (value is double fraction)
				return Cue.FromFraction(fraction);

			if (value is string text && Cue.TryParse(text, CultureInfo.InvariantCulture, out var cue))
				return cue;

			throw new FormatException($"Cannot convert \"{value}\" into {typeof(Cue)}. Use a percentage (\"60%\"), a fraction (\"0.6\") or a time (\"0:0:0.24\").");
		}

		/// <inheritdoc/>
		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
			=> value is Cue cue && destinationType == typeof(string) ? cue.ToString() : throw new NotSupportedException();
	}
}
