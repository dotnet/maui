#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// One run of a <see cref="Storyboard"/> against one element.
	/// </summary>
	/// <remarks>
	/// <para>Exactly one run owns a given (target, property) at a time. A run that starts on a property another run
	/// owns takes it over in place, continuing from the value on screen (<see cref="StoryboardHandoffBehavior"/>).
	/// A run whose every property is taken over, by another run, by user input or by a TwoWay binding, completes
	/// with <see cref="AnimationResult.Replaced"/>.</para>
	/// <para>All members must be called on the target's UI thread. The target is held weakly: a run whose target is
	/// collected, unloaded or loses its window stops.</para>
	/// </remarks>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class StoryboardPlayer : IDisposable, IAnimationHoldOwner
	{
		static readonly TimeSpan ShortenCap = TimeSpan.FromMilliseconds(100);

		readonly WeakReference<VisualElement> _target;
		readonly IStoryboardClock? _explicitClock;
		readonly PlaybackOptions _options;
		readonly ReducedMotionBehavior _reducedMotion;
		readonly CompiledStoryboard _compiled;
		readonly SlotState[] _slots;
		readonly Contribution[] _scratch;
		readonly TaskCompletionSource<AnimationResult> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
		readonly List<BindableProperty> _replaced = new();

		IStoryboardClock? _clock;
		IMotionPolicy? _policy;
		bool _started;
		bool _ticking;
		bool _observingLifetime;
		bool _observingPolicy;
		bool _waitingForWindow;
		bool _observingClock;
		bool _claiming;
		bool _ended;
		double _rate = 1;
		double _timeTicks;
		TimeSpan _lastClock;
		double? _pendingSeekTicks;
		double _shorten = 1;
		PlaybackState _state = PlaybackState.Created;

		internal StoryboardPlayer(Storyboard storyboard, VisualElement target, IStoryboardClock? clock, PlaybackOptions options)
		{
			Storyboard = storyboard;
			_target = new WeakReference<VisualElement>(target);
			_explicitClock = clock;
			_options = options;
			_reducedMotion = storyboard.ReducedMotion;

			// The definition is snapshotted here: later edits affect only later runs.
			_compiled = CompiledStoryboard.Compile(storyboard, target, options);
			_slots = new SlotState[_compiled.Slots.Count];
			for (var i = 0; i < _slots.Length; i++)
				_slots[i] = new SlotState(_compiled.Slots[i]);
			_scratch = new Contribution[_slots.Length];

			if (options.FillCompletion == FillCompletion.Hold && storyboard.FillMode != AnimationFillMode.Forward && storyboard.FillMode != AnimationFillMode.Both)
				options.FillCompletion = FillCompletion.Commit;
		}

		/// <summary>The storyboard this run plays.</summary>
		public Storyboard Storyboard { get; }

		/// <summary>The element the run plays against; <see langword="null"/> once it has been collected.</summary>
		public VisualElement? Target => _target.TryGetTarget(out var target) ? target : null;

		/// <summary>The playback state.</summary>
		public PlaybackState State => _state;

		/// <summary>Progress through the current iteration, 0..1.</summary>
		public double Progress
		{
			get
			{
				var root = _compiled.Root;
				var (tau, _, _, _) = root.Locate(_timeTicks);
				return root.Duration > 0 ? tau / root.Duration : 1;
			}
		}

		/// <summary>The time within the current iteration.</summary>
		public TimeSpan CurrentTime => TimeSpan.FromTicks((long)Math.Round(_compiled.Root.Locate(_timeTicks).Tau));

		/// <summary>The total time since the run started, across iterations.</summary>
		public TimeSpan ElapsedTime => TimeSpan.FromTicks((long)Math.Round(_timeTicks));

		/// <summary>The zero-based iteration the run is in.</summary>
		public long CurrentIteration => _compiled.Root.Locate(_timeTicks).Iteration;

		/// <summary>How fast the run plays; negative plays backwards. Zero is not allowed.</summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when set to zero, NaN or infinity.</exception>
		public double PlaybackRate
		{
			get => _rate;
			set
			{
				if (value == 0 || double.IsNaN(value) || double.IsInfinity(value))
					throw new ArgumentOutOfRangeException(nameof(value), value, "PlaybackRate must be a non-zero finite number.");

				VerifyAccess();
				_rate = value;
			}
		}

		/// <summary>Completes with how the run ended. Never canceled; faults when an interpolator, coercion callback or
		/// property-changed handler throws.</summary>
		public Task<AnimationResult> Completion => _completion.Task;

		/// <summary>Raised when <see cref="State"/> changes.</summary>
		public event EventHandler<PlaybackStateChangedEventArgs>? StateChanged;

		/// <summary>Raised once when the run ends, however it ends.</summary>
		public event EventHandler<StoryboardCompletedEventArgs>? Completed;

		/// <summary>Starts a created player, or resumes a paused one. Reduced motion applies here.</summary>
		public void Resume()
		{
			VerifyAccess();
			if (_ended)
				return;

			try
			{
				if (!_started && !Start())
					return;

				if (_state == PlaybackState.Running)
					return;

				if (ApplyMotionPolicy())
					return;

				SetState(PlaybackState.Running);

				// A StateChanged handler may have paused or stopped the run.
				if (!_ended && _state == PlaybackState.Running)
					StartTicking();
			}
			catch (Exception ex)
			{
				// A synchronous call propagates its fault, but never leaves values or ownership behind.
				FaultSynchronously(ex);
				throw;
			}
		}

		/// <summary>Pauses the run. Its values stay on screen.</summary>
		public void Pause()
		{
			VerifyAccess();
			if (_ended || _state != PlaybackState.Running)
				return;

			StopTicking();
			SetState(PlaybackState.Paused);
		}

		/// <summary>Moves to <paramref name="progress"/> of the storyboard's active duration on the next frame.</summary>
		public void Seek(double progress, TimeSeekOrigin origin = TimeSeekOrigin.BeginTime)
		{
			if (double.IsNaN(progress))
				throw new ArgumentOutOfRangeException(nameof(progress));

			var end = SeekableEnd();
			SeekToTicks(origin == TimeSeekOrigin.Duration ? end - progress * end : progress * end, now: false);
		}

		/// <summary>Moves to <paramref name="time"/> on the next frame.</summary>
		public void SeekTo(TimeSpan time, TimeSeekOrigin origin = TimeSeekOrigin.BeginTime)
			=> SeekToTicks(origin == TimeSeekOrigin.Duration ? SeekableEnd() - time.Ticks : time.Ticks, now: false);

		/// <summary>Moves to <paramref name="time"/> now, synchronously. On a created player this starts the run without
		/// playing it, for gesture scrubbing; seeking is direct manipulation and isn't subject to reduced motion.</summary>
		public void SeekAlignedToLastTick(TimeSpan time, TimeSeekOrigin origin = TimeSeekOrigin.BeginTime)
			=> SeekToTicks(origin == TimeSeekOrigin.Duration ? SeekableEnd() - time.Ticks : time.Ticks, now: true);

		/// <summary>Jumps to the end state and completes normally. A forever run has no end state and is released.</summary>
		public void SkipToFill()
		{
			VerifyAccess();
			if (_ended)
				return;

			if (!_started && !Start())
				return;

			try
			{
				if (_compiled.IsForever)
				{
					ReleaseAll();
					End(AnimationResult.Stopped);
					return;
				}

				// Playing backwards, the end of the run is its start.
				if (_rate < 0)
				{
					_timeTicks = 0;
					ApplyStartFill();
				}
				else
				{
					_timeTicks = _compiled.EndTicks;
					ApplyEndFill();
				}
				End(AnimationResult.Completed, finished: true);
			}
			catch (Exception ex)
			{
				FaultSynchronously(ex);
				throw;
			}
		}

		/// <summary>Ends the run without an exception. By default the animated values are removed.</summary>
		public void Stop(StopBehavior behavior = StopBehavior.Release)
		{
			VerifyAccess();

			// A run that has ended holding its fill can still be released.
			if (_ended && !HoldsAnything())
				return;

			try
			{
			switch (behavior)
			{
				case StopBehavior.ApplyFill:
					if (!_ended)
					{
						if (_rate < 0)
						{
							_timeTicks = 0;
							ApplyStartFill();
						}
						else
						{
							if (!_compiled.IsForever)
								_timeTicks = _compiled.EndTicks;
							ApplyEndFill();
						}
					}
					break;
				case StopBehavior.CommitCurrent:
					CommitShown();
					break;
				default:
					ReleaseAll();
					break;
			}
			}
			catch (Exception ex)
			{
				FaultSynchronously(ex);
				throw;
			}

			if (!_ended)
				End(AnimationResult.Stopped);
			else if (!HoldsAnything())
				StopObservingLifetime();
		}

		/// <summary>Stops the run with <see cref="StopBehavior.Release"/>.</summary>
		public void Dispose()
		{
			if (_ended && !HoldsAnything())
				return;

			Stop(StopBehavior.Release);
		}

		internal bool HoldsAnything()
		{
			foreach (var slot in _slots)
			{
				if (slot.Owned && !slot.Lost)
					return true;
			}
			return false;
		}

		internal static void VerifyAccess(BindableObject target)
		{
			// In a process with no dispatcher (a unit test), affinity isn't checked.
			if (target.GetAnimationDispatcher() is IDispatcher dispatcher && dispatcher.IsDispatchRequired)
				throw new InvalidOperationException("Storyboard players must be created and used on the target's UI thread.");
		}

		void VerifyAccess()
		{
			if (Target is VisualElement target)
				VerifyAccess(target);
		}

		double SeekableEnd()
			=> _compiled.IsForever ? _compiled.Root.Begin + _compiled.Root.Duration / _compiled.Root.Speed : _compiled.EndTicks;

		void SeekToTicks(double ticks, bool now)
		{
			VerifyAccess();
			if (_ended)
				return;

			ticks = Math.Max(0, _compiled.IsForever ? ticks : Math.Min(ticks, _compiled.EndTicks));

			// A paused run doesn't tick, so a seek on it applies straight away.
			if (_state == PlaybackState.Paused)
				now = true;

			if (!now)
			{
				_pendingSeekTicks = ticks;
				return;
			}

			if (!_started && !Start())
				return;

			_pendingSeekTicks = null;
			_timeTicks = ticks;
			_lastClock = _clock?.Elapsed ?? TimeSpan.Zero;
			Apply();
		}

		// Claims the tracks, snapshots implicit starts and starts observing the target's lifetime.
		bool Start()
		{
			_started = true;
			var target = Target;
			if (target is null)
			{
				End(AnimationResult.Stopped);
				return false;
			}

			// Claiming tells previous owners, whose handlers can start other runs that take properties from this one.
			// While claiming, a loss is only recorded; whether anything is left is decided once every slot is claimed.
			_claiming = true;
			try
			{
				// A previous owner's handler can stop this run mid-loop; nothing is claimed after that.
				for (var i = 0; i < _slots.Length && !_ended; i++)
					Claim(_slots[i]);
			}
			finally
			{
				_claiming = false;
			}

			if (_ended)
				return false;

			var anyOwned = false;
			foreach (var slot in _slots)
				anyOwned |= !slot.Lost && (slot.Owned || slot.Excluded);

			if (!anyOwned)
			{
				var anyTakenOver = _replaced.Count > 0;
				End(anyTakenOver ? AnimationResult.Replaced
					: _options.HandoffBehavior == StoryboardHandoffBehavior.Ignore ? AnimationResult.Ignored : AnimationResult.Stopped);
				return false;
			}

			// Implicit starts come from the value on screen now, so a run that takes over mid-flight neither jumps nor notifies.
			_compiled.SnapshotImplicitStarts(slot => slot.Target?.GetValue(slot.Property));

			if (_rate < 0)
			{
				if (_compiled.IsForever)
					throw new InvalidOperationException("A forever storyboard has no end to play backwards from.");
				_timeTicks = _compiled.EndTicks;
			}

			StartObservingLifetime(target);
			return true;
		}

		// Claims one slot: owned, excluded (never interpolated) or lost (no target, or left to its owner under Ignore).
		bool Claim(SlotState slot)
		{
			if (slot.Slot.Target is not BindableObject target)
			{
				slot.Lost = true;
				return false;
			}

			var currentOwner = target.GetAnimationOwner(slot.Slot.Property);
			if (currentOwner is not null && !ReferenceEquals(currentOwner, this) && _options.HandoffBehavior == StoryboardHandoffBehavior.Ignore)
			{
				slot.Lost = true;
				return false;
			}

			// Recorded first: claiming tells the previous owner, whose handler may throw, and fault cleanup must still
			// know this run owns the property.
			slot.Owned = true;
			if (!target.ClaimAnimation(slot.Slot.Property, this))
			{
				slot.Owned = false;
				slot.Excluded = true; // TwoWay / OneWayToSource-bound or platform-written: never interpolated; a commit writes once.
			}

			return true;
		}

		// Returns true when the policy ended the run.
		bool ApplyMotionPolicy()
		{
			var target = Target;
			_policy ??= MotionPolicy.Resolve(target);
			var instant = MotionPolicy.IsInstant(_policy);

			_shorten = 1;
			if (instant || (_policy.IsReducedMotionRequested && _reducedMotion != ReducedMotionBehavior.Play))
			{
				if (!instant && _reducedMotion == ReducedMotionBehavior.Shorten && !_compiled.IsForever)
				{
					var total = _compiled.EndTicks / Math.Abs(_rate);
					var shortened = Math.Min(total / 10, ShortenCap.Ticks);
					if (shortened < 1)
					{
						// Too short to shorten (under a tick): it completes at once, as a normal completion would.
						_timeTicks = _rate < 0 ? 0 : _compiled.EndTicks;
						if (_rate < 0)
							ApplyStartFill();
						else
							ApplyEndFill();
						End(AnimationResult.Completed, finished: true);
						return true;
					}

					_shorten = total / shortened;
				}
				else
				{
					SkipForReducedMotion();
					return true;
				}
			}
			else if (_reducedMotion != ReducedMotionBehavior.Play && _policy.DurationScale > 0 && _policy.DurationScale != 1)
			{
				// The platform's animator duration scale applies exactly as it does to native animators.
				_shorten = 1 / _policy.DurationScale;
			}

			if (!_observingPolicy && !instant)
			{
				_observingPolicy = true;
				_policy.Changed += OnPolicyChanged;
			}

			return false;
		}

		void SkipForReducedMotion()
		{
			if (_compiled.IsForever)
			{
				ReleaseAll();
			}
			else
			{
				_timeTicks = _rate < 0 ? 0 : _compiled.EndTicks;
				if (_rate < 0)
					ApplyStartFill();
				else
					ApplyEndFill();
			}

			End(AnimationResult.ReducedMotionSkipped, finished: !_compiled.IsForever);
		}

		void OnPolicyChanged(object? sender, EventArgs e)
		{
			// Turning reduced motion on mid-flight applies it to the run in flight.
			if (_ended || _state != PlaybackState.Running)
				return;

			// No caller: a fault here ends this run like a frame failure and never escapes into the policy's event.
			try
			{
				ApplyMotionPolicy();
			}
			catch (Exception ex)
			{
				FaultFromCallback(ex);
			}
		}

		void StartTicking()
		{
			if (_ticking)
				return;

			var target = Target;
			_clock ??= ResolveClock(target);
			if (_clock is null)
			{
				// A forever run on a target with no window waits for one.
				WaitForWindow(target);
				return;
			}

			_ticking = true;
			_lastClock = _clock.Elapsed;
			_clock.Tick += OnTick;
			if (!_observingClock && _clock is AnimationFrameClock frameClock)
			{
				// Once per run, however often it pauses and resumes; removed when the run ends.
				_observingClock = true;
				frameClock.Destroyed += OnClockDestroyed;
			}
		}

		void StopTicking()
		{
			if (!_ticking || _clock is null)
				return;

			_ticking = false;
			_clock.Tick -= OnTick;
		}

		IStoryboardClock? ResolveClock(VisualElement? target)
		{
			if (_explicitClock is not null)
				return _explicitClock;

			if (AnimationFrameClock.TestClock is IStoryboardClock testClock)
				return testClock;

			if (target?.Window is Window window && window.GetAnimationClock() is { } windowClock)
				return windowClock;

			// Finite runs on a target with no window run on the application clock, so their tasks complete.
			return _compiled.IsForever ? null : AnimationFrameClock.GetApplicationClock();
		}

		void WaitForWindow(VisualElement? target)
		{
			if (target is null || _waitingForWindow)
				return;

			// Wait for the target to get a window. Not Loaded: a natively hosted view can be loaded (and Loaded fires
			// immediately for a loaded view) with no Controls window at all.
			_waitingForWindow = true;
			((IControlsVisualElement)target).WindowChanged += OnWindowChangedWaitingForWindow;
		}

		void OnWindowChangedWaitingForWindow(object? sender, EventArgs e)
		{
			if (sender is not VisualElement target || target.Window is null)
				return;

			((IControlsVisualElement)target).WindowChanged -= OnWindowChangedWaitingForWindow;
			_waitingForWindow = false;
			if (!_ended && _state == PlaybackState.Running)
				StartTicking();
		}

		void OnTick(object? sender, EventArgs e)
		{
			if (_ended || _clock is null || _state != PlaybackState.Running)
				return;

			var target = Target;
			if (target is null || (_clock is AnimationFrameClock && !ReferenceEquals(_clock, AnimationFrameClock.TestClock)
				&& target.Window is null && _explicitClock is null && !ReferenceEquals(_clock, AnimationFrameClock.ApplicationClockIfCreated)))
			{
				ReleaseAll();
				End(AnimationResult.Stopped);
				return;
			}

			var now = _clock.Elapsed;
			var delta = (now - _lastClock).Ticks;
			_lastClock = now;

			if (_pendingSeekTicks is double seek)
			{
				_pendingSeekTicks = null;
				_timeTicks = seek;
			}
			else
			{
				_timeTicks += delta * _rate * _shorten;
			}

			try
			{
				if (_rate > 0 && !_compiled.IsForever && _timeTicks >= _compiled.EndTicks)
				{
					_timeTicks = _compiled.EndTicks;
					ApplyEndFill();
					End(AnimationResult.Completed, finished: true);
					return;
				}

				if (_rate < 0 && _timeTicks <= 0)
				{
					_timeTicks = 0;
					ApplyStartFill();
					End(AnimationResult.Completed, finished: true);
					return;
				}

				Apply();
			}
			catch (Exception ex)
			{
				FaultFromCallback(ex);
			}
		}

		void OnClockDestroyed(object? sender, EventArgs e)
		{
			// Teardown with no caller: the run reaches its terminal state even when a release throws.
			try
			{
				ReleaseAll();
			}
			catch (Exception ex)
			{
				Target?.FindMauiContext()?.CreateLogger<StoryboardPlayer>()?.LogError(ex, "MAUIA1005: A storyboard run faulted while its window was destroyed.");
			}
			finally
			{
				StopObservingLifetime();
				EndQuietly(AnimationResult.Stopped);
			}
		}

		// Writes every slot's value at the current time. Writes to one target are batched so transform changes reach
		// the platform once per frame.
		void Apply()
		{
			_compiled.Evaluate(_timeTicks, _scratch);
			Write(_scratch, commit: false);
		}

		void Write(Contribution[] values, bool commit, bool final = false)
		{
			VisualElement? batched = null;
			try
			{
				var wasEnded = _ended;
				for (var i = 0; i < _slots.Length; i++)
				{
					// A property callback stopped the run (Stop, Dispose, a fault): leave the remaining slots alone.
					if (_ended && !wasEnded)
						break;

					var slot = _slots[i];
					if (!slot.Lost && (slot.Owned || slot.Excluded) && slot.Slot.Target is BindableObject target)
					{
						if (!ReferenceEquals(target, batched))
						{
							batched?.EndAnimationFrame();
							batched = target as VisualElement;
							batched?.BeginAnimationFrame();
						}

						WriteSlot(slot, target, ref values[i], commit, final);
					}
				}
			}
			finally
			{
				batched?.EndAnimationFrame();
			}
		}

		void WriteSlot(SlotState slot, BindableObject target, ref Contribution contribution, bool commit, bool final)
		{
			var property = slot.Slot.Property;

			if (slot.Excluded)
			{
				// A TwoWay-bound or platform-written property gets one handler-rank write, only when the run commits its end.
				if (commit && contribution.Kind != ContributionKind.None)
				{
					target.SetValue(property, contribution.Value, SetterSpecificity.FromHandler);
					LogExcluded(property);
				}
				return;
			}

			if (contribution.Kind == ContributionKind.None)
			{
				if (final)
				{
					// The run has ended and contributes nothing here: give the property up entirely.
					target.ClearAnimationValue(property, this);
					slot.Held = false;
					slot.Owned = false;
					return;
				}

				// Before the node starts (no backward fill) or after it ends (no forward fill): show the base value,
				// but keep owning the property.
				if (slot.Held)
				{
					target.ClearAnimationValue(property, this);
					slot.Held = false;

					// The release's callbacks may have stopped the run (it reclaims nothing) or given the property to another
					// run (it lost the property, and must not take it back).
					if (!_ended && slot.Owned)
					{
						if (target.GetAnimationOwner(property) is null)
							target.ClaimAnimation(property, this);
						else
							((IAnimationHoldOwner)this).OnHoldLost(target, property, AnimationHoldLossReason.TakenOver);
					}
				}
				return;
			}

			if (commit)
			{
				// A source-preserving commit: the value lands in the slot of the base value and the hold, if any, is released.
				// When the property is being set (rule 13), the kernel queues it behind the current write.
				target.CommitAnimationValue(property, contribution.Value, this);

				slot.Held = false;
				slot.Owned = false;
				slot.Shown = contribution.Value;
				target.ClearAnimationValue(property, this);
				return;
			}

			switch (target.SetAnimationValue(property, contribution.Value, this))
			{
				case AnimationWriteResult.Written:
					slot.Held = true;
					slot.Shown = contribution.Value;
					break;
				case AnimationWriteResult.Excluded:
					slot.Excluded = true;
					break;
				case AnimationWriteResult.Lost:
					slot.Lost = true;
					slot.Held = false;
					break;
				// Refused: the property is being set; this frame's value is retried on the next tick.
			}
		}

		void ApplyEndFill() => ApplyFill(start: false);

		void ApplyStartFill() => ApplyFill(start: true);

		void ApplyFill(bool start)
		{
			// The fill compiled when the run started: later edits to the definition affect only later runs.
			var fill = _compiled.Root.Fill;
			var keeps = start
				? fill == AnimationFillMode.Backward || fill == AnimationFillMode.Both
				: fill == AnimationFillMode.Forward || fill == AnimationFillMode.Both;

			if (!keeps)
			{
				ReleaseAll();
				return;
			}

			_compiled.EvaluateEdge(start, _scratch);
			if (_options.FillCompletion == FillCompletion.Hold)
				Write(_scratch, commit: false, final: true);
			else
				Write(_scratch, commit: true, final: true);
		}

		void CommitShown()
		{
			for (var i = 0; i < _slots.Length; i++)
			{
				var slot = _slots[i];
				_scratch[i] = slot.Held
					? new Contribution { Kind = ContributionKind.Active, Value = slot.Shown }
					: default;
			}

			Write(_scratch, commit: true, final: true);
			ReleaseAll();
		}

		void ReleaseAll()
		{
			List<Exception>? errors = null;

			// Every release is attempted, even when one release's handler throws.
			foreach (var slot in _slots)
			{
				if (slot.Owned && !slot.Lost)
				{
					slot.Owned = false;
					slot.Held = false;
					try
					{
						slot.Slot.Target?.ClearAnimationValue(slot.Slot.Property, this);
					}
					catch (Exception ex)
					{
						(errors ??= new List<Exception>()).Add(ex);
					}
				}
			}

			if (errors is not null)
				throw new AggregateException(errors);
		}

		// A fault before the run ended releases and faults it. After it ended (a StateChanged/Completed handler of this
		// run threw), the run is already final: nothing is released and the exception just propagates to the caller.
		void FaultSynchronously(Exception exception)
		{
			if (!_ended)
				Fault(exception);
		}

		void Fault(Exception exception)
		{
			try
			{
				ReleaseAll();
			}
			catch (Exception releaseException)
			{
				exception = new AggregateException(exception, releaseException);
			}

			Target?.FindMauiContext()?.CreateLogger<StoryboardPlayer>()?.LogError(exception, "MAUIA1005: A storyboard run faulted.");
			EndCore(PlaybackState.Stopped);
			StopObservingLifetime();
			_completion.TrySetException(exception);
		}

		void End(AnimationResult result, bool finished = false)
		{
			if (_ended)
				return;

			// The run reaches its terminal state (task completed, lifetime observation dropped) even when a StateChanged
			// handler throws; the handler's exception still propagates afterwards.
			Exception? stateError = null;
			try
			{
				EndCore(finished ? PlaybackState.Finished : PlaybackState.Stopped);
			}
			catch (Exception ex)
			{
				stateError = ex;
			}

			// A fill held at animation rank keeps observing the target's lifetime until it is released.
			if (!HoldsAnything())
				StopObservingLifetime();

			_completion.TrySetResult(result);

			// Raised once however the run ends, even when a StateChanged handler threw; that exception propagates after.
			Completed?.Invoke(this, new StoryboardCompletedEventArgs(result, _replaced.ToArray()));

			if (stateError is not null)
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(stateError).Throw();
		}

		// Ends the run from a path with no caller of its own (another run's takeover, Unloaded, window teardown): a
		// StateChanged or Completed handler that throws is logged instead of escaping into that path.
		void EndQuietly(AnimationResult result)
		{
			try
			{
				End(result);
			}
			catch (Exception ex)
			{
				Target?.FindMauiContext()?.CreateLogger<StoryboardPlayer>()?.LogError(ex, "A storyboard Completed or StateChanged handler threw.");
			}
		}

		// A fault on a callback with no caller (frame, policy change): a live run is faulted; once the run has ended, the
		// exception came from its own StateChanged/Completed handlers and is only logged.
		void FaultFromCallback(Exception exception)
		{
			if (!_ended)
			{
				Fault(exception);
				return;
			}

			Target?.FindMauiContext()?.CreateLogger<StoryboardPlayer>()?.LogError(exception, "A storyboard Completed or StateChanged handler threw.");
		}

		void EndCore(PlaybackState state)
		{
			_ended = true;
			StopTicking();

			if (_observingClock && _clock is AnimationFrameClock frameClock)
			{
				_observingClock = false;
				frameClock.Destroyed -= OnClockDestroyed;
			}

			if (_observingPolicy && _policy is not null)
			{
				_observingPolicy = false;
				_policy.Changed -= OnPolicyChanged;
			}

			if (_waitingForWindow && Target is VisualElement target)
			{
				_waitingForWindow = false;
				((IControlsVisualElement)target).WindowChanged -= OnWindowChangedWaitingForWindow;
			}

			SetState(state);
		}

		void SetState(PlaybackState state)
		{
			if (_state == state)
				return;

			var old = _state;
			_state = state;
			StateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(old, state));
		}

		void StartObservingLifetime(VisualElement target)
		{
			if (_observingLifetime)
				return;

			_observingLifetime = true;
			target.Unloaded += OnTargetUnloaded;
		}

		void StopObservingLifetime()
		{
			if (!_observingLifetime)
				return;

			_observingLifetime = false;
			if (Target is VisualElement target)
				target.Unloaded -= OnTargetUnloaded;
		}

		void OnTargetUnloaded(object? sender, EventArgs e)
		{
			// A teardown with no caller of its own: a fault never escapes into the Unloaded event.
			try
			{
				ReleaseAll();
			}
			catch (Exception ex)
			{
				Target?.FindMauiContext()?.CreateLogger<StoryboardPlayer>()?.LogError(ex, "MAUIA1005: A storyboard run faulted while its target unloaded.");
			}

			StopObservingLifetime();
			EndQuietly(AnimationResult.Stopped);
		}

		void LogExcluded(BindableProperty property)
			=> Target?.FindMauiContext()?.CreateLogger<StoryboardPlayer>()?.LogInformation(
				"MAUIA1003: {Property} has an active TwoWay or OneWayToSource binding or a platform-written value; the final value was applied without interpolation.", property.PropertyName);

		void IAnimationHoldOwner.OnHoldLost(BindableObject target, BindableProperty property, AnimationHoldLossReason reason)
		{
			var anyLeft = false;
			foreach (var slot in _slots)
			{
				if (ReferenceEquals(slot.Slot.Target, target) && ReferenceEquals(slot.Slot.Property, property))
				{
					if (!slot.Lost && slot.Owned)
						_replaced.Add(property);
					slot.Lost = true;
					slot.Held = false;
					slot.Owned = false;
				}
				else if (!slot.Lost && (slot.Owned || slot.Excluded))
				{
					anyLeft = true;
				}
			}

			// While claiming, Start decides once every slot is claimed (a loss here must not end a half-claimed run).
			// Raised from another run's (or a write's) operation: this run's Completed/StateChanged handlers must not throw
			// into that caller, so they are isolated here.
			if (!anyLeft && !_claiming)
			{
				if (!_ended)
					EndQuietly(AnimationResult.Replaced);
				else
					StopObservingLifetime();
			}
		}

		void IAnimationHoldOwner.OnBaseValueChanged(BindableObject target, BindableProperty property)
		{
			// Storyboard values don't follow the base value; it shows when the run releases.
		}

		sealed class SlotState
		{
			public SlotState(TrackSlot slot) => Slot = slot;

			public TrackSlot Slot { get; }

			/// <summary>The run owns the property (it may or may not show a value).</summary>
			public bool Owned;

			/// <summary>The run shows a value at animation rank.</summary>
			public bool Held;

			/// <summary>Taken over, or skipped by the handoff behavior.</summary>
			public bool Lost;

			/// <summary>TwoWay-bound or platform-written: never interpolated.</summary>
			public bool Excluded;

			public object? Shown;
		}
	}

	/// <summary>Data for <see cref="StoryboardPlayer.StateChanged"/>.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class PlaybackStateChangedEventArgs : EventArgs
	{
		/// <summary>Creates the event data.</summary>
		public PlaybackStateChangedEventArgs(PlaybackState oldState, PlaybackState newState)
		{
			OldState = oldState;
			NewState = newState;
		}

		/// <summary>The state before the change.</summary>
		public PlaybackState OldState { get; }

		/// <summary>The state after the change.</summary>
		public PlaybackState NewState { get; }
	}

	/// <summary>Data for <see cref="StoryboardPlayer.Completed"/>.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class StoryboardCompletedEventArgs : EventArgs
	{
		/// <summary>Creates the event data.</summary>
		public StoryboardCompletedEventArgs(AnimationResult result, IReadOnlyList<BindableProperty> replacedTracks)
		{
			Result = result;
			ReplacedTracks = replacedTracks;
		}

		/// <summary>How the run ended.</summary>
		public AnimationResult Result { get; }

		/// <summary>The properties another run, user input or a TwoWay binding took over before the run ended.</summary>
		public IReadOnlyList<BindableProperty> ReplacedTracks { get; }
	}
}
