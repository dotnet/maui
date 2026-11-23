#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Builds a <see cref="Storyboard"/> in C#. Each <see cref="Build"/> returns a new storyboard, a snapshot of the
	/// builder at that moment.
	/// </summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class StoryboardBuilder
	{
		readonly List<Action<Storyboard>> _steps = new();

		internal StoryboardBuilder()
		{
		}

		/// <summary>Sets the length of one iteration.</summary>
		public StoryboardBuilder WithDuration(TimeSpan duration) => Add(s => s.Duration = duration);

		/// <summary>Sets the default target, resolved in the play target's name scope.</summary>
		public StoryboardBuilder WithTargetName(string targetName) => Add(s => s.TargetName = targetName);

		/// <summary>Sets the curve of segments whose keyframe sets none.</summary>
		public StoryboardBuilder WithEasing(Easing easing) => Add(s => s.Easing = easing);

		/// <summary>Sets how long the storyboard repeats.</summary>
		public StoryboardBuilder WithRepeat(RepeatBehavior repeat) => Add(s => s.RepeatBehavior = repeat);

		/// <summary>Sets which way each iteration runs.</summary>
		public StoryboardBuilder WithDirection(AnimationDirection direction) => Add(s => s.Direction = direction);

		/// <summary>Sets what the storyboard shows outside its active interval.</summary>
		public StoryboardBuilder WithFillMode(AnimationFillMode fillMode) => Add(s => s.FillMode = fillMode);

		/// <summary>Sets the delay before the storyboard starts.</summary>
		public StoryboardBuilder WithBeginTime(TimeSpan beginTime) => Add(s => s.BeginTime = beginTime);

		/// <summary>Sets the pause between iterations.</summary>
		public StoryboardBuilder WithIterationDelay(TimeSpan delay) => Add(s => s.IterationDelay = delay);

		/// <summary>Sets the speed relative to the parent.</summary>
		public StoryboardBuilder WithSpeedRatio(double speedRatio) => Add(s => s.SpeedRatio = speedRatio);

		/// <summary>Sets what a run does under reduced motion.</summary>
		public StoryboardBuilder WithReducedMotion(ReducedMotionBehavior behavior) => Add(s => s.ReducedMotion = behavior);

		/// <summary>
		/// Adds the keyframes of one property. <typeparamref name="T"/> must be the property's type.
		/// </summary>
		/// <param name="property">The property to animate.</param>
		/// <param name="track">Adds the keyframes.</param>
		/// <param name="targetName">The element to animate, resolved in the play target's name scope; the default target when null.</param>
		public StoryboardBuilder AddTrack<T>(BindableProperty property, Action<TrackBuilder<T>> track, string? targetName = null)
		{
			if (property is null)
				throw new ArgumentNullException(nameof(property));
			if (track is null)
				throw new ArgumentNullException(nameof(track));

			var builder = new TrackBuilder<T>();
			track(builder);
			var keys = builder.Keys.ToArray();

			return Add(storyboard =>
			{
				if (!property.ReturnType.IsAssignableFrom(typeof(T)) && property.ReturnType != typeof(T))
					throw new InvalidOperationException($"AddTrack<{typeof(T).Name}> doesn't match {property.PropertyName}, which is {property.ReturnType.Name}.");

				foreach (var key in keys)
				{
					var keyFrame = FindOrAddKeyFrame(storyboard, key.Cue);
					keyFrame.Setters.Add(new Setter { Property = property, Value = key.Value, TargetName = targetName });
					if (key.Easing is not null)
						keyFrame.Easing = key.Easing;
				}
			});
		}

		/// <summary>Adds a nested timeline starting at <paramref name="beginTime"/>.</summary>
		public StoryboardBuilder AddChild(TimeSpan beginTime, Action<StoryboardBuilder> child)
		{
			if (child is null)
				throw new ArgumentNullException(nameof(child));

			var builder = new StoryboardBuilder();
			child(builder);
			return Add(storyboard =>
			{
				var built = builder.Build();
				built.BeginTime = beginTime;
				storyboard.Children.Add(built);
			});
		}

		/// <summary>Adds nested timelines one after another, each starting where the previous one's effective span ends.</summary>
		public StoryboardBuilder Sequence(params Action<StoryboardBuilder>[] children)
		{
			if (children is null)
				throw new ArgumentNullException(nameof(children));

			var builders = new List<StoryboardBuilder>();
			foreach (var child in children)
			{
				var builder = new StoryboardBuilder();
				child(builder);
				builders.Add(builder);
			}

			return Add(storyboard =>
			{
				var offset = TimeSpan.Zero;
				foreach (var builder in builders)
				{
					var built = builder.Build();
					built.BeginTime += offset;
					storyboard.Children.Add(built);
					offset = EffectiveEnd(built);
				}
			});
		}

		/// <summary>
		/// Adds the same nested timeline once per target name, each starting <paramref name="delay"/> after the previous.
		/// Names resolve in the play target's name scope, so the storyboard stays a reusable definition.
		/// </summary>
		public StoryboardBuilder Stagger(TimeSpan delay, IEnumerable<string> targetNames, Action<StoryboardBuilder> child)
		{
			if (targetNames is null)
				throw new ArgumentNullException(nameof(targetNames));
			if (child is null)
				throw new ArgumentNullException(nameof(child));

			var names = new List<string>(targetNames);
			var builder = new StoryboardBuilder();
			child(builder);

			return Add(storyboard =>
			{
				for (var i = 0; i < names.Count; i++)
				{
					var built = builder.Build();
					built.TargetName = names[i];
					built.BeginTime += TimeSpan.FromTicks(delay.Ticks * i);
					storyboard.Children.Add(built);
				}
			});
		}

		/// <summary>Builds a new storyboard from the builder's current state.</summary>
		public Storyboard Build()
		{
			var storyboard = new Storyboard();
			foreach (var step in _steps)
				step(storyboard);
			return storyboard;
		}

		StoryboardBuilder Add(Action<Storyboard> step)
		{
			_steps.Add(step);
			return this;
		}

		static KeyFrame FindOrAddKeyFrame(Storyboard storyboard, Cue cue)
		{
			foreach (var keyFrame in storyboard.KeyFrames)
			{
				if (keyFrame.Cue == cue)
					return keyFrame;
			}

			var added = new KeyFrame { Cue = cue };

			// Keep keyframes in cue order, so cues set by different tracks never decrease in declaration order.
			var index = storyboard.KeyFrames.Count;
			while (index > 0 && Compare(storyboard.KeyFrames[index - 1].Cue, cue, storyboard.Duration) > 0)
				index--;

			storyboard.KeyFrames.Insert(index, added);
			return added;
		}

		static int Compare(Cue left, Cue right, TimeSpan duration)
		{
			var d = duration > TimeSpan.Zero ? duration : TimeSpan.FromTicks(1);
			return left.Resolve(d).CompareTo(right.Resolve(d));
		}

		static TimeSpan EffectiveEnd(Storyboard storyboard)
		{
			var repeat = storyboard.RepeatBehavior;
			var duration = storyboard.Duration;
			if (duration <= TimeSpan.Zero)
			{
				foreach (var child in storyboard.Children)
				{
					var end = EffectiveEnd(child);
					if (end > duration)
						duration = end;
				}
			}

			double active;
			if (repeat.HasDuration)
				active = repeat.Duration.Ticks * storyboard.SpeedRatio;
			else if (repeat == RepeatBehavior.Forever)
				return TimeSpan.MaxValue;
			else
				active = repeat.Count * duration.Ticks + Math.Max(0, Math.Ceiling(repeat.Count) - 1) * storyboard.IterationDelay.Ticks;

			return storyboard.BeginTime + TimeSpan.FromTicks((long)Math.Round(active / storyboard.SpeedRatio));
		}
	}

	/// <summary>Adds the keyframes of one property to a <see cref="StoryboardBuilder"/>.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class TrackBuilder<T>
	{
		internal List<(Cue Cue, T Value, Easing? Easing)> Keys { get; } = new();

		internal TrackBuilder()
		{
		}

		/// <summary>A keyframe at <paramref name="cue"/> (0..1) of the duration. <paramref name="easing"/> shapes the segment arriving at it.</summary>
		public TrackBuilder<T> At(double cue, T value, Easing? easing = null)
		{
			Keys.Add((Cue.FromFraction(cue), value, easing));
			return this;
		}

		/// <summary>A keyframe at <paramref name="time"/> from the start. <paramref name="easing"/> shapes the segment arriving at it.</summary>
		public TrackBuilder<T> At(TimeSpan time, T value, Easing? easing = null)
		{
			Keys.Add((Cue.FromTimeSpan(time), value, easing));
			return this;
		}
	}
}
