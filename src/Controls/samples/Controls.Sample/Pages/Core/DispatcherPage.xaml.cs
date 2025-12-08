using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace Maui.Controls.Sample.Pages
{
	public partial class DispatcherPage
	{
		public DispatcherPage()
		{
			InitializeComponent();
		}

		async void OnFailAccessClicked(object sender, EventArgs e)
		{
			try
			{
				await Task.Run(() =>
				{
					failLabel.Text = "Oops!";
				});
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync("EXCEPTION", ex.Message, "OK");
			}
		}

		async void OnAccessClicked(object sender, EventArgs e)
		{
			try
			{
				await Task.Run(async () =>
				{
					await happyLabel.Dispatcher.DispatchAsync(() =>
					{
						happyLabel.Text = "This was a success!";
					});
				});
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync("EXCEPTION", ex.Message, "OK");
			}
		}

		void OnLaterClicked(object sender, EventArgs e)
		{
			var now = DateTime.Now;
			laterLabel.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(3), () =>
			{
				var later = DateTime.Now;
				laterLabel.Text = "I happened 3 seconds later! " + (later - now);
			});
			laterLabel.Text = "Started!";
		}

		IDispatcherTimer? _timer;

		void OnTimerClicked(object sender, EventArgs e)
		{
			if (_timer != null)
			{
				_timer.Stop();
				_timer = null;
				timerLabel.Text = "Stopped!";
				return;
			}

			var now = DateTime.Now;
			var counter = 0;

			_timer = timerLabel.Dispatcher.CreateTimer();

			_timer.Interval = TimeSpan.FromSeconds(3);
			_timer.IsRepeating = true;
			_timer.Start();

			timerLabel.Text = "Started!";

			_timer.Tick += (_, _) =>
			{
				var later = DateTime.Now;
				counter++;
				timerLabel.Text = $"I am on a 3 second timer! {counter} ticks => {later - now}";
			};
		}

		bool keepRunning;

		[Obsolete]
		void OnObsoleteClicked(object sender, EventArgs e)
		{
			if (keepRunning)
			{
				keepRunning = false;
				obsoleteLabel.Text = "Stopping!";
				return;
			}

			var now = DateTime.Now;
			var counter = 0;

			keepRunning = true;
			Device.StartTimer(TimeSpan.FromSeconds(3), () =>
			{
				if (!keepRunning)
				{
					obsoleteLabel.Text = "Stopped!";
					return false;
				}

				var later = DateTime.Now;
				counter++;
				obsoleteLabel.Text = $"I am on a 3 second timer! {counter} ticks => {later - now}";

				return true;
			});

			obsoleteLabel.Text = "Started!";
		}
	}
}