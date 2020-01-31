using System;
using System.Net;
using Xamarin.Forms.Internals;
using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3262, "Adding Cookies ability to a WebView...")]
	public class Issue3262 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			Label header = new Label
			{
				Text = "Check that a WebView can use Cookies...",
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
				HorizontalOptions = LayoutOptions.Center
			};

			try
			{
				CookieContainer cookieContainer = new CookieContainer();
				string url = "https://dotnet.microsoft.com/apps/xamarin";
				Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);

				Cookie cookie = new Cookie
				{
					Name = "TestCookie",
					Expires = DateTime.Now.AddDays(1),
					Value = "My Test Cookie...",
					Domain = uri.Host,
					Path = "/"
				};

				cookieContainer.Add(uri, cookie);

				WebView webView = new WebView
				{
					Source = url,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.FillAndExpand,
					Cookies = cookieContainer
				};

				Content = new StackLayout
				{
					Padding = new Thickness(20),
					Children =
					{
						header,
						webView
					}
				};
			}
			catch (Exception ex)
			{
				_ = ex.Message;
				throw;
			}
		}
	}
}