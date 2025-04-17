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

		void OnUpdateBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			UpdateDatePickerBackground();
		}

		void OnClearBackgroundButtonClicked(object sender, System.EventArgs e)
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

		void OnFocusDatePickerFocused(object sender, Microsoft.Maui.Controls.FocusEventArgs e)
		{
			Debug.WriteLine("Focused");
		}

		void OnFocusDatePickerUnfocused(object sender, Microsoft.Maui.Controls.FocusEventArgs e)
		{
			Debug.WriteLine("Unfocused");
		}

		void OnDateSelected(System.Object sender, Microsoft.Maui.Controls.DateChangedEventArgs e)
		{
			Debug.WriteLine("Date Selected");
			DateSelectedText.Text = String.Format("Selected: {0}", e.NewDate.ToShortDateString());
		}

		void OnDateChanged(System.Object sender, Microsoft.Maui.Controls.DateChangedEventArgs e)
		{
			Debug.WriteLine("Date Changed");
			DateChangedText.Text = String.Format("Changed: {0}", e.NewDate.ToShortDateString());
		}

		void OnClearPickerEventText(object sender, System.EventArgs e)
		{
			Debug.WriteLine("Resetting Date Selected/Changed Label Text");
			DateSelectedText.Text = "Selected: ";
			DateChangedText.Text = "Changed: ";
		}
	}
}