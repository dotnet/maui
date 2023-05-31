using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1760, "Content set after an await is not visible", PlatformAffected.Android, issueTestNumber: 1)]
	public class Issue1760_1 : TestFlyoutPage
	{
		protected override void Init()
		{
			Flyout = new Issue1760._1760Master(false);
			Detail = new Issue1760._1760TestPage(false);
			IsPresented = true;
		}

#if UITEST && __ANDROID__
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		public void Issue1760_1Test()
		{
			RunningApp.WaitForElement(Issue1760.Before);
			RunningApp.WaitForElement(Issue1760.After);

			RunningApp.Tap("Test Page 1");
			RunningApp.WaitForElement(Issue1760.Before);
			RunningApp.WaitForElement(Issue1760.After);
		}
#endif
	}
}