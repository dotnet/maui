#nullable disable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Performance;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class PerformanceWarningsPage : ContentPage
	{
		const double Period = 2000;
		
		readonly DateTime _now = DateTime.Now;
		IDispatcherTimer _timer;
		
		readonly IPerformanceProfiler _performanceProfiler;
		
		public PerformanceWarningsPage()
		{
			InitializeComponent();
			
			var serviceProvider = IPlatformApplication.Current?.Services;
			_performanceProfiler = serviceProvider?.GetService<IPerformanceProfiler>() 
			                       ?? throw new InvalidOperationException("IPerformanceProfiler service not found");

			_performanceProfiler.Warnings.WarningRaised += OnPerformanceWarning;
		}
		
		~PerformanceWarningsPage() => _timer.Tick -= OnTimerTick;
	
		protected override void OnAppearing()
		{
			base.OnAppearing();

			_timer = Dispatcher.CreateTimer();
			_timer.Interval = TimeSpan.FromMilliseconds(15);
			_timer.Tick += OnTimerTick;
			_timer.Start();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_timer.Stop();
		}

		void OnTimerTick(object sender, EventArgs e)
		{
			TimeSpan elapsed = DateTime.Now - _now;
			double t = (elapsed.TotalMilliseconds % Period) / Period;
			t = 2 * (t < 0.5 ? t : 1 - t);

			AbsoluteLayout.SetLayoutBounds(label1, new Rect(t, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
			AbsoluteLayout.SetLayoutBounds(label2,
				new Rect(0.5, 1 - t, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
		}
		
		void OnPerformanceWarning(object sender, PerformanceWarningEventArgs e)
		{
			if (e.Warning.Level != PerformanceWarningLevel.Info)
			{
				DisplayAlert("Performance Issue", $" Information: {e.Warning.Message}, Recommendation: {e.Warning.Recommendation}", "OK");
			}
		}
	}
}