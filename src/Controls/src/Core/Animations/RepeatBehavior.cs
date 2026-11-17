#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// How often a storyboard repeats: an iteration count (<c>"3x"</c>, fractional counts allowed),
	/// a total duration (<c>"0:0:10"</c>) or <see cref="Forever"/>. The default is one iteration.
	/// </summary>
	[TypeConverter(typeof(RepeatBehaviorTypeConverter))]
	public readonly struct RepeatBehavior : IEquatable<RepeatBehavior>
	{
		const byte KindCount = 0;
		const byte KindDuration = 1;
		const byte KindForever = 2;

		readonly byte _kind;
		readonly double _count;
		readonly TimeSpan _duration;

		/// <summary>Repeats <paramref name="count"/> times.</summary>
		/// <param name="count">The number of iterations. Must be greater than zero.</param>
		public RepeatBehavior(double count)
		{
			if (!(count > 0) || double.IsInfinity(count))
				throw new ArgumentOutOfRangeException(nameof(count));

			_kind = KindCount;
			_count = count;
			_duration = TimeSpan.Zero;
		}

		/// <summary>Repeats for a total of <paramref name="duration"/>.</summary>
		/// <param name="duration">The total active time. Must be greater than zero.</param>
		public RepeatBehavior(TimeSpan duration)
		{
			if (duration <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(duration));

			_kind = KindDuration;
			_count = 0;
			_duration = duration;
		}

		RepeatBehavior(byte kind)
		{
			_kind = kind;
			_count = 0;
			_duration = TimeSpan.Zero;
		}

		/// <summary>Gets a behavior that repeats until stopped.</summary>
		public static RepeatBehavior Forever => new(KindForever);

		/// <summary>Gets whether this behavior is an iteration count.</summary>
		public bool HasCount => _kind == KindCount;

		/// <summary>Gets whether this behavior is a total duration.</summary>
		public bool HasDuration => _kind == KindDuration;

		/// <summary>Gets the iteration count. Throws when <see cref="HasCount"/> is <see langword="false"/>.</summary>
		public double Count => _kind == KindCount ? (_count == 0 ? 1 : _count) : throw new InvalidOperationException("This repeat behavior is not a count.");

		/// <summary>Gets the total duration. Throws when <see cref="HasDuration"/> is <see langword="false"/>.</summary>
		public TimeSpan Duration => _kind == KindDuration ? _duration : throw new InvalidOperationException("This repeat behavior is not a duration.");

		internal bool IsForever => _kind == KindForever;

		/// <inheritdoc/>
		public override string ToString() => _kind switch
		{
			KindDuration => _duration.ToString("c", CultureInfo.InvariantCulture),
			KindForever => nameof(Forever),
			_ => Count.ToString("R", CultureInfo.InvariantCulture) + "x",
		};

		/// <inheritdoc/>
		public bool Equals(RepeatBehavior other)
			=> _kind == other._kind && (_kind != KindCount || Count.Equals(other.Count)) && _duration == other._duration;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => obj is RepeatBehavior other && Equals(other);

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				return (_kind * 397) ^ (_kind == KindCount ? Count.GetHashCode() : 0) ^ _duration.GetHashCode();
			}
		}

		/// <summary>Compares two behaviors for equality.</summary>
		/// <param name="left">The first behavior.</param>
		/// <param name="right">The second behavior.</param>
		/// <returns><see langword="true"/> when the behaviors are equal.</returns>
		public static bool operator ==(RepeatBehavior left, RepeatBehavior right) => left.Equals(right);

		/// <summary>Compares two behaviors for inequality.</summary>
		/// <param name="left">The first behavior.</param>
		/// <param name="right">The second behavior.</param>
		/// <returns><see langword="true"/> when the behaviors differ.</returns>
		public static bool operator !=(RepeatBehavior left, RepeatBehavior right) => !left.Equals(right);
	}

	/// <summary>
	/// Converts strings such as <c>"3x"</c>, <c>"0:0:10"</c> and <c>"Forever"</c> to a <see cref="RepeatBehavior"/>.
	/// </summary>
	public sealed class RepeatBehaviorTypeConverter : TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		/// <inheritdoc/>
		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var text = (value as string)?.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				if (string.Equals(text, nameof(RepeatBehavior.Forever), StringComparison.OrdinalIgnoreCase))
					return RepeatBehavior.Forever;

				if (text![text.Length - 1] is 'x' or 'X'
					&& double.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var count)
					&& count > 0 && !double.IsInfinity(count))
					return new RepeatBehavior(count);

				if (TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var duration) && duration > TimeSpan.Zero)
					return new RepeatBehavior(duration);
			}

			throw new FormatException($"Cannot convert \"{value}\" into {typeof(RepeatBehavior)}. Use a count (\"3x\"), a duration (\"0:0:10\") or \"Forever\".");
		}

		/// <inheritdoc/>
		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
			=> value is RepeatBehavior behavior && destinationType == typeof(string) ? behavior.ToString() : throw new NotSupportedException();
	}
}
