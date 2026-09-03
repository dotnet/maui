using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class DatePickerPage
	{
		public DatePickerPage()
		{
			InitializeComponent();

			UpdateDatePickerBackground();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			IsOpenDatePicker.Opened += IsOpenDatePickerOpened;
			IsOpenDatePicker.Closed += IsOpenDatePickerClosed;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			IsOpenDatePicker.Opened -= IsOpenDatePickerOpened;
			IsOpenDatePicker.Closed -= IsOpenDatePickerClosed;
		}

		void OnUpdateBackgroundButtonClicked(object sender, EventArgs e)
		{
			UpdateDatePickerBackground();
		}

		void OnClearBackgroundButtonClicked(object sender, EventArgs e)
		{
			BackgroundDatePicker.Background = null;
		}

		void UpdateDatePickerBackground()
		{
			Random rnd = new Random();
			Color startColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			Color endColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

			BackgroundDatePicker.Background = new LinearGradientBrush
			{
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = startColor },
					new GradientStop { Color = endColor, Offset = 1 }
				}
			};
		}

		void OnFocusDatePickerFocused(object sender, FocusEventArgs e)
		{
			Debug.WriteLine("Focused");
		}

		void OnFocusDatePickerUnfocused(object sender, FocusEventArgs e)
		{
			Debug.WriteLine("Unfocused");
		}

		void SetDatePickerToNull(object sender, EventArgs e)
		{
			NullDatePicker.Date = null;
		}

		void SetDatePickerToToday(object sender, EventArgs e)
		{
			NullDatePicker.Date = DateTime.Now;
		}

		void OnOpenClicked(object sender, EventArgs e)
		{
			IsOpenDatePicker.IsOpen = true;
		}

		void OnCloseClicked(object sender, EventArgs e)
		{
			IsOpenDatePicker.IsOpen = false;
		}

		void IsOpenDatePickerOpened(object? sender, DatePickerOpenedEventArgs e)
		{
			Console.WriteLine("IsOpenDatePicker Opened");
		}

		void IsOpenDatePickerClosed(object? sender, DatePickerClosedEventArgs e)
		{
			Console.WriteLine("IsOpenDatePicker Closed");
		}
	}
}