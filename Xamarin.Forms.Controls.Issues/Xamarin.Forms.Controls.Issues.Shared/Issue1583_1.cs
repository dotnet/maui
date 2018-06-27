using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using System;
using System.Threading.Tasks;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1583, "WebView fails to load from urlwebviewsource with non-ascii characters (works with Uri)", PlatformAffected.iOS, issueTestNumber: 1)]
	public class Issue1583_1 : TestContentPage
	{
		WebView _webview;
		Label _label;

		protected override void Init()
		{
			_webview = new WebView
			{
				AutomationId = "webview",
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			_label = new Label { AutomationId = "label", Text = "label" };

			var hashButton = new Button { Text = "1:hash", HorizontalOptions = LayoutOptions.FillAndExpand, AutomationId = "hashButton" };
			hashButton.Clicked += (sender, args) => Load("https://github.com/xamarin/Xamarin.Forms/issues/2736#issuecomment-389443737");

			var unicodeButton = new Button { Text = "2:unicode", HorizontalOptions = LayoutOptions.FillAndExpand, AutomationId = "unicodeButton" };
			unicodeButton.Clicked += (sender, args) => Load("https://www.google.no/maps/place/Skøyen");

			var queryButton = new Button { Text = "3:query", HorizontalOptions = LayoutOptions.FillAndExpand, AutomationId = "queryButton" };
			queryButton.Clicked += (sender, args) => Load("https://www.google.com/search?q=http%3A%2F%2Fmicrosoft.com");

			var portButton = new Button { Text = "4:port", HorizontalOptions = LayoutOptions.FillAndExpand, AutomationId = "portButton" };
			portButton.Clicked += (sender, args) => Load("http://portquiz.net:666");

			Content = new StackLayout
			{
				Children =
				{
					_label,
					new StackLayout
					{
						Orientation = StackOrientation.Horizontal,
						Children = { hashButton, unicodeButton, queryButton, portButton  }
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

#if UITEST
		[Test]
		public void Issue1583Test ()
		{
			Task.Delay(TimeSpan.FromSeconds(3)).Wait();
			RunningApp.WaitForElement (q => q.Marked ("label"));
			Task.Delay(TimeSpan.FromSeconds(3)).Wait();
			RunningApp.Screenshot ("I didn't crash and i can see Skøyen");
			RunningApp.Tap("hashButton");
			Task.Delay(TimeSpan.FromSeconds(3)).Wait();
			RunningApp.Screenshot ("I didn't crash and i can see the GitHub comment #issuecomment-389443737");
			RunningApp.Tap("queryButton");
			Task.Delay(TimeSpan.FromSeconds(3)).Wait();
			RunningApp.Screenshot ("I didn't crash and i can see google search for http://microsoft.com");
			RunningApp.Tap("portButton");
			Task.Delay(TimeSpan.FromSeconds(3)).Wait();
			RunningApp.Screenshot ("I didn't crash and i can see that i visited port 666");
		}
#endif
	}
}
