using NUnit.Framework;
using OpenQA.Selenium.Appium;

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
			Driver?.FindElement(MobileBy.Id(GetElementId("entryUsername"))).SendKeys("user@email.com");
			Driver?.FindElement(MobileBy.Id(GetElementId("entryPassword"))).SendKeys("password");
			Driver?.FindElement(MobileBy.Id(GetElementId("btnLogin"))).Click();
			var text = Driver?.FindElement(MobileBy.Id(GetElementId("lblStatus"))).Text ?? "";

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