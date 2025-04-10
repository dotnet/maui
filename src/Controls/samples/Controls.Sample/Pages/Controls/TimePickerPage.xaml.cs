﻿using System;
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

		void SetTImePickerToNull(object sender, EventArgs e)
		{
			NullTimePicker.Time = null;
		}

		void SetTimePickerToToday(object sender, EventArgs e)
		{
			NullTimePicker.Time = DateTime.Now.TimeOfDay;
		}
	}
}