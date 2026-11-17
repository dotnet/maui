#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// One visual-state transition (spec Part D). <see cref="VisualStateManager"/> stays synchronous:
	/// <see cref="VisualStateGroup.CurrentState"/> changes immediately, while the animated properties move
	/// from their pinned values to the new state's values.
	/// </summary>
	internal sealed class VisualTransitionRun : AnimationRun
	{
		sealed class Track
		{
			public Track(BindableObject target, BindableProperty property)
			{
				Target = target;
				Property = property;
			}

			public BindableObject Target { get; }

			public BindableProperty Property { get; }

			public object? From { get; set; }

			public bool Interpolates { get; set; }

			// IsVisible follows CSS visibility: visible for the whole transition when either end is visible.
			public object? VisibleDuring { get; set; }
		}

		readonly VisualElement _element;
		readonly VisualTransition _transition;
		readonly List<Track> _tracks = new();
		TimeSpan _duration;
		TimeSpan _startTime;
		Easing _easing = Easing.Default;
		bool _observingUnload;

		VisualTransitionRun(VisualElement element, VisualTransition transition)
		{
			_element = element;
			_transition = transition;
		}

		/// <summary>
		/// Returns a run when this state change animates, or <see langword="null"/> for today's instant behavior.
		/// </summary>
		public static VisualTransitionRun? Prepare(VisualElement element, VisualStateGroup group, VisualState to)
		{
			// Never animate the initial state, a missing match, an element that isn't on screen, or a change
			// the caller asked to apply without transitions (handled by the caller).
			if (group.CurrentState is not { } from || ReferenceEquals(from, to) || !group.HasTransitions)
				return null;

			var transition = group.FindTransition(from.Name, to.Name);
			if (transition is null || transition.Duration <= TimeSpan.Zero)
				return null;

			if (!element.IsLoaded || element.Handler is null)
				return null;

			// Step 1: the union of the outgoing and incoming setters. MAUI unapplies every outgoing setter,
			// so a property only the outgoing state sets must animate back, not snap.
			var run = new VisualTransitionRun(element, transition);
			run.AddSetters(from);
			run.AddSetters(to);
			return run._tracks.Count > 0 ? run : null;
		}

		void AddSetters(VisualState state)
		{
			foreach (var setter in state.Setters)
			{
				if (setter.Property is not { } property || property.IsReadOnly || setter.Value is Style || setter.Value is IList<VisualStateGroup>)
					continue;

				if (setter.ResolveTarget(_element) is { } target)
					Add(target, property);
			}
		}

		/// <summary>
		/// A state change that doesn't animate applies instantly, also when a transition is still running:
		/// the running transition lets go of the properties either state sets, which then show their new values.
		/// </summary>
		public static void ReleaseRunningTracks(VisualElement element, VisualState from, VisualState to)
		{
			ReleaseRunningTracks(element, from);
			ReleaseRunningTracks(element, to);
		}

		static void ReleaseRunningTracks(VisualElement element, VisualState state)
		{
			foreach (var setter in state.Setters)
			{
				if (setter.Property is not { } property || setter.ResolveTarget(element) is not { } target)
					continue;

				if (AnimationLayer.GetOwner(target, property) is VisualTransitionRun run)
					run.ReleaseTrack(target, property);
			}
		}

		void ReleaseTrack(BindableObject target, BindableProperty property)
		{
			for (int i = _tracks.Count - 1; i >= 0; i--)
			{
				if (ReferenceEquals(_tracks[i].Target, target) && _tracks[i].Property == property)
					_tracks.RemoveAt(i);
			}

			AnimationLayer.Release(target, property, this);

			if (_tracks.Count == 0)
			{
				StopTicking();
				StopObservingUnload();
			}
		}

		void Add(BindableObject target, BindableProperty property)
		{
			foreach (var track in _tracks)
			{
				if (ReferenceEquals(track.Target, target) && track.Property == property)
					return;
			}

			// Step 2: TwoWay-bound and platform-written values apply without animation (rule 8).
			if (AnimationLayer.IsExcluded(target, property))
				return;

			_tracks.Add(new Track(target, property));
		}

		/// <summary>
		/// Step 3: pin each property at its current value. Nothing changes, so nothing is raised. A property
		/// a running transition already holds keeps its entry: the new run takes it over in place.
		/// </summary>
		public void Pin()
		{
			foreach (var track in _tracks)
			{
				var current = track.Target.GetValue(track.Property);
				var alreadyHeld = track.Target.HasAnimatedValue(track.Property);

				AnimationLayer.Acquire(track.Target, track.Property, this);
				if (!alreadyHeld)
					AnimationLayer.Write(track.Target, track.Property, current);

				track.From = current;
			}
		}

		/// <summary>
		/// Steps 5 and 6, after the state was applied beneath the pins: read destinations and start animating.
		/// </summary>
		public void Start()
		{
			for (int i = _tracks.Count - 1; i >= 0; i--)
			{
				var track = _tracks[i];
				if (!AnimationLayer.Owns(track.Target, track.Property, this))
				{
					_tracks.RemoveAt(i);
					continue;
				}

				var destination = AnimationLayer.GetBaseValue(track.Target, track.Property);
				if (Equals(destination, track.From))
				{
					AnimationLayer.Release(track.Target, track.Property, this);
					_tracks.RemoveAt(i);
					continue;
				}

				if (track.Property == VisualElement.IsVisibleProperty && track.From is bool fromVisible && destination is bool toVisible)
				{
					track.VisibleDuring = fromVisible || toVisible;
					AnimationLayer.Write(track.Target, track.Property, track.VisibleDuring);
				}
				else
				{
					track.Interpolates = AnimationInterpolation.CanInterpolate(track.From, destination);
				}
			}

			if (_tracks.Count == 0)
				return;

			var duration = _transition.Duration;
			if (MotionPolicy.IsReducedMotion && _transition.ReducedMotion != ReducedMotionBehavior.Play)
			{
				// The destinations are already applied beneath the pins: releasing completes the change.
				if (_transition.ReducedMotion == ReducedMotionBehavior.SkipToEnd)
				{
					Finish();
					return;
				}

				duration = TimeSpan.FromTicks(Math.Min(duration.Ticks / 10, TimeSpan.FromMilliseconds(100).Ticks));
			}
			else if (!MotionPolicy.IsReducedMotion)
			{
				duration = TimeSpan.FromTicks((long)(duration.Ticks * MotionPolicy.DurationScale));
			}

			_duration = duration;
			_easing = _transition.Easing ?? Easing.Default;

			_element.Unloaded += OnUnloaded;
			_observingUnload = true;

			var clock = AnimationClocks.For(_element);
			_startTime = clock.Elapsed;
			StartTicking(clock);
		}

		protected override void OnTick()
		{
			var elapsed = Clock!.Elapsed - _startTime;
			var progress = _duration.Ticks <= 0 ? 1.0 : Math.Max(0.0, (double)elapsed.Ticks / _duration.Ticks);
			if (progress >= 1.0)
			{
				Finish();
				return;
			}

			var eased = _easing.Ease(progress);

			for (int i = _tracks.Count - 1; i >= 0; i--)
			{
				if (i >= _tracks.Count)
					continue;

				var track = _tracks[i];
				object? value;
				if (track.VisibleDuring is not null)
				{
					value = track.VisibleDuring;
				}
				else
				{
					// Destinations are re-read every frame, so a source that changes mid-transition moves the
					// destination with it instead of jumping at release.
					var destination = AnimationLayer.GetBaseValue(track.Target, track.Property);
					value = track.Interpolates && AnimationInterpolation.CanInterpolate(track.From, destination)
						? AnimationInterpolation.Interpolate(track.From, destination, eased)
						: track.From;
				}

				AnimationLayer.Write(track.Target, track.Property, value);
			}
		}

		/// <summary>Step 7: release every hold. The highest remaining value, the new state's, is shown.</summary>
		void Finish()
		{
			StopTicking();
			StopObservingUnload();

			var tracks = _tracks.ToArray();
			_tracks.Clear();
			foreach (var track in tracks)
				AnimationLayer.Release(track.Target, track.Property, this);
		}

		void StopObservingUnload()
		{
			if (!_observingUnload)
				return;

			_observingUnload = false;
			_element.Unloaded -= OnUnloaded;
		}

		void OnUnloaded(object? sender, EventArgs e) => Finish();

		protected override void OnWindowDestroyed() => Finish();

		public override void OnTrackLost(BindableObject target, BindableProperty property, TrackLossReason reason)
		{
			for (int i = _tracks.Count - 1; i >= 0; i--)
			{
				if (ReferenceEquals(_tracks[i].Target, target) && _tracks[i].Property == property)
					_tracks.RemoveAt(i);
			}

			if (_tracks.Count == 0)
			{
				StopTicking();
				StopObservingUnload();
			}
		}
	}
}
