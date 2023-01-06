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
		protected AppiumDriver? Driver;
		protected readonly AppiumOptions appiumOptions;
		protected readonly Uri _driverUri;

		bool _isAndroid = true;
		bool _isIos = false;
		string? _appId;
		IApp? _app;

		public IApp? App => _app;

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
			Driver = GetDriver(appId);
			_app = new TestApp(appId, Driver);
			Driver?.ActivateApp(appId);
		}

		[OneTimeTearDown()]
		public void TearDown()
		{
			Driver?.Quit();
		}

		protected virtual AppiumDriver? GetDriver(string appId)
		{
			_appId = appId;

			if (_isAndroid)
			{
				return GetDriverAndroid(appId);
			}
			if (_isIos)
			{
				return GetDriverIos(appId);
			}

			return  null;
		}

		protected virtual AppiumDriver GetDriverAndroid(string appId)
		{
			return new AndroidDriver(_driverUri, appiumOptions);
		}

		protected virtual AppiumDriver GetDriverIos(string appId)
		{
			return new IOSDriver(_driverUri, appiumOptions);
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

		public string GetElementId(string elementId)
		{
			if (IsAndroid)
				return $"{_appId}:id/{elementId}";
		
			return elementId;
		}

		public bool IsAndroid => Driver == null ? false : Driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("Android");
	}
}
