#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// One run of a <see cref="Storyboard"/> against one element.
	/// </summary>
	public sealed class StoryboardPlayer : IDisposable
	{
		readonly StoryboardRun _run;

		internal StoryboardPlayer(Storyboard storyboard, VisualElement target, IStoryboardClock? clock, PlaybackOptions options)
		{
			Storyboard = storyboard;
			_run = new StoryboardRun(this, storyboard, target, clock, options);
		}

		/// <summary>Gets the storyboard this player runs.</summary>
		public Storyboard Storyboard { get; }

		/// <summary>Gets the element the run plays on.</summary>
		public VisualElement? Target => _run.Target;

		/// <summary>Gets the state of the player.</summary>
		public PlaybackState State => _run.State;

		/// <summary>Gets the progress within the current iteration, from 0 to 1.</summary>
		public double Progress => _run.Progress;

		/// <summary>Gets the time within the current iteration.</summary>
		public TimeSpan CurrentTime => _run.CurrentTime;

		/// <summary>Gets the time since the run started, across iterations.</summary>
		public TimeSpan ElapsedTime => _run.ElapsedTime;

		/// <summary>Gets the zero-based index of the current iteration.</summary>
		public long CurrentIteration => _run.CurrentIteration;

		/// <summary>Gets or sets a speed multiplier for the whole run. This prototype supports positive rates only.</summary>
		public double PlaybackRate
		{
			get => _run.PlaybackRate;
			set => _run.PlaybackRate = value;
		}

		/// <summary>Gets a task that completes with the outcome of the run. It is never canceled.</summary>
		public Task<AnimationResult> Completion => _run.Completion;

		/// <summary>Raised when <see cref="State"/> changes.</summary>
		public event EventHandler<PlaybackStateChangedEventArgs>? StateChanged;

		/// <summary>Raised once when the run ends, after its values were committed or released.</summary>
		public event EventHandler<StoryboardCompletedEventArgs>? Completed;

		/// <summary>Starts a created player, or resumes a paused one.</summary>
		public void Resume() => _run.Resume();

		/// <summary>Pauses the run. Animated values stay where they are.</summary>
		public void Pause() => _run.Pause();

		/// <summary>Moves to <paramref name="progress"/> of one iteration on the next frame.</summary>
		/// <param name="progress">The progress within one iteration, from 0 to 1.</param>
		/// <param name="origin">Whether <paramref name="progress"/> counts from the beginning or from the end of the iteration.</param>
		public void Seek(double progress, TimeSeekOrigin origin = TimeSeekOrigin.BeginTime) => _run.Seek(progress, origin);

		/// <summary>Moves to <paramref name="time"/> on the next frame.</summary>
		/// <param name="time">The time to move to.</param>
		/// <param name="origin">Whether <paramref name="time"/> counts from the beginning or from the end of the first iteration.</param>
		public void SeekTo(TimeSpan time, TimeSeekOrigin origin = TimeSeekOrigin.BeginTime) => _run.SeekTo(time, origin);

		/// <summary>Moves to <paramref name="time"/> and applies the values now, also on a player that hasn't started.</summary>
		/// <param name="time">The time to move to.</param>
		/// <param name="origin">Whether <paramref name="time"/> counts from the beginning or from the end of the first iteration.</param>
		public void SeekAlignedToLastTick(TimeSpan time, TimeSeekOrigin origin = TimeSeekOrigin.BeginTime) => _run.SeekAlignedToLastTick(time, origin);

		/// <summary>Jumps to the end state and completes normally. A <see cref="RepeatBehavior.Forever"/> run, which has no end, is released.</summary>
		public void SkipToFill() => _run.SkipToFill();

		/// <summary>Stops the run.</summary>
		/// <param name="behavior">What happens to the animated values. The default removes them.</param>
		public void Stop(StopBehavior behavior = StopBehavior.Release) => _run.Stop(behavior);

		/// <summary>Stops the run and removes its animated values, including a held fill.</summary>
		public void Dispose() => _run.Stop(StopBehavior.Release);

		internal bool CanceledByToken => _run.CanceledByToken;

		internal void CancelFromToken() => _run.CancelFromToken();

		/// <summary>Whether the run can still show values: not ended, or ended holding a fill.</summary>
		internal bool HoldsValues => _run.HoldsValues;

		internal void RaiseStateChanged(PlaybackState oldState, PlaybackState newState)
			=> StateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(oldState, newState));

		internal void RaiseCompleted(AnimationResult result, int replacedTracks)
			=> Completed?.Invoke(this, new StoryboardCompletedEventArgs(result, replacedTracks));
	}

	/// <summary>The engine behind <see cref="StoryboardPlayer"/> (spec Part F).</summary>
	internal sealed class StoryboardRun : AnimationRun
	{
		enum Phase
		{
			Before,
			Active,
			After,
		}

		sealed class Track
		{
			public Track(BindableObject target, BindableProperty property)
			{
				Target = target;
				Property = property;
			}

			public BindableObject Target { get; }

			public BindableProperty Property { get; }

			public List<double> Cues { get; } = new();

			public List<object?> Values { get; } = new();

			public List<Easing?> Easings { get; } = new();

			bool[] _interpolates = Array.Empty<bool>();

			public bool Owned { get; set; }

			public bool Excluded { get; set; }

			public bool HasWritten { get; set; }

			public object? LastWritten { get; set; }

			public void Seal()
			{
				_interpolates = new bool[Cues.Count];
				for (int k = 1; k < Cues.Count; k++)
					_interpolates[k] = AnimationInterpolation.CanInterpolate(Property, Values[k - 1], Values[k]);
			}

			// The keyframe's easing shapes the segment that arrives at it.
			public object? Sample(double progress, Easing defaultEasing)
			{
				int last = Cues.Count - 1;
				if (progress <= Cues[0])
					return Values[0];
				if (progress >= Cues[last])
					return Values[last];

				int k = 0;
				while (k < last - 1 && progress >= Cues[k + 1])
					k++;

				double span = Cues[k + 1] - Cues[k];
				double t = span <= 0 ? 1 : (progress - Cues[k]) / span;

				if (!_interpolates[k + 1])
					return t >= 1 ? Values[k + 1] : Values[k];

				var easing = Easings[k + 1] ?? defaultEasing;
				return AnimationInterpolation.Interpolate(Values[k], Values[k + 1], easing.Ease(t));
			}
		}

		readonly StoryboardPlayer _player;
		readonly Storyboard _storyboard;
		readonly VisualElement _target;
		readonly IStoryboardClock? _explicitClock;
		readonly FillCompletion _fillCompletion;
		readonly StoryboardHandoffBehavior _handoff;
		readonly List<Track> _tracks;
		readonly TaskCompletionSource<AnimationResult> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

		// Timing, in ticks: begin time, speed, iteration duration, iteration delay, active duration.
		readonly double _begin;
		readonly double _speed;
		readonly double _duration;
		readonly double _delay;
		readonly double _active;
		readonly bool _forever;
		readonly long _endIteration;
		readonly double _endTau;
		readonly AnimationDirection _direction;
		readonly AnimationFillMode _fillMode;
		readonly Easing _defaultEasing;

		PlaybackState _state = PlaybackState.Created;
		bool _started;
		bool _prepared;
		bool _completed;
		bool _holding;
		bool _observingUnload;
		bool _waitingForWindow;
		double _rootTime;
		TimeSpan _segmentStart;
		double _rate = 1.0;
		double _motionFactor = 1.0;
		double? _pendingSeek;
		double _tau;
		long _iteration;
		int _replacedTracks;

		public StoryboardRun(StoryboardPlayer player, Storyboard storyboard, VisualElement target, IStoryboardClock? clock, PlaybackOptions options)
		{
			_player = player;
			_storyboard = storyboard;
			_target = target;
			_explicitClock = clock;

			// Options are read once. Changing the options object later doesn't affect this run.
			_fillCompletion = options.FillCompletion;
			_handoff = options.HandoffBehavior;

			if (!(options.SpeedRatio > 0) || double.IsInfinity(options.SpeedRatio))
				throw new ArgumentOutOfRangeException(nameof(options), "SpeedRatio must be greater than zero.");

			var duration = storyboard.Duration;
			if (duration <= TimeSpan.Zero)
				throw new InvalidOperationException("A storyboard with keyframes needs a Duration (MAUIX2303).");

			_duration = duration.Ticks;
			_delay = storyboard.IterationDelay.Ticks;
			_begin = (options.BeginTimeOverride ?? storyboard.BeginTime).Ticks;
			_speed = storyboard.SpeedRatio * options.SpeedRatio;
			_direction = storyboard.Direction;
			_fillMode = storyboard.FillMode;
			_defaultEasing = storyboard.Easing ?? Easing.Linear;

			// Products such as 300 ms x 1.1 and differences such as 1.2 - 1 aren't exact in binary. The END state
			// is computed from boundaries snapped at far below one tick, so an end that is mathematically on an
			// iteration boundary or on a cue isn't placed just before it.
			var span = _duration + _delay;
			var repeat = storyboard.RepeatBehavior;
			long endIteration;
			double endProgress;
			if (repeat.IsForever)
			{
				_forever = true;
				_active = double.PositiveInfinity;
				endIteration = 0;
				endProgress = 0;
			}
			else if (repeat.HasDuration)
			{
				// An end exactly on an iteration boundary belongs to the previous iteration.
				_active = SnapToTick(repeat.Duration.Ticks * _speed);
				var iterations = SnapToInteger(_active / span);
				endIteration = Math.Max(0, (long)Math.Ceiling(iterations) - 1);
				endProgress = Math.Min(SnapToTick(_active - endIteration * span), _duration);
			}
			else
			{
				var count = repeat.Count;
				_active = SnapToTick(count * _duration + (Math.Ceiling(count) - 1) * _delay);
				endIteration = (long)Math.Ceiling(count) - 1;
				endProgress = SnapToTick((count - endIteration) * _duration);
			}

			_endIteration = endIteration;
			_endTau = Directed(endIteration, endProgress);
			_tracks = BuildTracks(storyboard, target, duration);
		}

		static double SnapToTick(double ticks)
		{
			var rounded = Math.Round(ticks);
			return Math.Abs(ticks - rounded) < 1e-6 ? rounded : ticks;
		}

		static double SnapToInteger(double value)
		{
			var rounded = Math.Round(value);
			return Math.Abs(value - rounded) < 1e-9 ? rounded : value;
		}

		public VisualElement? Target => _target;

		public PlaybackState State => _state;

		public Task<AnimationResult> Completion => _completion.Task;

		public double Progress => _duration > 0 ? _tau / _duration : 0;

		public TimeSpan CurrentTime => TimeSpan.FromTicks((long)_tau);

		public TimeSpan ElapsedTime => TimeSpan.FromTicks((long)CurrentRootTime());

		public long CurrentIteration => _iteration;

		public bool CanceledByToken { get; private set; }

		/// <summary>Whether the run can still show values: not ended, or ended holding a fill.</summary>
		public bool HoldsValues => !_completed || _holding;

		public double PlaybackRate
		{
			get => _rate;
			set
			{
				CheckAccess();
				if (!(value > 0) || double.IsInfinity(value))
					throw new ArgumentOutOfRangeException(nameof(value), "This prototype supports positive playback rates only.");

				Rebase();
				_rate = value;
			}
		}

		public void Resume()
		{
			CheckAccess();
			if (_completed || _state == PlaybackState.Running)
				return;

			if (!Start())
				return;

			SetState(PlaybackState.Running);

			// A StateChanged handler can stop or pause the run, which must then not tick.
			if (_completed || _state != PlaybackState.Running)
				return;

			BeginTicking();
		}

		// Runs once, at the first Resume or synchronous seek: claims the tracks, applies the reduced-motion
		// policy and observes the target's lifetime. Returns false when that ended the run.
		bool Start()
		{
			if (_started)
				return !_completed;

			_started = true;
			Prepare();
			if (_completed)
				return false;

			// Observed before the reduced-motion policy, which can end the run holding its fill.
			ObserveLifetime();
			ApplyMotionPolicy();
			return !_completed;
		}

		void BeginTicking()
		{
			// A Forever run on an element without a window waits for one. A forgotten run on an element that's
			// never shown doesn't tick forever, and nothing static keeps it or its target alive.
			if (_explicitClock is null && _forever && _target.Window is null)
			{
				if (!_waitingForWindow)
				{
					_waitingForWindow = true;
					_target.Loaded += OnTargetLoaded;
				}

				return;
			}

			if (_pendingSeek is double seek)
			{
				_rootTime = seek;
				_pendingSeek = null;
			}

			var clock = _explicitClock ?? AnimationClocks.For(_target);
			_segmentStart = clock.Elapsed;
			StartTicking(clock);

			// Show the current frame now, so an entrance never flashes its final state.
			Render(CurrentRootTime());
		}

		void OnTargetLoaded(object? sender, EventArgs e)
		{
			// Loaded can be raised while the window isn't set yet; keep waiting until it is.
			if (_target.Window is null)
				return;

			StopWaitingForWindow();
			if (_completed || _state != PlaybackState.Running)
				return;

			ObserveLifetime();
			BeginTicking();
		}

		void StopWaitingForWindow()
		{
			if (!_waitingForWindow)
				return;

			_waitingForWindow = false;
			_target.Loaded -= OnTargetLoaded;
		}

		public void Pause()
		{
			CheckAccess();
			if (_state != PlaybackState.Running)
				return;

			_rootTime = CurrentRootTime();
			StopTicking();
			StopWaitingForWindow();
			SetState(PlaybackState.Paused);
		}

		public void Seek(double progress, TimeSeekOrigin origin)
		{
			CheckAccess();
			var local = (origin == TimeSeekOrigin.Duration ? 1 - progress : progress) * _duration;
			_pendingSeek = Math.Max(0, _begin + local / _speed);
		}

		public void SeekTo(TimeSpan time, TimeSeekOrigin origin)
		{
			CheckAccess();
			_pendingSeek = ToRootTime(time, origin);
		}

		public void SeekAlignedToLastTick(TimeSpan time, TimeSeekOrigin origin)
		{
			CheckAccess();
			if (_completed || !Start())
				return;

			_rootTime = ToRootTime(time, origin);
			_pendingSeek = null;
			if (IsTicking)
				_segmentStart = Clock!.Elapsed;

			Render(_rootTime);
		}

		public void SkipToFill()
		{
			CheckAccess();
			if (_completed)
				return;

			Prepare();
			if (_completed)
				return;

			// A held fill must still end with its target or window.
			ObserveLifetime();
			if (_forever)
				Finish(AnimationResult.Stopped, keepEnd: false, commitCurrent: false);
			else
				Finish(AnimationResult.Completed, keepEnd: KeepsEnd, commitCurrent: false);
		}

		public void Stop(StopBehavior behavior)
		{
			CheckAccess();
			StopCore(behavior);
		}

		void StopCore(StopBehavior behavior)
		{
			if (_holding)
			{
				// The run already ended at its fill. ApplyFill keeps it; CommitCurrent keeps the values beneath their
				// value sources; Release removes them.
				if (behavior != StopBehavior.ApplyFill)
					ReleaseHeldFill(commit: behavior == StopBehavior.CommitCurrent);
				return;
			}

			if (_completed)
				return;

			if (!_prepared)
			{
				Finish(AnimationResult.Stopped, keepEnd: false, commitCurrent: false);
				return;
			}

			switch (behavior)
			{
				case StopBehavior.ApplyFill:
					Finish(AnimationResult.Stopped, keepEnd: KeepsEnd, commitCurrent: false);
					break;
				case StopBehavior.CommitCurrent:
					Finish(AnimationResult.Stopped, keepEnd: false, commitCurrent: true);
					break;
				default:
					Finish(AnimationResult.Stopped, keepEnd: false, commitCurrent: false);
					break;
			}
		}

		public void CancelFromToken()
		{
			void Cancel()
			{
				if (_completed)
					return;

				CanceledByToken = true;
				StopCore(StopBehavior.Release);
			}

			if (_target.TryGetDispatcher() is { IsDispatchRequired: true } dispatcher)
			{
				// A dispatcher that no longer accepts work can't release anything, but the task still ends.
				if (!dispatcher.Dispatch(Cancel))
				{
					CanceledByToken = true;
					_completion.TrySetResult(AnimationResult.Stopped);
				}
			}
			else
			{
				Cancel();
			}
		}

		// Players are UI-thread only. Without a dispatcher, as in unit tests, nothing is checked.
		void CheckAccess()
		{
			if (_target.TryGetDispatcher() is { IsDispatchRequired: true })
				throw new InvalidOperationException("A storyboard player can only be used on its target's UI thread. Use Storyboard.PlayAsync from other threads.");
		}

		protected override void OnTick()
		{
			if (_pendingSeek is double seek)
			{
				_rootTime = seek;
				_segmentStart = Clock!.Elapsed;
				_pendingSeek = null;
			}

			Render(CurrentRootTime());
		}

		protected override void OnWindowDestroyed() => StopCore(StopBehavior.Release);

		// Teardown first, then the fault is reported: Completion faults, and the engine observes and logs it
		// (MAUIA1005) rather than letting it escape into the platform's frame callback.
		protected override void OnFault(Exception exception)
		{
			LogFault<StoryboardRun>(exception);
			if (_completed && !_holding)
				return;

			_completed = true;
			_holding = false;
			_rootTime = CurrentRootTime();
			StopTicking();
			StopWaitingForWindow();
			StopObservingLifetime();
			foreach (var track in _tracks)
				ReleaseQuietly(track);

			if (_completion.TrySetException(exception))
				_ = _completion.Task.Exception;

			try
			{
				SetState(PlaybackState.Stopped);
			}
			catch (Exception handlerException)
			{
				LogFault<StoryboardRun>(handlerException);
			}
		}

		public override void OnTrackLost(BindableObject target, BindableProperty property, TrackLossReason reason)
		{
			foreach (var track in _tracks)
			{
				if (track.Owned && ReferenceEquals(track.Target, target) && track.Property == property)
				{
					track.Owned = false;
					_replacedTracks++;
				}
			}

			if (_holding)
			{
				_holding = _tracks.Exists(t => t.Owned);
				if (!_holding)
					StopObservingLifetime();
				return;
			}

			// The run reports Replaced only when every track was taken over.
			if (!_completed && _prepared && !_tracks.Exists(t => t.Owned || t.Excluded))
				Finish(AnimationResult.Replaced, keepEnd: false, commitCurrent: false);
		}

		bool KeepsEnd => !_forever && (_fillMode == AnimationFillMode.Forward || _fillMode == AnimationFillMode.Both);

		double CurrentRootTime()
		{
			if (!IsTicking || Clock is null)
				return _rootTime;

			var elapsed = (Clock.Elapsed - _segmentStart).Ticks;
			return Math.Max(0, _rootTime + elapsed * _rate * _motionFactor);
		}

		void Rebase()
		{
			if (!IsTicking || Clock is null)
				return;

			_rootTime = CurrentRootTime();
			_segmentStart = Clock.Elapsed;
		}

		double ToRootTime(TimeSpan time, TimeSeekOrigin origin)
		{
			var root = origin == TimeSeekOrigin.Duration ? _begin + _duration / _speed - time.Ticks : time.Ticks;
			return Math.Max(0, root);
		}

		void Prepare()
		{
			if (_prepared)
				return;

			_prepared = true;
			foreach (var track in _tracks)
			{
				if (AnimationLayer.IsExcluded(track.Target, track.Property))
				{
					// Rule 8: TwoWay-bound and platform-written values are never interpolated.
					track.Excluded = true;
				}
				else if (_handoff == StoryboardHandoffBehavior.Ignore && AnimationLayer.GetOwner(track.Target, track.Property) is { } owner && !ReferenceEquals(owner, this))
				{
					// Leave the property to the run that animates it.
				}
				else
				{
					AnimationLayer.Acquire(track.Target, track.Property, this);
					track.Owned = true;
				}

				// Implicit start: a track without a cue-0 sample starts from the current value, which is the
				// animated value when another run held the property, so a handoff neither jumps nor notifies.
				if (track.Cues[0] > 0)
				{
					track.Cues.Insert(0, 0);
					track.Values.Insert(0, track.Target.GetValue(track.Property));
					track.Easings.Insert(0, null);
				}

				track.Seal();
			}

			if (_tracks.Count > 0 && !_tracks.Exists(t => t.Owned || t.Excluded))
				Finish(AnimationResult.Ignored, keepEnd: false, commitCurrent: false);
		}

		void ApplyMotionPolicy()
		{
			var behavior = _storyboard.ReducedMotion;
			if (MotionPolicy.IsReducedMotion && behavior != ReducedMotionBehavior.Play)
			{
				if (behavior == ReducedMotionBehavior.Shorten && !_forever)
				{
					var total = _begin + _active / _speed;
					var shortened = Math.Min(total * 0.1, TimeSpan.FromMilliseconds(100).Ticks);
					_motionFactor = shortened > 0 ? total / shortened : 1.0;
				}
				else
				{
					// One rule everywhere: settle into exactly the state a normal completion produces.
					Finish(AnimationResult.ReducedMotionSkipped, keepEnd: KeepsEnd, commitCurrent: false);
				}
			}
			else if (!MotionPolicy.IsReducedMotion)
			{
				_motionFactor = 1.0 / MotionPolicy.DurationScale;
			}
		}

		void Render(double rootTime)
		{
			var a = (rootTime - _begin) * _speed;
			Phase phase;
			if (a < 0)
			{
				phase = Phase.Before;
				_tau = Directed(0, 0);
				_iteration = 0;
			}
			else if (a < _active)
			{
				var span = _duration + _delay;
				var iteration = (long)Math.Floor(a / span);
				var x = Math.Min(a - iteration * span, _duration);
				phase = Phase.Active;
				_tau = Directed(iteration, x);
				_iteration = iteration;
			}
			else
			{
				phase = Phase.After;
				_tau = _endTau;
				_iteration = _endIteration;
			}

			switch (phase)
			{
				case Phase.Before:
					if (_fillMode == AnimationFillMode.Backward || _fillMode == AnimationFillMode.Both)
						Write(_tau / _duration);
					else
						ClearContribution();
					break;
				case Phase.Active:
					Write(_tau / _duration);
					break;
				default:
					Finish(AnimationResult.Completed, keepEnd: KeepsEnd, commitCurrent: false);
					break;
			}
		}

		double Directed(long iteration, double x)
		{
			bool forward = _direction switch
			{
				AnimationDirection.Reverse => false,
				AnimationDirection.Alternate => iteration % 2 == 0,
				AnimationDirection.AlternateReverse => iteration % 2 != 0,
				_ => true,
			};

			return forward ? x : _duration - x;
		}

		void Write(double progress)
		{
			for (int i = 0; i < _tracks.Count; i++)
			{
				var track = _tracks[i];
				if (!track.Owned)
					continue;

				var value = track.Sample(progress, _defaultEasing);
				if (track.HasWritten && Equals(value, track.LastWritten))
					continue;

				if (AnimationLayer.Write(track.Target, track.Property, value))
				{
					track.HasWritten = true;
					track.LastWritten = value;
				}
			}
		}

		// Before the active interval, without a backward fill, the run contributes nothing: a seek back removes
		// what it showed. It keeps ownership, so it can write again once active.
		void ClearContribution()
		{
			foreach (var track in _tracks)
			{
				if (!track.Owned || !track.HasWritten)
					continue;

				track.HasWritten = false;
				track.LastWritten = null;
				AnimationLayer.Clear(track.Target, track.Property, this);
			}
		}

		void Finish(AnimationResult result, bool keepEnd, bool commitCurrent)
		{
			if (_completed)
				return;

			// Teardown happens before any user callback runs. Nothing ticks, waits or observes past this point,
			// except to end a held fill with its target or window.
			_rootTime = CurrentRootTime();
			_completed = true;
			StopTicking();
			StopWaitingForWindow();

			if (!_forever && (keepEnd || result == AnimationResult.Completed || result == AnimationResult.ReducedMotionSkipped))
			{
				_tau = _endTau;
				_iteration = _endIteration;
				_rootTime = Math.Max(_rootTime, _begin + _active / _speed);
			}

			var hold = keepEnd && _fillCompletion == FillCompletion.Hold;
			var endProgress = _endTau / _duration;
			Exception? error = null;

			if (_prepared)
			{
				foreach (var track in _tracks)
				{
					try
					{
						FinishTrack(track, keepEnd, hold, commitCurrent, endProgress);
					}
					catch (Exception exception)
					{
						error ??= exception;
						ReleaseQuietly(track);
					}
				}

				// A fault ends the run: nothing it still holds stays held, held fills included.
				if (error is not null)
				{
					foreach (var track in _tracks)
					{
						if (track.Owned)
							ReleaseQuietly(track);
					}
				}
			}

			_holding = hold && error is null && _tracks.Exists(t => t.Owned);
			if (!_holding)
				StopObservingLifetime();

			if (error is null)
			{
				_completion.TrySetResult(result);
			}
			else
			{
				LogFault<StoryboardRun>(error);
				if (_completion.TrySetException(error))
					_ = _completion.Task.Exception;
			}

			var finished = error is null && (result == AnimationResult.Completed || result == AnimationResult.ReducedMotionSkipped);
			SetState(finished ? PlaybackState.Finished : PlaybackState.Stopped);
			if (error is null)
				_player.RaiseCompleted(result, _replacedTracks);
		}

		void FinishTrack(Track track, bool keepEnd, bool hold, bool commitCurrent, double endProgress)
		{
			if (keepEnd && track.Owned)
			{
				// Exact end (rule 5), then keep it: held at animation rank, or committed beneath the hold at the
				// rank of the value it replaces (rule 6), so nothing is raised at release.
				var end = track.Sample(endProgress, _defaultEasing);
				var shown = AnimationLayer.Write(track.Target, track.Property, end);

				// A hold needs the end at animation rank, which can't be written while the property is mid-set:
				// then the end is committed instead.
				if (!hold || !shown)
				{
					AnimationLayer.CommitAndRelease(track.Target, track.Property, end, this);
					track.Owned = false;
				}
			}
			else if (keepEnd && track.Excluded && !hold)
			{
				// Rule 8: a run that commits its end writes an excluded property once at handler rank, as if the
				// user had edited the value. A held fill writes nothing to it.
				track.Target.SetValueForAnimation(track.Property, track.Sample(endProgress, _defaultEasing), SetterSpecificity.FromHandler);
			}
			else if (track.Owned)
			{
				if (commitCurrent)
					AnimationLayer.CommitAndRelease(track.Target, track.Property, track.Target.GetValue(track.Property), this);
				else
					AnimationLayer.Release(track.Target, track.Property, this);

				track.Owned = false;
			}
		}

		// Best effort after an exception: whatever this run still holds is released.
		void ReleaseQuietly(Track track)
		{
			track.Owned = false;
			try
			{
				AnimationLayer.Release(track.Target, track.Property, this);
			}
			catch (Exception exception)
			{
				LogFault<StoryboardRun>(exception);
			}
		}

		void ReleaseHeldFill(bool commit)
		{
			_holding = false;
			StopObservingLifetime();
			Exception? error = null;
			foreach (var track in _tracks)
			{
				if (!track.Owned)
					continue;

				try
				{
					if (commit)
						AnimationLayer.CommitAndRelease(track.Target, track.Property, track.Target.GetValue(track.Property), this);
					else
						AnimationLayer.Release(track.Target, track.Property, this);

					track.Owned = false;
				}
				catch (Exception exception)
				{
					error ??= exception;
					ReleaseQuietly(track);
				}
			}

			if (error is not null)
				LogFault<StoryboardRun>(error);

			SetState(PlaybackState.Stopped);
		}

		void SetState(PlaybackState state)
		{
			if (_state == state)
				return;

			var old = _state;
			_state = state;
			_player.RaiseStateChanged(old, state);
		}

		// The target's Unloaded and its window's destruction end the run, also while it's paused or holding a
		// fill, until it holds nothing.
		void ObserveLifetime()
		{
			if (!_observingUnload)
			{
				_observingUnload = true;
				_target.Unloaded += OnTargetUnloaded;
			}

			if (_explicitClock is not null)
				ObserveClock(_explicitClock);
			else if (_target.Window is not null)
				ObserveClock(AnimationClocks.For(_target));
		}

		void StopObservingLifetime()
		{
			StopObservingClock();
			if (!_observingUnload)
				return;

			_observingUnload = false;
			_target.Unloaded -= OnTargetUnloaded;
		}

		void OnTargetUnloaded(object? sender, EventArgs e) => StopCore(StopBehavior.Release);

		static List<Track> BuildTracks(Storyboard storyboard, VisualElement playTarget, TimeSpan duration)
		{
			var frames = storyboard.KeyFrames;
			var cues = ResolveCues(frames, duration);
			var tracks = new List<Track>();

			for (int k = 0; k < frames.Count; k++)
			{
				var frame = frames[k];
				foreach (var setter in frame.Setters)
				{
					var property = setter.Property ?? throw new InvalidOperationException("A keyframe setter has no Property.");
					var targetName = !string.IsNullOrEmpty(setter.TargetName) ? setter.TargetName : storyboard.TargetName;

					BindableObject target = playTarget;
					if (!string.IsNullOrEmpty(targetName))
					{
						target = Setter.FindTargetByName(playTarget, targetName!)
							?? throw new InvalidOperationException($"TargetName '{targetName}' doesn't resolve in the name scope of {playTarget} (MAUIA1004).");
					}

					object? value = setter.Value;
					if (value is BindingBase || value is DynamicResource)
						throw new NotSupportedException("This prototype doesn't sample {Binding} or {DynamicResource} keyframe values yet.");

					if (!property.TryConvert(ref value))
						throw new InvalidOperationException($"Cannot convert '{setter.Value}' to {property.ReturnType} for {property.PropertyName}.");

					Track? track = null;
					foreach (var existing in tracks)
					{
						if (ReferenceEquals(existing.Target, target) && existing.Property == property)
						{
							track = existing;
							break;
						}
					}

					if (track is null)
					{
						track = new Track(target, property);
						tracks.Add(track);
					}
					else if (track.Cues[track.Cues.Count - 1] == cues[k])
					{
						throw new InvalidOperationException($"Two keyframes at the same cue set {property.PropertyName} (MAUIX2302).");
					}

					track.Cues.Add(cues[k]);
					track.Values.Add(value);
					track.Easings.Add(frame.Easing);
				}
			}

			return tracks;
		}

		// Cues resolve in one pass in declaration order. Runs of unspecified cues are placed evenly between
		// their bounds; a leading run starts at 0 and a trailing run ends at 1.
		static double[] ResolveCues(IList<KeyFrame> frames, TimeSpan duration)
		{
			int count = frames.Count;
			var cues = new double[count];
			var specified = new bool[count];
			double previous = double.NegativeInfinity;

			for (int i = 0; i < count; i++)
			{
				var cue = frames[i].Cue;
				if (cue.IsUnspecified)
					continue;

				var fraction = cue.HasTimeSpan ? (double)cue.TimeSpan.Ticks / duration.Ticks : cue.Fraction;
				if (fraction < 0 || fraction > 1)
					throw new InvalidOperationException($"Keyframe cue {cue} is outside the storyboard's duration (MAUIX2301).");
				if (fraction < previous)
					throw new InvalidOperationException($"Keyframe cues must not decrease; {cue} comes after a later cue (MAUIX2306).");

				cues[i] = fraction;
				specified[i] = true;
				previous = fraction;
			}

			if (count > 0 && Array.TrueForAll(specified, s => !s))
			{
				for (int i = 0; i < count; i++)
					cues[i] = count == 1 ? 0 : (double)i / (count - 1);
				return cues;
			}

			int index = 0;
			while (index < count)
			{
				if (specified[index])
				{
					index++;
					continue;
				}

				int end = index;
				while (end + 1 < count && !specified[end + 1])
					end++;

				int first = index, last = end;
				double low, high;
				if (index == 0)
				{
					cues[first++] = 0;
					low = 0;
				}
				else
				{
					low = cues[index - 1];
				}

				if (end == count - 1)
				{
					cues[last--] = 1;
					high = 1;
				}
				else
				{
					high = cues[end + 1];
				}

				int runLength = last - first + 1;
				for (int k = 0; k < runLength; k++)
					cues[first + k] = low + (high - low) * (k + 1) / (runLength + 1);

				index = end + 1;
			}

			return cues;
		}
	}
}
