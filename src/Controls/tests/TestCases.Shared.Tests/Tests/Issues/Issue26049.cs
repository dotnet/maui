#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS   //More information - https://github.com/dotnet/maui/issues/27494
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
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[Category(UITestCategories.Shell)]
		public void VerifyNewlyAddedShellContentTitle()
		{
			App.WaitForElement("AddShellContent");
			App.Click("AddShellContent");
			App.Click("UpdateNewShellContentTitle");
			VerifyScreenshot();
		}

		[Test, Order(3)]
		[Category(UITestCategories.Shell)]
		public void VerifyExistingTabTitle()
		{
			App.WaitForElement("RemoveShellContent");
			App.Click("RemoveShellContent");
			App.Click("UpdateThirdTabTitle");
			VerifyScreenshot();
		}
	}
}
#endif