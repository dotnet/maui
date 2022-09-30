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

		/// <inheritdoc/>
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
				return;

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

		long GetCurrentTick() =>
			Environment.TickCount & int.MaxValue;

		void OnFire()
		{
			var now = GetCurrentTick();
			var milliseconds = TimeSpan.FromMilliseconds(now - _lastUpdate).TotalMilliseconds;
			_lastUpdate = now;

			var animations = new List<Animation>(_animations);
			animations.ForEach(OnAnimationTick);

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

				animation.Tick(milliseconds * SpeedModifier);

				if (animation.HasFinished)
				{
					_animations.TryRemove(animation);
					animation.RemoveFromParent();
				}
			}
		}

		/// <inheritdoc/>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing && Ticker is IDisposable disposable)
					disposable.Dispose();

				_disposedValue = true;
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}