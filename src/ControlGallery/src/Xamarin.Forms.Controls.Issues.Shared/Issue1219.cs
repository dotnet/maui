using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
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
		public void ViewCellInTableViewDoesNotCrash()
		{
			// If we can see this element, then we didn't crash.
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}