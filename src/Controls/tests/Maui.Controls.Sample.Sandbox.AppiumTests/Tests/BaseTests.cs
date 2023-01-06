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
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Interfaces;
using Xamarin.UITest;
using Maui.Controls.Sample.Sandbox.AppiumTests.Tests;
using Microsoft.AspNetCore.Components;

namespace Maui.Controls.Sample.Sandbox.Tests
{
	[SetUpFixture]
	public class BaseTest
	{
		protected AppiumDriver? appiumAndroidDriver;
		protected AppiumDriver? appiumiOSDriver;


		protected AppiumDriver? Driver;

		protected readonly AppiumOptions appiumOptions;
		protected readonly Uri _driverUri;

		bool _isAndroid = true;
		bool _isIos = false;

		//Xamarin.UITest.IApp? _app;

		//public IApp? App => _app;

		public BaseTest(string testName)
		{
			_driverUri = new Uri("http://localhost:4723/wd/hub");

			appiumOptions = new AppiumOptions();
			appiumOptions.AddAdditionalAppiumOption("reportDirectory", "reports");
			appiumOptions.AddAdditionalAppiumOption("reportFormat", "xml");
			appiumOptions.AddAdditionalAppiumOption("testName", testName);
			appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.FullReset, "false");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			InitAppiumOptions(appiumOptions);

			string appId = "com.microsoft.maui.sandbox";
			if (_isAndroid)
			{
				Driver = GetDriverAndroid(appId);
			}
			if (_isIos)
			{
				Driver = GetDriverIos(appId);
			}
		}

		[OneTimeTearDown()]
		public void TearDown()
		{
			Driver?.Quit();
		}

		protected virtual AppiumDriver GetDriverAndroid(string appId)
		{
			var driver = new AndroidDriver(_driverUri, appiumOptions);
			driver.ActivateApp(appId);
			return driver;
		}

		protected virtual AppiumDriver GetDriverIos(string appId)
		{
			var driver = new IOSDriver(_driverUri, appiumOptions);
			driver.ActivateApp(appId);
			return driver;
		}

		protected virtual void InitAppiumOptions(AppiumOptions appiumOptions)
		{
			if (_isAndroid)
			{
				appiumOptions.PlatformName = "Android";
				appiumOptions.AutomationName = "UiAutomator2";
				//appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.DeviceName, "pixel_5_-_api_30");
				//appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.Udid, "emulator-5554");
				//appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.PlatformName, "Android");
				//appiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, "com.microsoft.maui.sandbox");
				//appiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, "crc64fa090d87c1ce7f0b.MainActivity");
			}
			if (_isIos)
			{
				appiumOptions.PlatformName = "iOS";
				appiumOptions.AutomationName = "XCUITest";
				appiumOptions.DeviceName = "iPad (10th generation)";
				appiumOptions.PlatformVersion = "16.2";		
				appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.Udid, "E198E6C0-0337-4990-8494-7B078BAD3070");
				appiumOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, "com.microsoft.maui.sandbox");
			}
		}

		public string GetElementText(string elementId)
		{
			if (appiumAndroidDriver == null)
				throw new InvalidOperationException("no appium driver");

			var element = appiumAndroidDriver.FindElement(By.Id(elementId));
			var attributName = IsAndroid ? "text" : "value";
			return element.GetAttribute(attributName);
		}

		public bool IsAndroid => Driver == null ? false : Driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("Android");
	}
}
