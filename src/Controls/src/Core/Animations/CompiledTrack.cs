#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>One keyframe of a compiled track: a resolved cue, a value, and the easing of the segment arriving at it.</summary>
	internal readonly struct TrackSample
	{
		public TrackSample(double cue, object? value, Easing? easing)
		{
			Cue = cue;
			Value = value;
			Easing = easing;
		}

		public double Cue { get; }

		public object? Value { get; }

		public Easing? Easing { get; }
	}

	/// <summary>
	/// The keyframes of one (target, property) in one storyboard node, evaluated as a pure function of progress.
	/// </summary>
	internal abstract class CompiledTrack
	{
		readonly WeakReference<BindableObject> _target;

		protected CompiledTrack(BindableObject target, BindableProperty property)
		{
			_target = new WeakReference<BindableObject>(target);
			Property = property;
		}

		public BindableProperty Property { get; }

		public BindableObject? Target => _target.TryGetTarget(out var target) ? target : null;

		/// <summary>The track has no sample at cue 0, so it starts from the property's value when the run starts.</summary>
		public abstract bool HasImplicitStart { get; }

		/// <summary>Sets the implicit start value (the value on screen when the run starts, or when the node activates).</summary>
		public abstract void SetImplicitStart(object? value);

		/// <summary>The value at <paramref name="progress"/> through the node's iteration, 0..1.</summary>
		public abstract object? Evaluate(double progress);
	}

	internal sealed class CompiledTrack<T> : CompiledTrack
	{
		readonly double[] _cues;
		readonly T[] _values;
		readonly Easing[] _easings;
		readonly IValueInterpolator<T> _interpolator;
		T _start = default!;

		public CompiledTrack(BindableObject target, BindableProperty property, IReadOnlyList<TrackSample> samples, Easing? defaultEasing, IValueInterpolator<T> interpolator)
			: base(target, property)
		{
			_interpolator = interpolator;
			_cues = new double[samples.Count];
			_values = new T[samples.Count];
			_easings = new Easing[samples.Count];

			for (var i = 0; i < samples.Count; i++)
			{
				_cues[i] = samples[i].Cue;
				_values[i] = Cast(samples[i].Value);

				// Storyboard keyframes default to Linear; Storyboard.Easing fills in only unspecified segments.
				_easings[i] = samples[i].Easing ?? defaultEasing ?? Easing.Linear;
			}

			_start = _values[0];
		}

		public override bool HasImplicitStart => _cues[0] > 0;

		public override void SetImplicitStart(object? value) => _start = Cast(value);

		static T Cast(object? value) => value is T typed ? typed : default!;

		public override object? Evaluate(double progress)
		{
			var cues = _cues;
			var last = cues.Length - 1;

			if (progress < cues[0])
			{
				// The segment from the implicit start (cue 0) to the first keyframe, shaped by that keyframe's easing.
				var local = progress <= 0 ? 0 : progress / cues[0];
				return _interpolator.Interpolate(_start, _values[0], _easings[0].Ease(local));
			}

			// Exact end: a sample that lands on a keyframe writes that keyframe's value itself, never Lerp(a, b, 1).
			if (progress >= cues[last])
				return _values[last];

			var i = 0;
			while (i < last && cues[i + 1] <= progress)
				i++;

			if (progress == cues[i])
				return _values[i];

			var span = cues[i + 1] - cues[i];
			var t = (progress - cues[i]) / span;
			return _interpolator.Interpolate(_values[i], _values[i + 1], _easings[i + 1].Ease(t));
		}
	}
}
