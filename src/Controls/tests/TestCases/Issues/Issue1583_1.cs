using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1583, "WebView fails to load from urlwebviewsource with non-ascii characters (works with Uri)", PlatformAffected.iOS, issueTestNumber: 1)]
	public class Issue1583_1 : TestContentPage
	{
		WebView _webview;
		Label _label;

		protected override void Init()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			_webview = new WebView
			{
				AutomationId = "webview",
				VerticalOptions = LayoutOptions.FillAndExpand
			};
#pragma warning restore CS0618 // Type or member is obsolete
			_label = new Label { AutomationId = "label" };

#pragma warning disable CS0618 // Type or member is obsolete
			var hashButton = new Button { Text = "1:hash", HorizontalOptions = LayoutOptions.FillAndExpand, AutomationId = "hashButton" };
#pragma warning restore CS0618 // Type or member is obsolete
			hashButton.Clicked += (sender, args) => Load("https://github.com/xamarin/Xamarin.Forms/issues/2736#issuecomment-389443737");

#pragma warning disable CS0618 // Type or member is obsolete
			var unicodeButton = new Button { Text = "2:unicode", HorizontalOptions = LayoutOptions.FillAndExpand, AutomationId = "unicodeButton" };
#pragma warning restore CS0618 // Type or member is obsolete
			unicodeButton.Clicked += (sender, args) => Load("https://www.google.no/maps/place/Skøyen");

#pragma warning disable CS0618 // Type or member is obsolete
			var queryButton = new Button { Text = "3:query", HorizontalOptions = LayoutOptions.FillAndExpand, AutomationId = "queryButton" };
#pragma warning restore CS0618 // Type or member is obsolete
			queryButton.Clicked += (sender, args) => Load("https://www.google.com/search?q=http%3A%2F%2Fmicrosoft.com");

			Content = new StackLayout
			{
				Children =
				{
					_label,
					new StackLayout
					{
						Orientation = StackOrientation.Horizontal,
						Children = { hashButton, unicodeButton, queryButton }
					},
					_webview
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Load("https://www.google.no/maps/place/Skøyen");
		}

		void Load(string url)
		{
			_webview.Source = new UrlWebViewSource { Url = url };
			_label.Text = $"Loaded {url}";
		}
	}
}
