#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Builds a <see cref="Storyboard"/> in C#. The result is a target-free definition that can be reused.
	/// </summary>
	public sealed class StoryboardBuilder
	{
		readonly Storyboard _storyboard = new();
		readonly List<(BindableProperty Property, Type ValueType, string? TargetName, IReadOnlyList<(Cue Cue, object? Value, Easing? Easing)> Samples)> _tracks = new();

		internal StoryboardBuilder()
		{
		}

		/// <summary>Sets the length of one iteration.</summary>
		/// <param name="duration">The duration.</param>
		/// <returns>This builder.</returns>
		public StoryboardBuilder WithDuration(TimeSpan duration)
		{
			_storyboard.Duration = duration;
			return this;
		}

		/// <summary>Sets the element that tracks without their own target name animate.</summary>
		/// <param name="targetName">The name, resolved in the play target's name scope.</param>
		/// <returns>This builder.</returns>
		public StoryboardBuilder WithTargetName(string targetName)
		{
			_storyboard.TargetName = targetName;
			return this;
		}

		/// <summary>Sets the easing of every segment that doesn't set its own.</summary>
		/// <param name="easing">The easing.</param>
		/// <returns>This builder.</returns>
		public StoryboardBuilder WithEasing(Easing easing)
		{
			_storyboard.Easing = easing;
			return this;
		}

		/// <summary>Sets how often the storyboard repeats.</summary>
		/// <param name="repeat">The repeat behavior.</param>
		/// <returns>This builder.</returns>
		public StoryboardBuilder WithRepeat(RepeatBehavior repeat)
		{
			_storyboard.RepeatBehavior = repeat;
			return this;
		}

		/// <summary>Sets the direction in which iterations play.</summary>
		/// <param name="direction">The direction.</param>
		/// <returns>This builder.</returns>
		public StoryboardBuilder WithDirection(AnimationDirection direction)
		{
			_storyboard.Direction = direction;
			return this;
		}

		/// <summary>Sets which state is shown before the storyboard begins and after it ends.</summary>
		/// <param name="fillMode">The fill mode.</param>
		/// <returns>This builder.</returns>
		public StoryboardBuilder WithFillMode(AnimationFillMode fillMode)
		{
			_storyboard.FillMode = fillMode;
			return this;
		}

		/// <summary>
		/// Adds a track for <paramref name="property"/>. <typeparamref name="T"/> must match the property's type;
		/// it is explicit because a <see cref="BindableProperty"/> is untyped.
		/// </summary>
		/// <typeparam name="T">The type of the property's values.</typeparam>
		/// <param name="property">The property to animate.</param>
		/// <param name="track">Adds the track's samples.</param>
		/// <param name="targetName">The element to animate, resolved in the play target's name scope; <see langword="null"/> for the default target.</param>
		/// <returns>This builder.</returns>
		public StoryboardBuilder AddTrack<T>(BindableProperty property, Action<TrackBuilder<T>> track, string? targetName = null)
		{
			if (property is null)
				throw new ArgumentNullException(nameof(property));
			if (track is null)
				throw new ArgumentNullException(nameof(track));

			var builder = new TrackBuilder<T>();
			track(builder);
			_tracks.Add((property, typeof(T), targetName, builder.Samples));
			return this;
		}

		/// <summary>Creates the storyboard.</summary>
		/// <returns>The storyboard.</returns>
		/// <exception cref="InvalidOperationException">Thrown when a track's type doesn't match its property, or no duration was set.</exception>
		public Storyboard Build()
		{
			if (_storyboard.Duration <= TimeSpan.Zero)
				throw new InvalidOperationException("Set a duration with WithDuration before building (MAUIX2303).");

			var frames = new List<(double Order, int Sequence, KeyFrame Frame)>();
			var sequence = 0;
			foreach (var (property, valueType, targetName, samples) in _tracks)
			{
				if (!property.ReturnType.IsAssignableFrom(valueType))
					throw new InvalidOperationException($"A track of {valueType} can't animate {property.PropertyName}, which is {property.ReturnType}.");

				foreach (var (cue, value, easing) in samples)
				{
					var frame = new KeyFrame { Cue = cue, Easing = easing };
					frame.Setters.Add(new Setter { Property = property, Value = value, TargetName = targetName });

					var order = cue.HasTimeSpan ? (double)cue.TimeSpan.Ticks / _storyboard.Duration.Ticks : cue.Fraction;
					frames.Add((order, sequence++, frame));
				}
			}

			// Cues must not decrease in declaration order; samples from different tracks interleave by cue.
			frames.Sort((left, right) => left.Order != right.Order ? left.Order.CompareTo(right.Order) : left.Sequence.CompareTo(right.Sequence));
			// Building again rebuilds the same storyboard rather than appending to it.
			_storyboard.KeyFrames.Clear();
			foreach (var (_, _, frame) in frames)
				_storyboard.KeyFrames.Add(frame);

			return _storyboard;
		}
	}

	/// <summary>
	/// Adds samples to one track of a <see cref="StoryboardBuilder"/>.
	/// </summary>
	/// <typeparam name="T">The type of the property's values.</typeparam>
	public sealed class TrackBuilder<T>
	{
		readonly List<(Cue Cue, object? Value, Easing? Easing)> _samples = new();

		internal TrackBuilder()
		{
		}

		internal IReadOnlyList<(Cue Cue, object? Value, Easing? Easing)> Samples => _samples;

		/// <summary>Adds a value at a fraction of the duration.</summary>
		/// <param name="cue">The fraction, from 0 to 1.</param>
		/// <param name="value">The value.</param>
		/// <param name="easing">The easing of the segment arriving at this value; <see langword="null"/> for the storyboard's.</param>
		/// <returns>This builder.</returns>
		public TrackBuilder<T> At(double cue, T value, Easing? easing = null)
		{
			_samples.Add((Cue.FromFraction(cue), value, easing));
			return this;
		}

		/// <summary>Adds a value at a time from the start of an iteration.</summary>
		/// <param name="time">The time.</param>
		/// <param name="value">The value.</param>
		/// <param name="easing">The easing of the segment arriving at this value; <see langword="null"/> for the storyboard's.</param>
		/// <returns>This builder.</returns>
		public TrackBuilder<T> At(TimeSpan time, T value, Easing? easing = null)
		{
			_samples.Add((Cue.FromTimeSpan(time), value, easing));
			return this;
		}
	}
}
