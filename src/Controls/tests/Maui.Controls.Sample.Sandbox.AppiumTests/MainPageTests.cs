using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium;
using Maui.Controls.Sample.Sandbox.Tests;

namespace Maui.Controls.Sample.Sandbox.AppiumTests
{
	[TestFixture]
	public class MainPageTests : BaseTest
	{
		public MainPageTests() : base(nameof(MainPageTests))
		{

		}

		[Test()]
		public void TestLogin()
		{
			if (Driver == null)
				throw new InvalidOperationException("no appium driver");

			Driver.FindElement(By.Id(GetElementId("entryUsername"))).SendKeys("user@email.com");
			Driver.FindElement(By.Id(GetElementId("entryPassword"))).SendKeys("password");
			Driver.FindElement(By.Id(GetElementId("btnLogin"))).Click();
			var text = Driver.FindElement(By.Id(GetElementId("lblStatus"))).Text;

			Assert.IsNotNull(text);
			Assert.IsTrue(text.StartsWith("Logging in", StringComparison.CurrentCulture));
		}

		[Test()]
		public void TestLoginUITest()
		{
			if (Driver == null)
				throw new InvalidOperationException("no appium driver");
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