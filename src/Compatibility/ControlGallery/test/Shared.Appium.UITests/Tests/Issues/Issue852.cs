using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue852 : IssuesUITest
	{
		const string UsernameId = "username852";
		const string PasswordId = "password852";

		public Issue852(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Async loading of Content causes UI element to be unclickable"; 

		[Test]
		[Category(UITestCategories.Entry)]
		public void Issue852TestsEntriesClickable()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("WelcomeLabel");
			RunningApp.WaitForElement(UsernameId);
			RunningApp.WaitForElement(PasswordId);
			RunningApp.WaitForElement("Login");
			RunningApp.Screenshot("All elements present");

			RunningApp.Tap(UsernameId);
			RunningApp.WaitForElement("WelcomeLabel");
			RunningApp.EnterText(UsernameId, "Usertest");
			RunningApp.Screenshot("User entered");

			RunningApp.Tap(PasswordId);
			RunningApp.WaitForElement("WelcomeLabel");
			RunningApp.EnterText(PasswordId, "Userpass");
		}
	}
}
