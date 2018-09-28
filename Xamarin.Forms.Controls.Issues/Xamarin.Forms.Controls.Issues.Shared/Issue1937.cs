using System;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1937, "[UWP] Choppy animation", PlatformAffected.UWP)]
	public class Issue1937 : TestContentPage
	{
		protected override void Init()
		{
			var description = new Label
			{
				Text = "Fast timer have interval 20ms. Slow timer have interval 100ms. Both timers work for 1000ms. Start timer and check timer ticks."
			};
			var finishFlag = new Label();
			var resultDesription = new Label()
			{
				Text = "Timer ticks:"
			};
			var resultContainer = new Label()
			{
				AutomationId = "RESULT"
			};

			var fastTimerStartButton = new Button
			{
				Text = "FAST_TIMER"
			};

			fastTimerStartButton.Clicked += (_, __) =>
			{
				finishFlag.Text = "";
				var timerTicks = 0;
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				Device.StartTimer(TimeSpan.FromMilliseconds(20), () =>
				{
					timerTicks++;
					if (stopwatch.ElapsedMilliseconds < 1000)
						return true;
					resultContainer.Text = timerTicks.ToString();
					finishFlag.Text = "COMPLETE";
					return false;
				});
			};

			var slowTimerStartButton = new Button
			{
				Text = "SLOW_TIMER"
			};

			slowTimerStartButton.Clicked += (_, __) =>
			{
				finishFlag.Text = "";
				var timerTicks = 0;
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
				{
					timerTicks++;
					if (stopwatch.ElapsedMilliseconds < 1000)
						return true;
					resultContainer.Text = timerTicks.ToString();
					finishFlag.Text = "COMPLETE";
					return false;
				});
			};

			Content = new StackLayout
			{
				Children =
				{
					description,
					fastTimerStartButton,
					slowTimerStartButton,
					finishFlag,
					resultDesription,
					resultContainer
				}
			};
		}

#if UITEST && __WINDOWS__
		[Test]
		public void Issue1937Test ()
		{
			RunningApp.Tap(q => q.Marked("FAST_TIMER"));
			RunningApp.WaitForElement(q => q.Marked("COMPLETE"), timeout:TimeSpan.FromSeconds(2));
			var result = RunningApp.WaitForElement(q => q.Marked("RESULT"))[0];
			var timerTicks = int.Parse(result.Text);
			//If fps > 50 then result must be 50. For small fps we use comparing with 35.
			Assert.IsTrue(timerTicks > 35, $"Expected timer ticks are greater than 35. Actual: {timerTicks}");

			RunningApp.Tap(q => q.Marked("SLOW_TIMER"));
			RunningApp.WaitForElement(q => q.Marked("COMPLETE"), timeout:TimeSpan.FromSeconds(2));
			result = RunningApp.WaitForElement(q => q.Marked("RESULT"))[0];
			timerTicks = int.Parse(result.Text);
			Assert.IsTrue(timerTicks < 11, $"Expected timer ticks are less than 11. Actual: {timerTicks}");
		}
#endif
	}
}
