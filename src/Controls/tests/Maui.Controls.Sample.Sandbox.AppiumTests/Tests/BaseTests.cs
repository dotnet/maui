using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Interfaces;
using Xamarin.UITest;
using Maui.Controls.Sample.Sandbox.AppiumTests.Tests;
using Microsoft.AspNetCore.Components;

namespace Maui.Controls.Sample.Sandbox.Tests
{

	[SetUpFixture]
	public class BaseTest
	{

		private readonly string reportDirectory = "reports";
		private readonly string reportFormat = "xml";

		protected AppiumDriver<AndroidElement>? appiumDriver;
		protected readonly AppiumOptions appiumOptions;
		protected readonly Uri driverUri;

		Xamarin.UITest.IApp? _app;

		public IApp? App => _app;

		public BaseTest(string testName)
		{
			driverUri = new Uri("http://localhost:4723/wd/hub");

			appiumOptions = new AppiumOptions();
			appiumOptions.AddAdditionalCapability("reportDirectory", reportDirectory);
			appiumOptions.AddAdditionalCapability("reportFormat", reportFormat);
			appiumOptions.AddAdditionalCapability("testName", testName);
			appiumOptions.AddAdditionalCapability(MobileCapabilityType.FullReset, "false");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			InitAppiumOptions(appiumOptions);

			string appId = "com.microsoft.maui.sandbox";
			appiumDriver = GetDriver(appId);
			if (appiumDriver == null)
				throw new InvalidOperationException("no appiumDriver");
			_app = new TestApp<AppiumDriver<AndroidElement>, AndroidElement>(appId, appiumDriver);
		}

		[OneTimeTearDown()]
		public void TearDown()
		{
			// Perform a driver quit so that the report is printed
			appiumDriver?.Quit();
		}

		protected virtual AppiumDriver<AndroidElement>? GetDriver(string appId)
		{
			var driver = new AndroidDriver<AndroidElement>(driverUri, appiumOptions);
			driver.ActivateApp(appId);
			return driver;
		}

		protected virtual void InitAppiumOptions(AppiumOptions appiumOptions)
		{
			appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformName, "Android");
			//appiumOptions.AddAdditionalCapability(MobileCapabilityType.DeviceName, "pixel_5_-_api_30");
			//appiumOptions.AddAdditionalCapability(MobileCapabilityType.Udid, "emulator-5554");
			//appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformName, "Android");
			//appiumOptions.AddAdditionalCapability(AndroidMobileCapabilityType.AppPackage, "com.microsoft.maui.sandbox");
			//appiumOptions.AddAdditionalCapability(AndroidMobileCapabilityType.AppActivity, "crc64fa090d87c1ce7f0b.MainActivity");
			appiumOptions.AddAdditionalCapability(MobileCapabilityType.AutomationName, "UiAutomator2");
		}

		public string GetElementText(string elementId)
		{
			if (appiumDriver == null)
				throw new InvalidOperationException("no appium driver");

			var element = appiumDriver.FindElement(By.Id(elementId));
			var attributName = IsAndroid ? "text" : "value";
			return element.GetAttribute(attributName);
		}

		public bool IsAndroid => appiumDriver == null ? false : appiumDriver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("Android");
	}
}
