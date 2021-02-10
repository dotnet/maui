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
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12259, "App crash when rendering label with FormattedText", PlatformAffected.macOS)]
	public class Issue12259 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var label = new Label();

			var fs = new FormattedString();

			fs.Spans.Add(new Span { Text = "Learn more at " });

			fs.Spans.Add(new Span { Text = "https://aka.ms/xamarin-quickstart ", FontAttributes = FontAttributes.Bold });

			label.FormattedText = fs;

			Content = label;
		}
	}
}