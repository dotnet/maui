using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.WebView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6286, "ObjectDisposedException in Android WebView.EvaluateJavascriptAsync ", PlatformAffected.Android)]
	public class Issue6286 : TestNavigationPage
	{
		WebView webview;
		WebView webview2;
		protected override void Init()
		{
			webview = new WebView { Source = "http://microsoft.com" };
			webview.Navigated += Webview_Navigated;
			var page1 = new ContentPage { Content = webview };

			webview2 = new WebView { Source = "http://xamarin.com" };
			webview2.Navigated += Webview_Navigated;
			var page2 = new ContentPage { Content = webview2 };

			int count = 0;
			Navigation.PushAsync(page1);
			Device.StartTimer(TimeSpan.FromSeconds(2), () =>
			{
				webview.Source = "http://xamarin.com";
				Navigation.PushAsync(page2);

				webview2.Source = "http://microsoft.com";
				Navigation.PopAsync();

				var done = count++ < 3;

				if (done)
					page1.Content = new Label { Text = "success", AutomationId = "success" };

				return done;
			});
		}

		void Webview_Navigated(object sender, WebNavigatedEventArgs e)
		{
			webview.EvaluateJavaScriptAsync("document.write('i executed this javascript woohoo');");
		}

#if UITEST
		[Test]
		public void Issue6286_WebView_Test()
		{	
			RunningApp.WaitForElement(q => q.Marked("success"));
		}
#endif
	}
}