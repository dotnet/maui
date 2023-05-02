using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Maui.Controls.Sample.Sandbox.AppiumTests
{
	public class MainPageTests : AppiumPlatformsTestBase
	{
		public MainPageTests(TestDevice testDevice) : base(testDevice)
		{

		}

		[Test()]
		public void TestLogin()
		{
			//just to make the test pass, we need a way t wait for the app to launch
			App?.WaitForElement("btnLogin");

			Driver?.FindElement(ByAutomationId("entryUsername")).SendKeys("user@email.com");
			Driver?.FindElement(ByAutomationId("entryPassword")).SendKeys("Password");
			Thread.Sleep(2000);

			Driver?.FindElement(ByAutomationId("btnLogin")).Click();
			var text = Driver?.FindElement(ByAutomationId("lblStatus")).Text ?? "";

			Assert.IsNotNull(text);
			Assert.IsTrue(text.StartsWith("Logging in", StringComparison.CurrentCulture));
		}

		[Test()]
		public void TestLoginUITest()
		{
			App?.WaitForElement("btnLogin");
			App?.EnterText("entryUsername", "user@email.com");
			App?.EnterText("entryPassword", "Password");
			Thread.Sleep(2000);

			App?.Tap("btnLogin");
			var lblStatus = App?.WaitForElement("lblStatus").FirstOrDefault();
			var text = lblStatus?.Text;

			Assert.IsNotNull(text);
			Assert.IsTrue(text?.StartsWith("Logging in", StringComparison.CurrentCulture));
		}


		//[Test()]
		//public void TestAddItem()
		//{
		//	if (appiumDriver == null)
		//		throw new InvalidOperationException("no appium driver");

		//	appiumDriver.FindElement(By.Id("Browse")).Click();
		//	appiumDriver.FindElement(By.Id("AddToolbarItem")).Click();
		//	var itemNameField = appiumDriver.FindElement(By.Id("ItemNameEntry"));
		//	itemNameField.SendKeys("todo");

		//	var itemDesriptionField = appiumDriver.FindElement(By.Id("ItemDescriptionEntry"));
		//	itemDesriptionField.SendKeys("todo description");

		//	appiumDriver.FindElement(By.Id("SaveToolbarItem")).Click();
		//}

		//[Test()]
		//public void TestAbout()
		//{
		//	if (appiumDriver == null)
		//		throw new InvalidOperationException("no appium driver");

		//	appiumDriver.FindElement(By.Id("About")).Click(); // works for iOS
		//}
	}
}