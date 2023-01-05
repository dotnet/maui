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
			if (appiumDriver == null || App == null)
				throw new InvalidOperationException("no appium driver");

			App?.WaitForElement("btnLogin");
			App?.EnterText("entryUsername", "user@email.com");
			//appiumDriver.FindElement(By.Id("com.microsoft.maui.sandbox:id/btnLogin")).Click();
			//appiumDriver.FindElement(By.Id("com.microsoft.maui.sandbox:id/entryUsername")).SendKeys("user@email.com");
			//appiumDriver.FindElement(By.Id("com.microsoft.maui.sandbox:id/entryPassword")).SendKeys("password");

			//appiumDriver.FindElement(By.Id("com.microsoft.maui.sandbox:id/btnLogin")).Click();
			//var text = GetElementText("com.microsoft.maui.sandbox:id/lblStatus"); // Android is "text"

			//Assert.IsNotNull(text);
			//Assert.IsTrue(text.StartsWith("Logging in", StringComparison.CurrentCulture));
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