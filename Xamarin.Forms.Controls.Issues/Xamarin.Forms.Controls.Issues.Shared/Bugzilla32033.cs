using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32033, "WebView on Windows does not display local HTML files", PlatformAffected.WinRT)]
	public class Bugzilla32033 : TestNavigationPage 
	{
		protected override void Init ()
		{
			PushAsync(Menu());
		}

		ContentPage Menu()
		{
			var page = new ContentPage();

			var layout = new StackLayout();

			var buttonLocal = new Button() {Text = "Local HTML file"};
			buttonLocal.Clicked += (sender, args) => Navigation.PushAsync(LocalUrl());
			
			var buttonHtmlString = new Button() {Text = "HTML string with links/refs to local files"};
			buttonHtmlString.Clicked += (sender, args) => Navigation.PushAsync(HtmlString());

			var buttonHtmlStringNoHead = new Button() {Text = "HTML string with links/refs to local files (no <head>)"};
			buttonHtmlStringNoHead.Clicked += (sender, args) => Navigation.PushAsync(HtmlStringNoHead());

			layout.Children.Add(buttonLocal);
			layout.Children.Add(buttonHtmlString);
			layout.Children.Add(buttonHtmlStringNoHead);

			page.Content = layout;

			return page;
		}

		static ContentPage LocalUrl()
		{
			var page = new ContentPage();

			var instructions = new Label
			{
				Text = @"The WebView below should contain the heading 'Xamarin Forms' and text reading 'This is a local HTML page'. All text should be italicized."
			};

			var webView = new WebView
			{
				WidthRequest = 300,
				HeightRequest = 500,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Source = new UrlWebViewSource() { Url = "local.html" }
			};

			var layout = new StackLayout { Children = { instructions, webView } };
			page.Content = layout;

			return page;
		}

		static ContentPage HtmlString()
		{
			var page = new ContentPage();

			var instructions = new Label
			{
				Text =
@"The WebView below should contain the heading 'Xamarin Forms', display the Xamarin logo, and have a link labeled 'next page'.
Clicking that link should navigate to a page with the heading 'Xamarin Forms' and text reading 'This is a local HTML page'. All text on both pages should be italicized."
			};

			var webView = new WebView
			{
				WidthRequest = 300,
				HeightRequest = 500,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Source = new HtmlWebViewSource
				{
					Html = @"<html>
<head>
<link rel=""stylesheet"" href=""default.css"">
</head>
<body>
<h1>Xamarin.Forms</h1>
<p>The CSS and image are loaded from local files!</p>
<img src='WebImages/XamarinLogo.png'/>
<p><a href=""local.html"">next page</a></p>
</body>
</html>"
				}
			};


			var layout = new StackLayout {HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill,  Children = { instructions, webView } };
			page.Content = layout;

			return page;
		}

		// This test verifies that the <base> injection solution works even if the HTML string doesn't explicitly include a <head> section
		static ContentPage HtmlStringNoHead()
		{
			var page = new ContentPage();

			var instructions = new Label
			{
				Text =
@"The WebView below should contain the heading 'Xamarin Forms', display the Xamarin logo, and have a link labeled 'next page'.
Clicking that link should navigate to a page with the heading 'Xamarin Forms' and text reading 'This is a local HTML page'. "
			};

			var webView = new WebView
			{
				WidthRequest = 300,
				HeightRequest = 500,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Source = new HtmlWebViewSource
				{
					Html = @"<html>
<body>
<h1>Xamarin.Forms</h1>
<p>The CSS and image are loaded from local files!</p>
<img src='WebImages/XamarinLogo.png'/>
<p><a href=""local.html"">next page</a></p>
</body>
</html>"
				}
			};


			var layout = new StackLayout {HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill,  Children = { instructions, webView } };
			page.Content = layout;

			return page;
		}
	}
}
