using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 181, "Color not initialized for Label", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Issue181 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 181";
			Content = new Frame
			{
				BorderColor = Colors.Red,
				BackgroundColor = new Color(1.0f, 1.0f, 0.0f),
				Content = new Label
				{
					Text = "I should have red text",
					TextColor = Colors.Red,
					BackgroundColor = new Color(0.5f, 0.5f, 0.5f),
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				}
			};
		}

#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		[UiTest(typeof(Label), "TextColor")]
		public void Issue181TestsLabelShouldHaveRedText()
		{
			RunningApp.WaitForElement(q => q.Marked("I should have red text"));
			RunningApp.Screenshot("Label should have red text");
		}
#endif
	}
}
