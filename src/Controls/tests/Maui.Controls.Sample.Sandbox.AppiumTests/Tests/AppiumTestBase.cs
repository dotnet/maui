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
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Appium.Interfaces;
using Xamarin.UITest;
using Maui.Controls.Sample.Sandbox.AppiumTests.Tests;
using Microsoft.AspNetCore.Components;

namespace Maui.Controls.Sample.Sandbox.AppiumTests
{
	[TestFixture(TestDevice.Android)]
	[TestFixture(TestDevice.iOS)]
	[TestFixture(TestDevice.Mac)]
	[TestFixture(TestDevice.Windows)]
	public class AppiumPlatformsTestBase : AppiumTestBase
	{
		public AppiumPlatformsTestBase(TestDevice device)
		{
			_testDevice = device;
		}

		[TearDown]
		public void TearDown()
		{
			//this crashes on Android
			Driver?.ResetApp();
		}
	}

	[SetUpFixture]
	public class AppiumTestBase
	{
		protected AppiumDriver? Driver;
		readonly AppiumOptions appiumOptions;
		readonly Uri _driverUri;
		internal TestDevice _testDevice;
		string? _appId;
		IApp? _app;

		public IApp? App => _app;

		public bool IsAndroid => Driver == null ? false : Driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("Android");

		public AppiumTestBase()
		{
			_driverUri = new Uri("http://localhost:4723/wd/hub");

			appiumOptions = new AppiumOptions();
			appiumOptions.AddAdditionalAppiumOption("reportDirectory", "reports");
			appiumOptions.AddAdditionalAppiumOption("reportFormat", "xml");
		//	appiumOptions.AddAdditionalAppiumOption("testName", testName);
			appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.FullReset, "false");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			InitAppiumOptions(appiumOptions);

			string appId = "com.microsoft.maui.sandbox";
			Driver = GetDriver(appId);
			_app = new TestApp(appId, Driver);

			//Mac is throwing if we call ActivateApp
			if (_testDevice != TestDevice.Mac)
				Driver?.ActivateApp(appId);
		}

		[OneTimeTearDown()]
		public void OneTimeTearDown()
		{
			Driver?.Quit();
		}

		protected virtual AppiumDriver? GetDriver(string appId)
		{
			_appId = appId;

			switch (_testDevice)
			{
				case TestDevice.Android:
					return new AndroidDriver(_driverUri, appiumOptions);
				case TestDevice.iOS:
					return new IOSDriver(_driverUri, appiumOptions);
				case TestDevice.Mac:
					return new MacDriver(_driverUri, appiumOptions);
				case TestDevice.Windows:
					return new WindowsDriver(_driverUri, appiumOptions);
				default:
					return null;
			}
		}

		protected virtual void InitAppiumOptions(AppiumOptions appiumOptions)
		{
			switch (_testDevice)
			{
				case TestDevice.Android:
					appiumOptions.PlatformName = "Android";
					appiumOptions.AutomationName = "UiAutomator2";
					break;
				case TestDevice.iOS:
					appiumOptions.PlatformName = "iOS";
					appiumOptions.AutomationName = "XCUITest";
					appiumOptions.DeviceName = "iPad (10th generation)";
					appiumOptions.PlatformVersion = "16.2";
					appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.Udid, "E198E6C0-0337-4990-8494-7B078BAD3070");
					appiumOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, "com.microsoft.maui.sandbox");
					break;
				case TestDevice.Mac:
					appiumOptions.PlatformName = "mac";
					appiumOptions.AutomationName = "mac2";
					appiumOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, "com.microsoft.maui.sandbox");
					break;
				case TestDevice.Windows:
					break;
			}
		}

		internal string GetElementId(string elementId)
		{
			if (IsAndroid)
				return $"{_appId}:id/{elementId}";

			return elementId;
		}
	}
}
