using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Animations
{
	public class AnimationManager : IAnimationManager
	{
		public double SpeedModifier { get; set; } = 1;

		long lastUpdate;


		ITicker? _ticker;
		public ITicker Ticker {
			get => _ticker ?? (Ticker = new Ticker()); 
			set => setTicker(value);
		}
		void setTicker(ITicker ticker)
		{
			_ = ticker ?? throw new ArgumentNullException(nameof(ticker));

			var oldTicker = _ticker;
			if (oldTicker == ticker)
				return;

			var isRunning = oldTicker?.IsRunning ?? false;
			oldTicker?.Stop();
			_ticker = ticker;
			ticker.Fire = OnFire;
			if (isRunning)
				ticker.Start();
			lastUpdate = GetCurrentTick();
		}

		List<Animation> Animations = new List<Animation>();

		public void Add(Animation animation)
		{
			//If animations are disabled, don't do anything
			if (!Ticker.SystemEnabled)
			{
				return;
			}
			if (!Animations.Contains(animation))
				Animations.Add(animation);
			if (!Ticker.IsRunning)
				Start();
		}

		public void Remove(Animation animation)
		{
			Animations.TryRemove(animation);
			if (!Animations.Any())
				End();
		}

		void Start()
		{
			lastUpdate = GetCurrentTick();
			Ticker.Start();
		}

		long GetCurrentTick() => (Environment.TickCount & int.MaxValue);

		void End() => Ticker?.Stop();
		void OnFire()
		{
			var now = GetCurrentTick();
			var seconds = TimeSpan.FromMilliseconds((now - lastUpdate)).TotalSeconds;
			lastUpdate = now;
			var animations = Animations.ToList();
			void animationTick(Animation animation)
			{
				if (animation.HasFinished)
				{
					Animations.TryRemove(animation);
					animation.RemoveFromParent();
					return;
				}

				animation.Tick(seconds * SpeedModifier);
				if (animation.HasFinished)
				{
					Animations.TryRemove(animation);
					animation.RemoveFromParent();
				}
			}
			animations.ForEach(animationTick);

			if (!Animations.Any())
				End();
		}

	}
}
