using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1219, "Setting ToolbarItems in ContentPage constructor crashes app", PlatformAffected.iOS)]
	public class Issue1219 : TestContentPage
	{
		const string Success = "Success";

		public Issue1219()
		{
			Content = new Label { Text = Success };

			ToolbarItems.Add(new ToolbarItem("MenuItem", "", () =>
			{

			}));
		}

		protected override void Init() { }
#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void ViewCellInTableViewDoesNotCrash()
		{
			// If we can see this element, then we didn't crash.
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}