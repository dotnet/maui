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

		private void SetDatePickerToNull(object sender, EventArgs e)
		{
			NullDatePicker.Date = null;
		}

		private void SetDatePickerToToday(object sender, EventArgs e)
		{
			NullDatePicker.Date = DateTime.Now;
		}
    }
}