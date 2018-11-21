using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39963, "iOS WebView has wrong scrolling size when loading local html content with images")]
	public class Bugzilla39963 : TestContentPage 
	{
		protected override void Init()
		{

			var notWorkingHtml = @"<html><body>
						<p><img src='test.jpg' /></p>
						<p>After starting (not re-entering!) the app in landscape, scroll down to see a black area which is not supposed to be there.</p>
						<p>After starting (not re-entering!) the app in portrait, scroll to the right to see a black area which is not supposed to be there.</p>
						<p>This only happens when a local image is loaded.</p>
						</body></html>";

			var workingHtml = @"<html><body>
						<p></p>
						<p>Without local image, everything works fine.</p>
						</body></html>";

			WebView webView = new WebView {
				Source = new HtmlWebViewSource() {
					Html = notWorkingHtml
				},
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			Content = webView;
		}
	}
}
