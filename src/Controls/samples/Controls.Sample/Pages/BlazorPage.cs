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
			var verticalStack = new VerticalStackLayout() { Spacing = 5, BackgroundColor = Colors.Purple, };
			verticalStack.Add(new Label { Text = "The content below is brought to you by Blazor!", FontSize = 24, TextColor = Colors.BlanchedAlmond, HorizontalOptions = LayoutOptions.Center });

			var bwv = new BlazorWebView
			{
				// General properties
				BackgroundColor = Colors.Orange,
				HeightRequest = 400,
				MinimumHeightRequest = 400,
				VerticalOptions=LayoutOptions.FillAndExpand,

				// BlazorWebView properties
				HostPage = @"wwwroot/index.html",
			};
			bwv.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Main) });
			verticalStack.Add(bwv);

			verticalStack.Add(new Label { Text = "Thank you for using Blazor and .NET MAUI!", FontSize = 24, TextColor = Colors.BlanchedAlmond, HorizontalOptions = LayoutOptions.Center });

			Content = verticalStack;
		}
	}
}
#endif
