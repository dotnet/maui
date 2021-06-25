using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TestAnimationManager : IAnimationManager
	{
		readonly List<Animations.Animation> _animations = new();

		public TestAnimationManager(ITicker ticker = null)
		{
			Ticker = ticker ?? new BlockingTicker();
			Ticker.Fire = OnFire;
		}

		public double SpeedModifier { get; set; } = 1;

		public bool AutoStartTicker { get; set; } = false;

		public ITicker Ticker { get; }

		public void Add(Animations.Animation animation)
		{
			_animations.Add(animation);
			if (AutoStartTicker && !Ticker.IsRunning)
				Ticker.Start();
		}

		public void Remove(Animations.Animation animation)
		{
			_animations.Remove(animation);
			if (!_animations.Any())
				Ticker.Stop();
		}

		void OnFire()
		{
			var animations = _animations.ToList();
			animations.ForEach(animationTick);

			if (!_animations.Any())
				Ticker.Stop();

			void animationTick(Animations.Animation animation)
			{
				if (animation.HasFinished)
				{
					_animations.Remove(animation);
					animation.RemoveFromParent();
					return;
				}

				animation.Tick(16);
				if (animation.HasFinished)
				{
					_animations.Remove(animation);
					animation.RemoveFromParent();
				}
			}
		}
	}
}