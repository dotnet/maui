using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 35733, "iOS WebView crashes when loading an URL with encoded parameters", PlatformAffected.iOS)]
	public class Bugzilla35733 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var thisDoesNotWorkButton = new Button
			{
				Text = "This will crash",
				AutomationId = "btnGo"

			};
			thisDoesNotWorkButton.Clicked += async (object sender, EventArgs e) => await ShowLocation("KÅRA");

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = {
					thisDoesNotWorkButton
				}
			};
		}

		async Task ShowLocation(string locationString)
		{
			var stringUri = $"https://raw.githubusercontent.com/xamarin/Xamarin.Forms/main/README.md?l=en&px_location={Uri.EscapeDataString(locationString)}";

			var uri = new Uri(stringUri);
			var webPage = new ContentPage
			{
				Title = "WebViewTest",
				Content = new WebView
				{
					Source = uri
				}
			};
			await Navigation.PushAsync(webPage);
		}
	}
}
