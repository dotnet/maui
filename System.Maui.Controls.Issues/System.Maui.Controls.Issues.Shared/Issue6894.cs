using System.Threading;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6894, "Accessibility tags are not working with WebView", PlatformAffected.iOS)]
	public class Issue6894 : TestContentPage 
	{
		protected override void Init()
		{
			var stack = new StackLayout
			{
				Children = {
					new Label { Text = "Turn on the Screen Reader. Swipe next to the WebView. You should be able to swipe between the elements on the webpage and hear the text announced. If not, this test has failed." },
					new WebView { Source = "https://microsoft.com", VerticalOptions = LayoutOptions.FillAndExpand }
				},
			};

			Content = stack;
		}

	}
}