using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class StackLayoutGallery : ContentPage
	{
		public StackLayoutGallery()
		{
			if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var stack = new StackLayout { Orientation = StackOrientation.Vertical };
			Button b1 = new Button { Text = "Boring", HeightRequest = 500, MinimumHeightRequest = 50 };
			Button b2 = new Button
			{
				Text = "Exciting!",
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			Button b3 = new Button { Text = "Amazing!", VerticalOptions = LayoutOptions.FillAndExpand };
			Button b4 = new Button { Text = "Meh", HeightRequest = 400, MinimumHeightRequest = 50 };
			b1.Clicked += (sender, e) =>
			{
				b1.Text = "clicked1";
			};
			b2.Clicked += (sender, e) =>
			{
				b2.Text = "clicked2";
			};
			b3.Clicked += (sender, e) =>
			{
				b3.Text = "clicked3";
			};
			b4.Clicked += (sender, e) =>
			{
				b4.Text = "clicked4";
			};
			stack.Children.Add(b1);
			stack.Children.Add(b2);
			stack.Children.Add(b3);
			stack.Children.Add(b4);
			Content = stack;
		}
	}
}
