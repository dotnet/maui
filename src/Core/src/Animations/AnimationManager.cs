using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class AnimationManager : IAnimationManager, IDisposable
	{
		readonly List<Animation> _animations = new();
		long _lastUpdate;
		bool _disposedValue;

		/// <summary>
		/// Instantiate a new <see cref="AnimationManager"/> object.
		/// </summary>
		/// <param name="ticker">An instance of <see cref="ITicker"/> that will be used to time the animations.</param>
		public AnimationManager(ITicker ticker)
		{
			_lastUpdate = GetCurrentTick();

			Ticker = ticker;
			Ticker.Fire = OnFire;
		}

		/// <inheritdoc/>
		public ITicker Ticker { get; }

		/// <inheritdoc/>
		public double SpeedModifier { get; set; } = 1;

		/// <inheritdoc/>
		public bool AutoStartTicker { get; set; } = true;

		/// <inheritdoc/>
		public void Add(Animation animation)
		{
			// If animations are disabled, don't do anything
			if (!Ticker.SystemEnabled)
			{
				return;
			}

			if (!_animations.Contains(animation))
				_animations.Add(animation);
			if (!Ticker.IsRunning && AutoStartTicker)
				Start();
		}

		/// <inheritdoc/>
		public void Remove(Animation animation)
		{
			_animations.TryRemove(animation);

			if (_animations.Count == 0)
				End();
		}

		void Start()
		{
			_lastUpdate = GetCurrentTick();
			Ticker.Start();
		}

		void End() =>
			Ticker?.Stop();

		static long GetCurrentTick() =>
			Environment.TickCount & int.MaxValue;

		void OnFire()
		{
			if (!Ticker.SystemEnabled)
			{
				// This is a hack - if we're here, the ticker has detected that animations are no longer enabled,
				// and it's invoked the Fire event one last time because that's the only communication mechanism
				// it currently has available with the AnimationManager. We need to force all the running animations
				// to move to their finished state and stop running.

				ForceFinishAnimations();
				return;
			}

			var now = GetCurrentTick();
			var milliseconds = TimeSpan.FromMilliseconds(now - _lastUpdate).TotalMilliseconds;
			_lastUpdate = now;

			Animation[] animations = [.._animations];

			foreach (var animation in animations)
			{
				OnAnimationTick(animation);
			}

			if (_animations.Count == 0)
				End();

			void OnAnimationTick(Animation animation)
			{
				if (animation.HasFinished)
				{
					_animations.TryRemove(animation);
					animation.RemoveFromParent();
					return;
				}

				animation.Tick(AdjustSpeed(milliseconds));

				if (animation.HasFinished)
				{
					_animations.TryRemove(animation);
					animation.RemoveFromParent();
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				_disposedValue = true;

				if (disposing)
				{
					try
					{
						ForceFinishAnimations();
					}
					finally
					{
						// Always dispose the ticker, even if force-finishing an animation
						// threw or the drain safety cap was hit, so the manager doesn't
						// leak a running ticker on a failed teardown.
						if (Ticker is IDisposable disposable)
							disposable.Dispose();
					}
				}
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		void ForceFinishAnimations()
		{
			// Drain until _animations is empty (or the safety cap is hit). A Finished
			// callback invoked below may reentrantly queue a new animation on this same
			// manager (e.g. via AnimationExtensions.Add/Insert); a single snapshot would
			// leave that new entry un-finished and still rooted in the static tweener
			// registry, so we keep re-snapshotting until no new animations show up.
			const int MaxDrainPasses = 8;

			for (int pass = 0; pass < MaxDrainPasses && _animations.Count > 0; pass++)
			{
				Animation[] animations = [.._animations];

				foreach (var animation in animations)
				{
					ForceFinish(animation);
				}
			}

			// If a pathological handler kept re-adding animations past the safety cap,
			// force the collection empty so this manager can't be left holding entries
			// after Dispose returns.
			if (_animations.Count > 0)
				_animations.Clear();

			End();

			void ForceFinish(Animation animation)
			{
				try
				{
					// Isolate failures per-animation so one bad callback can't prevent
					// the remaining animations from being finished and removed, and
					// can't abort disposal before the ticker is disposed.
					animation.ForceFinish();
				}
				catch
				{
				}
				finally
				{
					_animations.TryRemove(animation);
					animation.RemoveFromParent();
				}
			}
		}

		internal virtual double AdjustSpeed(double elapsedMilliseconds)
		{
			return elapsedMilliseconds * SpeedModifier;
		}
	}
}