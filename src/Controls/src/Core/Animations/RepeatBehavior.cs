#nullable enable
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// How long a storyboard repeats: a number of iterations (<c>"3x"</c>, fractions allowed, <c>"0.5x"</c>), a total
	/// duration (<c>"0:0:10"</c>), or <see cref="Forever"/>.
	/// </summary>
	[TypeConverter(typeof(RepeatBehaviorTypeConverter))]
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public readonly struct RepeatBehavior : IEquatable<RepeatBehavior>
	{
		readonly byte _kind; // 0 = default (1x), 1 count, 2 duration, 3 forever
		readonly double _count;
		readonly TimeSpan _duration;

		/// <summary>Repeats <paramref name="count"/> times. Fractional counts stop part-way through the last iteration.</summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative, NaN or infinite.</exception>
		public RepeatBehavior(double count)
		{
			if (double.IsNaN(count) || double.IsInfinity(count) || count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), count, "A repeat count must be a finite number of zero or more.");

			_kind = 1;
			_count = count;
			_duration = default;
		}

		/// <summary>Repeats for <paramref name="duration"/> in total, measured in the parent's time.</summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="duration"/> is negative.</exception>
		public RepeatBehavior(TimeSpan duration)
		{
			if (duration < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(duration), duration, "A repeat duration can't be negative.");

			_kind = 2;
			_count = 0;
			_duration = duration;
		}

		RepeatBehavior(byte kind)
		{
			_kind = kind;
			_count = 0;
			_duration = default;
		}

		/// <summary>Repeats until stopped.</summary>
		public static RepeatBehavior Forever => new(3);

		/// <summary>Whether this is a number of iterations (the default, once, included).</summary>
		public bool HasCount => _kind <= 1;

		/// <summary>Whether this is a total duration.</summary>
		public bool HasDuration => _kind == 2;

		internal bool IsForever => _kind == 3;

		/// <summary>The number of iterations (1 for the default value).</summary>
		public double Count => _kind == 0 ? 1 : _kind == 1 ? _count : throw new InvalidOperationException("This repeat behavior is not a count.");

		/// <summary>The total duration.</summary>
		public TimeSpan Duration => _kind == 2 ? _duration : throw new InvalidOperationException("This repeat behavior is not a duration.");

		/// <inheritdoc/>
		public bool Equals(RepeatBehavior other)
			=> (HasCount && other.HasCount && Count.Equals(other.Count))
				|| (_kind == other._kind && _kind != 0 && _kind != 1 && _duration == other._duration);

		/// <inheritdoc/>
		public override bool Equals(object? obj) => obj is RepeatBehavior other && Equals(other);

		/// <inheritdoc/>
		public override int GetHashCode() => HasCount ? Count.GetHashCode() : (_kind, _duration).GetHashCode();

		/// <summary>Whether two repeat behaviors are equal.</summary>
		public static bool operator ==(RepeatBehavior left, RepeatBehavior right) => left.Equals(right);

		/// <summary>Whether two repeat behaviors differ.</summary>
		public static bool operator !=(RepeatBehavior left, RepeatBehavior right) => !left.Equals(right);

		/// <summary>The invariant text that parses back to this value: <c>"3x"</c>, <c>"0:0:10"</c> or <c>"Forever"</c>.</summary>
		public override string ToString() => _kind switch
		{
			2 => _duration.ToString("c", CultureInfo.InvariantCulture),
			3 => "Forever",
			_ => Count.ToString("R", CultureInfo.InvariantCulture) + "x",
		};

		/// <summary>Parses <c>"3x"</c>, <c>"0.5x"</c>, <c>"0:0:10"</c> or <c>"Forever"</c>, invariantly.</summary>
		public static bool TryParse(string? s, out RepeatBehavior result)
		{
			result = default;
			var text = s?.Trim();
			if (string.IsNullOrEmpty(text))
				return false;

			if (string.Equals(text, "Forever", StringComparison.OrdinalIgnoreCase))
			{
				result = Forever;
				return true;
			}

			var last = text![text.Length - 1];
			if (last == 'x' || last == 'X')
			{
				if (!double.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var count)
					|| double.IsNaN(count) || double.IsInfinity(count) || count < 0)
					return false;

				result = new RepeatBehavior(count);
				return true;
			}

			if (!TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var duration) || duration < TimeSpan.Zero)
				return false;

			result = new RepeatBehavior(duration);
			return true;
		}
	}

	/// <summary>Converts <c>"3x"</c>, <c>"0:0:10"</c> and <c>"Forever"</c> to a <see cref="RepeatBehavior"/>.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class RepeatBehaviorTypeConverter : TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);

		/// <inheritdoc/>
		public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var text = value as string ?? value?.ToString();
			if (RepeatBehavior.TryParse(text, out var result))
				return result;

			throw new FormatException($"Cannot convert \"{text}\" into {typeof(RepeatBehavior)}. Use a count (\"3x\"), a duration (\"0:0:10\") or \"Forever\".");
		}

		/// <inheritdoc/>
		public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
			=> value is RepeatBehavior repeat ? repeat.ToString() : throw new NotSupportedException();
	}
}
