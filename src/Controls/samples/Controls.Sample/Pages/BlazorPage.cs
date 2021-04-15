#if NET6_0_OR_GREATER
using System;
using Maui.Controls.Sample.Controls;
using Maui.Controls.Sample.ViewModel;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public class BlazorPage : BasePage
	{
		readonly IServiceProvider _services;
		readonly MainPageViewModel _viewModel;

		public BlazorPage(IServiceProvider services, MainPageViewModel viewModel)
		{
			_services = services;
			BindingContext = _viewModel = viewModel;

			SetupMauiLayout();
		}

		void SetupMauiLayout()
		{
			var verticalStack = new StackLayout() { Spacing = 5, BackgroundColor = Colors.AntiqueWhite };
			verticalStack.Add(new Label { Text = "This should be TOP text!", FontSize = 24, HorizontalOptions = LayoutOptions.End });

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
			verticalStack.Add(bwv);
			verticalStack.Add(new Label { Text = "This should be BOTTOM text!", FontSize = 24, HorizontalOptions = LayoutOptions.End });

			Content = new ScrollView
			{
				Content = verticalStack
			};
		}
	}
}
