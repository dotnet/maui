using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue852 : _IssuesUITest
	{
		const string UsernameId = "username852";
		const string PasswordId = "password852";

		public Issue852(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Async loading of Content causes UI element to be unclickable";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void Issue852TestsEntriesClickable()
		{
			App.WaitForElement("WelcomeLabel");
			App.WaitForElement(UsernameId);
			App.WaitForElement(PasswordId);
			App.WaitForElement("Login");
			App.Screenshot("All elements present");

			App.Tap(UsernameId);
			App.WaitForElement("WelcomeLabel");
			App.EnterText(UsernameId, "Usertest");
			App.Screenshot("User entered");

			App.Tap(PasswordId);
			App.WaitForElement("WelcomeLabel");
			App.EnterText(PasswordId, "Userpass");
		}
	}
}
