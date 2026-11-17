using System;
using System.Threading;
using Microsoft.Maui.Controls.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	// Window clocks, without the test clock override. Nothing subscribes to these clocks, so they never need a
	// dispatcher; on this TFM their time is a Stopwatch, so these tests wait a little real time.
	[Collection(AnimationsCollection.Name)]
	public class AnimationClockTests : AnimationTestBase
	{
		[Fact]
		public void DestroyedWindowGetsAFreshClockThatRuns()
		{
			AnimationClocks.ClockOverride = null;
			try
			{
				var box = CreateWindowedBox();
				var window = (IWindow)box.Window;
				var first = AnimationClocks.For(box);
				Assert.Same(first, AnimationClocks.For(box));

				window.Stopped();
				window.Destroying();

				var second = AnimationClocks.For(box);
				Assert.NotSame(first, second);
				var start = second.Elapsed;
				Thread.Sleep(30);
				Assert.True(second.Elapsed > start, "The new clock should not be suspended.");
			}
			finally
			{
				AnimationClocks.ClockOverride = Clock;
			}
		}

		[Fact]
		public void StoppedWindowSuspendsItsClockAndActivationResumesIt()
		{
			AnimationClocks.ClockOverride = null;
			try
			{
				var box = CreateWindowedBox();
				var window = (IWindow)box.Window;
				var clock = AnimationClocks.For(box);

				window.Stopped();
				var frozen = clock.Elapsed;
				Thread.Sleep(100);
				Assert.Equal(frozen, clock.Elapsed);

				window.Activated();

				// The stopped time doesn't count, and the clock runs again.
				Assert.True(clock.Elapsed - frozen < TimeSpan.FromMilliseconds(50), $"The clock jumped by {clock.Elapsed - frozen}.");
				var resumed = clock.Elapsed;
				Thread.Sleep(30);
				Assert.True(clock.Elapsed > resumed, "The clock should run after the window is activated.");
			}
			finally
			{
				AnimationClocks.ClockOverride = Clock;
			}
		}
	}
}
