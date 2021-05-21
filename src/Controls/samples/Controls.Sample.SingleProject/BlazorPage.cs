using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;

namespace Maui.Controls.Sample.SingleProject
{
	public class BlazorPage : ContentPage, IPage
	{
		public BlazorPage()
		{
			var bwv = new BlazorWebView
			{
				// General properties
				BackgroundColor = Colors.Orange,
				HeightRequest = 400,
				MinimumHeightRequest = 400,

				// BlazorWebView properties
				HostPage = @"wwwroot/index.html",
			};
			bwv.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Main) });
			Content = bwv;
		}

		public IView View { get => (IView)Content; set => Content = (View)value; }
	}
}