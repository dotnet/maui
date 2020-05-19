using System;

using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 29128, "Slider background lays out wrong Android")]
	public class Bugzilla29128 : TestContentPage
	{
		protected override void Init ()
		{
			Content = new Slider {
				AutomationId = "SliderId",
				BackgroundColor = Color.Blue,
				Maximum = 255,
				Minimum = 0,
			};
		}

#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		public void Bugzilla29128Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("SliderId"));
			RunningApp.Screenshot("Slider and button should be centered");
			Assert.Inconclusive ("For visual review only");
		}
#endif
	}
}
