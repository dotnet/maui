using Microsoft.Maui.Appium;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Interactions;

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
			Driver?.FindElement(ByAutomationId("entryPassword")).SendKeys("password");
			Driver?.FindElement(ByAutomationId("btnLogin"));

			var btnElement = Driver?.FindElement(ByAutomationId("btnLogin"))!;
			if (Driver!.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("mac"))
			{
				Actions action = new Actions(Driver);
				action.Click(btnElement).Perform();
			}
			else
			{
				btnElement.Click();
			}

			var text = Driver?.FindElement(ByAutomationId("lblStatus")).Text ?? "";

			Assert.IsNotNull(text);
			Assert.IsTrue(text.StartsWith("Logging in", StringComparison.CurrentCulture));
		}

		[Test()]
		public void TestLoginUITest()
		{
			App?.WaitForElement("btnLogin");
			App?.EnterText("entryUsername", "user@email.com");
			App?.EnterText("entryPassword", "password");
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