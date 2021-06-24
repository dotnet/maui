using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls.Core.UnitTests;

namespace Controls.Core.UnitTests.TestClasses
{
	public class TestAnimationManager : IAnimationManager
	{
		public TestAnimationManager()
		{
			Ticker = new BlockingTicker();
			Ticker.Fire = OnFire;
		}

		public double SpeedModifier { get; set; } = 1;
		ITicker ticker;
		public ITicker Ticker {
			get => ticker;
			set
			{
				var wasRunning = ticker?.IsRunning ?? false;
				ticker?.Stop();
				ticker = value;
				ticker.Fire = OnFire;
				if (wasRunning)
					ticker?.Start();
			}
		}
		public bool AutoStartTicker { get; set; }
		List<Animation> Animations = new List<Animation>();
		public void Add(Animation animation)
		{
			Animations.Add(animation);
			if (AutoStartTicker && !Ticker.IsRunning)
				Ticker.Start();
		}

		public void Remove(Animation animation)
		{
			Animations.Remove(animation);
			if (!Animations.Any())
				Ticker.Stop();
		}

		void OnFire()
		{
			var animations = Animations.ToList();
			void animationTick(Animation animation)
			{
				if (animation.HasFinished)
				{
					Animations.Remove(animation);
					animation.RemoveFromParent();
					return;
				}

				animation.Tick(16);
				if (animation.HasFinished)
				{
					Animations.Remove(animation);
					animation.RemoveFromParent();
				}
			}
			animations.ForEach(animationTick);

			if (!Animations.Any())
				Ticker.Stop();
		}
	}
}
