using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26049 : _IssuesUITest
	{
		public Issue26049(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[iOS] Fix ShellContent Title Does Not Update at Runtime";

		[Test, Order(1)]
		[Category(UITestCategories.Shell)]
		public void VerifyFirstShellContentTitle()
		{
			App.WaitForElement("ChangeShellContentTitle");
			App.Click("ChangeShellContentTitle");
			TabNavigationViewItemIfWindows();
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[Category(UITestCategories.Shell)]
		public void VerifyNewlyAddedShellContentTitle()
		{
			TabNavigationViewItemIfWindows();
			App.WaitForElement("AddShellContent");
			App.Click("AddShellContent");
			App.Click("UpdateNewShellContentTitle");
			TabNavigationViewItemIfWindows();
			VerifyScreenshot();
		}

		[Test, Order(3)]
		[Category(UITestCategories.Shell)]
		public void VerifyExistingTabTitle()
		{
			TabNavigationViewItemIfWindows();
			App.WaitForElement("RemoveShellContent");
			App.Click("RemoveShellContent");
			App.Click("UpdateThirdTabTitle");
			TabNavigationViewItemIfWindows();
			VerifyScreenshot();
		}

		void TabNavigationViewItemIfWindows()
		{
#if WINDOWS
			App.Tap("navViewItem");
#endif
		}
	}
}