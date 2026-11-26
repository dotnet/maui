using System;
using Microsoft.Maui.Controls.Animations;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	/// <summary>
	/// Runs every storyboard on a manual clock, with reduced motion off, so tests are synchronous and deterministic:
	/// no ticker, no window and no real time.
	/// </summary>
	public abstract class AnimationTestBase : BaseTestFixture
	{
		protected AnimationTestBase()
		{
			Clock = new ManualStoryboardClock();
			Motion = new TestMotionPolicy();
			AnimationFrameClock.TestClock = Clock;
			MotionPolicy.TestOverride = Motion;
		}

		protected ManualStoryboardClock Clock { get; }

		protected TestMotionPolicy Motion { get; }

		protected void Advance(double milliseconds) => Clock.Advance(TimeSpan.FromMilliseconds(milliseconds));

		protected override void Dispose(bool disposing)
		{
			AnimationFrameClock.TestClock = null;
			MotionPolicy.TestOverride = null;
			base.Dispose(disposing);
		}

		protected static TimeSpan Ms(double milliseconds) => TimeSpan.FromMilliseconds(milliseconds);

		/// <summary>A linear 0 → 1 opacity storyboard of <paramref name="duration"/> ms.</summary>
		protected static Storyboard Fade(double from, double to, double duration = 100, AnimationFillMode fill = AnimationFillMode.Forward)
			=> Storyboard.Create()
				.WithDuration(Ms(duration))
				.WithFillMode(fill)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, from).At(1, to))
				.Build();

		protected sealed class TestMotionPolicy : IMotionPolicy
		{
			bool _reduced;
			double _scale = 1;

			public bool IsReducedMotionRequested
			{
				get => _reduced;
				set
				{
					_reduced = value;
					Changed?.Invoke(this, EventArgs.Empty);
				}
			}

			public double DurationScale
			{
				get => _scale;
				set
				{
					_scale = value;
					Changed?.Invoke(this, EventArgs.Empty);
				}
			}

			public event EventHandler Changed;
		}

		/// <summary>Counts change notifications for one property.</summary>
		protected sealed class ChangeCounter
		{
			public ChangeCounter(BindableObject target, string propertyName)
			{
				target.PropertyChanged += (_, e) =>
				{
					if (e.PropertyName == propertyName)
						Changed++;
				};
				target.PropertyChanging += (_, e) =>
				{
					if (e.PropertyName == propertyName)
						Changing++;
				};
			}

			public int Changed { get; private set; }

			public int Changing { get; private set; }
		}

		/// <summary>An owner that records how it lost its hold.</summary>
		internal sealed class TestOwner : IAnimationHoldOwner
		{
			public AnimationHoldLossReason? LostReason { get; private set; }

			public int BaseValueChanges { get; private set; }

			public void OnHoldLost(BindableObject target, BindableProperty property, AnimationHoldLossReason reason) => LostReason = reason;

			public void OnBaseValueChanged(BindableObject target, BindableProperty property) => BaseValueChanges++;
		}
	}
}
