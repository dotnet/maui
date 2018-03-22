using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1583, "WebView fails to load from urlwebviewsource with non-ascii characters (works with Uri)", PlatformAffected.iOS, issueTestNumber: 1)]
	public class Issue1583 : TestContentPage
	{
		protected override void Init()
		{
			var webview = new WebView
			{
				AutomationId = "webview"	
			};
			webview.Source = new UrlWebViewSource { Url = "https://www.google.no/maps/place/Skøyen" };

			Content = webview;
		}

#if UITEST
		[Test]
		public void Issue1583Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("webview"), "Could not find webview", System.TimeSpan.FromSeconds(60), null, null);
			RunningApp.Screenshot ("I didn't crash and i can see Skøyen");
		}
#endif
	}
}
