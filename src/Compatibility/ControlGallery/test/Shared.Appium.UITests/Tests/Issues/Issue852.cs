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
		public void Issue852TestsEntriesClickable()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("WelcomeLabel");
			App.WaitForElement(UsernameId);
			App.WaitForElement(PasswordId);
			App.WaitForElement("Login");
			App.Screenshot("All elements present");

			App.Click(UsernameId);
			App.WaitForElement("WelcomeLabel");
			App.EnterText(UsernameId, "Usertest");
			App.Screenshot("User entered");

			App.Click(PasswordId);
			App.WaitForElement("WelcomeLabel");
			App.EnterText(PasswordId, "Userpass");
		}
	}
}
