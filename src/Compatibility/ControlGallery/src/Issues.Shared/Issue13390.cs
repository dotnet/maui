using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13390, "Custom SlideFlyoutTransition is not working",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue13390 : TestShell
	{
		protected override void Init()
		{
			CreateContentPage()
				.Content = new Label()
				{
					Text = "If app has not crashed test has passed",
					AutomationId = "Success"
				};
		}

#if UITEST && __IOS__
		[Test]
		public void CustomSlideFlyoutTransitionCausesCrash()
		{
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
