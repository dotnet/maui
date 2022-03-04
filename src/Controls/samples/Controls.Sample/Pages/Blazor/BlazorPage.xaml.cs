using Maui.Controls.Sample.Controls;
using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

#if NET6_0_OR_GREATER
using Maui.Controls.Sample.Pages.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Maui;
#endif

namespace Maui.Controls.Sample.Pages
{
	public partial class BlazorPage : BasePage
	{
		public BlazorPage()
		{
			InitializeComponent();

#if NET6_0_OR_GREATER
			var grid = new Grid() { VerticalOptions = LayoutOptions.Fill, BackgroundColor = Colors.Purple, };
			grid.AddRowDefinition(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			grid.AddRowDefinition(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			grid.AddRowDefinition(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

			var headerLabel = new Label { Text = "The content below is brought to you by Blazor!", FontSize = 24, TextColor = Colors.BlanchedAlmond, HorizontalOptions = LayoutOptions.Center, };
			grid.Add(headerLabel);
			Grid.SetRow(headerLabel, 0);

			// You can replace this BlazorWebView with CustomBlazorWebView to see loading custom static assets
			var bwv = new BlazorWebView
			{
				// General properties
				BackgroundColor = Colors.Orange,

				// BlazorWebView properties
				HostPage = @"wwwroot/index.html",
			};
			bwv.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Main) });
			bwv.RootComponents.RegisterForJavaScript<MyDynamicComponent>("my-dynamic-root-component");

			grid.Add(bwv);
			Grid.SetRow(bwv, 1);

			var footerLabel = new Label { Text = "Thank you for using Blazor and .NET MAUI!", FontSize = 24, TextColor = Colors.BlanchedAlmond, HorizontalOptions = LayoutOptions.Center, };
			grid.Add(footerLabel);
			Grid.SetRow(footerLabel, 2);

			Content = grid;
#endif
		}
	}
}
