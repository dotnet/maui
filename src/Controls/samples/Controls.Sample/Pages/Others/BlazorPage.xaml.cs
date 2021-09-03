using Maui.Controls.Sample.Controls;
using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Components.Web;
using Maui.Controls.Sample.Pages.Others;
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
			var verticalStack = new VerticalStackLayout() { Spacing = 5, BackgroundColor = Colors.Purple, };
			verticalStack.Add(new Label { Text = "The content below is brought to you by Blazor!", FontSize = 24, TextColor = Colors.BlanchedAlmond, HorizontalOptions = LayoutOptions.Center });

			var bwv = new BlazorWebView
			{
				// General properties
				BackgroundColor = Colors.Orange,
				HeightRequest = 400,
				MinimumHeightRequest = 400,
				VerticalOptions = LayoutOptions.FillAndExpand,

				// BlazorWebView properties
				HostPage = @"wwwroot/index.html",
			};
			bwv.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Main) });

			bwv.RootComponents.RegisterForJavaScript<MyDynamicComponent>("my-dynamic-root-component");

			verticalStack.Add(bwv);

			verticalStack.Add(new Label { Text = "Thank you for using Blazor and .NET MAUI!", FontSize = 24, TextColor = Colors.BlanchedAlmond, HorizontalOptions = LayoutOptions.Center });

			Content = verticalStack;
#endif
		}
	}
}
