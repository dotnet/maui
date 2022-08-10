using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			BindingContext = this;
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			var disp = DeviceDisplay.MainDisplayInfo;

			const int newWidth = 800;
			const int newHeight = 600;

			Window.XRequest = (disp.Width / disp.Density - newWidth) / 2;
			Window.YRequest = (disp.Height / disp.Density - newHeight) / 2;

			Window.WidthRequest = newWidth;
			Window.HeightRequest = newHeight;
		}
	}
}