using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.Helpers;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.WebView)]
	[Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6286, "ObjectDisposedException in Android WebView.EvaluateJavascriptAsync ", PlatformAffected.Android)]
	public class Issue6286 : TestNavigationPage
	{
		WebView webview;
		WebView webview2;
		ContentPage page1;
		ContentPage page2;

		protected override void Init()
		{
			webview = new WebView { Source = "https://microsoft.com" };
			webview.Navigated += Webview_Navigated;
			page1 = new ContentPage { Content = webview };

			webview2 = new WebView { Source = "https://xamarin.com" };
			webview2.Navigated += Webview_Navigated;
			page2 = new ContentPage { Content = webview2 };

			Navigation.PushAsync(page1);
			RunTest();
		}

		async void RunTest()
		{
			try
			{
				int count = 0;
				while (count < 3)
				{
					await Task.Delay(2000);
					count++;

					webview.Source = "https://xamarin.com";
					await Navigation.PushAsync(page2);

					webview2.Source = "https://microsoft.com";
					await Navigation.PopAsync();
				}

				page1.Content = new Label { Text = "success", AutomationId = "success" };

			}
			catch (Exception exc)
			{
				page1.Content = new Label { Text = $"{exc}", AutomationId = "failure" };
			}
		}

		void Webview_Navigated(object sender, WebNavigatedEventArgs e)
		{
			webview.EvaluateJavaScriptAsync("document.write('i executed this javascript woohoo');");

			webview2.EvaluateJavaScriptAsync("document.write('i executed this javascript woohoo');");
		}

#if UITEST
		[Test]
		public void Issue6286_WebView_Test()
		{
			RunningApp.QueryUntilPresent(() => RunningApp.WaitForElement("success"));
		}
#endif
	}
}