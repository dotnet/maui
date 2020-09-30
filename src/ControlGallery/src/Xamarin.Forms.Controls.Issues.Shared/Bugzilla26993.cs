using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.UwpIgnore)]
	[Category(UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 26993, "https://bugzilla.xamarin.com/show_bug.cgi?id=26993")]
	public class Bugzilla26993 : TestContentPage // or TestFlyoutPage, etc ...
	{
		[Preserve(AllMembers = true)]
		public class Bz26993ViewCell : ViewCell
		{
			public static int s_id = 0;

			public Bz26993ViewCell()
			{
				View = new WebView
				{
					AutomationId = "AutomationId" + s_id,
					HeightRequest = 300,
					Source = new HtmlWebViewSource
					{
						Html = "<html><head><link rel=\"stylesheet\" href=\"default.css\"></head><body><h1 id=\"CellID" + s_id + "\">Xamarin.Forms " + s_id + "</h1><p>The CSS and image are loaded from local files!</p><img src='WebImages/XamarinLogo.png'/><p><a id=\"LinkID" + s_id++ + "\" href=\"local.html\">next page</a></p></body></html>"
					}
				};
			}
		}

		protected override void Init()
		{
			Bz26993ViewCell.s_id = 0;

			var itemSource = new List<string> {
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
			};

			Content = new StackLayout
			{
				Children = {
					new ListView {
						RowHeight = 300,
						ItemsSource = itemSource,
						ItemTemplate = new DataTemplate (typeof(Bz26993ViewCell))
					}
				}
			};
		}

#if UITEST
		[Test]
#if __MACOS__
		[Ignore("Webview query is not implemented yet on UITEst desktop")]
#endif
		public void Bugzilla26993Test()
		{
			RunningApp.Screenshot("I am at BZ26993");

#if __IOS__
			RunningApp.WaitForElement(q => q.Class("WKWebView"), postTimeout: TimeSpan.FromSeconds(5));

			RunningApp.Query(q => q.Class("WKWebView").Index(0).InvokeJS("document.getElementById('LinkID0').click()"));
			RunningApp.Screenshot("Load local HTML");

			RunningApp.WaitForNoElement(q => q.Class("WKWebView").Css("#LinkID0"));
			UITest.Queries.AppWebResult[] newElem =
			  RunningApp.QueryUntilPresent(() => RunningApp.Query(q => q.Class("WKWebView").Css("a")));

			Assert.AreEqual("#LocalHtmlPageLink", newElem[0].Id);
#elif __ANDROID__
			RunningApp.WaitForElement(q => q.WebView(0).Css("#CellID0"));
			RunningApp.Tap(q => q.WebView(0).Css("#LinkID0"));

			RunningApp.Screenshot("Load local HTML");

			RunningApp.WaitForNoElement(q => q.WebView(0).Css("#LinkID0"));

			UITest.Queries.AppWebResult[] newElem =
			  RunningApp.QueryUntilPresent(() => RunningApp.Query(q => q.WebView(0).Css("h1")));

			Assert.AreEqual("#LocalHtmlPage", newElem[0].Id);
#endif

			RunningApp.Screenshot("I see the Label");
		}
#endif
	}
}