using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
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
	}
}
