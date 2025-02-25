using System.Threading.Tasks;
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
		public void Issue852TestsEntriesClickable()
		{
			App.WaitForElement("WelcomeLabel");
			App.WaitForElement(UsernameId);
			App.WaitForElement(PasswordId);
			App.WaitForElement("Login");

			App.Tap(UsernameId);
			Assert.That(App.WaitForElement("WelcomeLabel").GetText(), Is.EqualTo("Clicked User"));
			App.EnterText(UsernameId, "Usertest");
			App.WaitForElement(PasswordId);
			App.Tap(PasswordId);
			Assert.That(App.WaitForElement("WelcomeLabel").GetText(), Is.EqualTo("Clicked Password"));
			App.EnterText(PasswordId, "Userpass");
		}
	}
}
