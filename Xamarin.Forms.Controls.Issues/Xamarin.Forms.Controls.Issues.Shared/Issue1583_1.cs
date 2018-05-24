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
		WebView webview;
		Label label;
		protected override void Init()
		{
			webview = new WebView
			{
				AutomationId = "webview",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			label = new Label();
			Content = new StackLayout()
			{
				Children = {
					label,
					webview
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			label.Text = "Loaded";
			webview.Source = new UrlWebViewSource { Url = "https://www.google.no/maps/place/Skøyen" };

		}

#if UITEST
		[Test]
		public void Issue1583Test ()
		{
			RunningApp.WaitForElement(x=> x.Marked("Loaded"));
			RunningApp.WaitForElement (q => q.Marked ("webview"), "Could not find webview", System.TimeSpan.FromSeconds(60), null, null);
			RunningApp.Screenshot ("I didn't crash and i can see Skøyen");
		}
#endif
	}
}
