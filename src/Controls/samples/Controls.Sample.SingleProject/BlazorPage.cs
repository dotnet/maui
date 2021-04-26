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
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddBlazorWebView();
			//serviceCollection.AddSingleton<AppState>(_appState);

			var bwv = new BlazorWebView
			{
				BackgroundColor = Colors.Orange,
				Services = serviceCollection.BuildServiceProvider(),
				HeightRequest = 400,
				MinimumHeightRequest = 400,
				HostPage = @"wwwroot/index.html",
			};
			bwv.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Main) });
			Content = bwv;
		}

		public IView View { get => (IView)Content; set => Content = (View)value; }
	}
}