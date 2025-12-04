using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class TimePickerPage
	{
		public TimePickerPage()
		{
			InitializeComponent();

			UpdateTimePickerBackground();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			IsOpenTimePicker.Opened += IsOpenTimePickerOpened;
			IsOpenTimePicker.Closed += IsOpenTimePickerClosed;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			IsOpenTimePicker.Opened -= IsOpenTimePickerOpened;
			IsOpenTimePicker.Closed -= IsOpenTimePickerClosed;
		}

		void OnUpdateBackgroundButtonClicked(object sender, EventArgs e)
		{
			UpdateTimePickerBackground();
		}

		void OnClearBackgroundButtonClicked(object sender, EventArgs e)
		{
			BackgroundTimePicker.Background = null;
		}

		void UpdateTimePickerBackground()
		{
			Random rnd = new Random();
			Color startColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			Color endColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

			BackgroundTimePicker.Background = new LinearGradientBrush
			{
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = startColor },
					new GradientStop { Color = endColor, Offset = 1 }
				}
			};
		}

		void SetTimePickerToNull(object sender, EventArgs e)
		{
			NullTimePicker.Time = null;
		}

		void SetTimePickerToNow(object sender, EventArgs e)
		{
			NullTimePicker.Time = DateTime.Now.TimeOfDay;
		}

		void OnOpenClicked(object sender, EventArgs e)
		{
			IsOpenTimePicker.IsOpen = true;
		}

		void OnCloseClicked(object sender, EventArgs e)
		{
			IsOpenTimePicker.IsOpen = false;
		}

		void IsOpenTimePickerOpened(object? sender, TimePickerOpenedEventArgs e)
		{
			Console.WriteLine("IsOpenTimePicker Opened");
		}

		void IsOpenTimePickerClosed(object? sender, TimePickerClosedEventArgs e)
		{
			Console.WriteLine("IsOpenTimePicker Closed");
		}
	}
}