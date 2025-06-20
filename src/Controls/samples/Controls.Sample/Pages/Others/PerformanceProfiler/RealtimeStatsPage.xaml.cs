#nullable disable
using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Performance;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class RealtimeStatsPage : ContentPage
	{
		const double Period = 2000;

		readonly IPerformanceProfiler _performanceProfiler;
		readonly DateTime _now = DateTime.Now;
		IDispatcherTimer _timer;
		
		readonly ObservableCollection<string> _items;

		public RealtimeStatsPage()
		{
			InitializeComponent();
			
			var serviceProvider = IPlatformApplication.Current?.Services;
			_performanceProfiler = serviceProvider?.GetService<IPerformanceProfiler>() 
			                       ?? throw new InvalidOperationException("IPerformanceProfiler service not found");
			
			_performanceProfiler.Layout.SubscribeToLayoutUpdates(OnLayoutUpdate);
			
			_items = new ObservableCollection<string>();
			RealtimeItems.ItemsSource = _items;
		}

		void OnLayoutUpdate(LayoutUpdate layoutUpdate)
		{
			MainThread.InvokeOnMainThreadAsync(() =>
			{
				_items.Add($"Element: {layoutUpdate.Element}, PassType: {layoutUpdate.PassType}, Duration: {layoutUpdate.TotalTime} ms");
			});
			
			// Scroll to the newly added item
			RealtimeItems.ScrollTo(_items.Count - 1, position: ScrollToPosition.End, animate: true);
		}

		~RealtimeStatsPage() => _timer.Tick -= OnTimerTick;

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
	}
}