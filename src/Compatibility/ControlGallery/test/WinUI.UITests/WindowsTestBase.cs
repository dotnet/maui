using System;
using System.Diagnostics;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using Xamarin.UITest;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	public class WindowsTestBase
	{
		protected const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
		protected static WindowsDriver<WindowsElement> Session;

		public static IApp ConfigureApp()
		{
			if (Session == null)
				Session = CreateWindowsDriver();
			else
				Reset();

			return new WinDriverApp(Session);
		}


		public static void StartupApplication()
		{
			AppiumOptions options = new AppiumOptions();
			options.AddAdditionalCapability("app", "0d4424f6-1e29-4476-ac00-ba22c3789cb6_ph1m9x8skttmg!App");

			try
			{
				Session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), options);
			}
			catch
			{
				// This crashes because it can't find the window but it will at least start the application
			}
		}

		public static WindowsElement GetWindowsElement()
		{
			AppiumOptions options = new AppiumOptions();
			options.AddAdditionalCapability("app", "Root");
			Session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), options);

			new Actions(Session)
					.SendKeys(Keys.Meta + "s" + Keys.Meta)
					.Perform();

			try
			{
				return Session.FindElementByName("WinUI Desktop");
			}
			catch { }

			StartupApplication();
			return GetWindowsElement();
		}

		public static WindowsDriver<WindowsElement> CreateWindowsDriver()
		{
			var topLevelWindowHandle = GetWindowsElement().GetAttribute("NativeWindowHandle");
			topLevelWindowHandle = (int.Parse(topLevelWindowHandle)).ToString("x"); // Convert to Hex

			AppiumOptions options = new AppiumOptions();
			options.AddAdditionalCapability("appTopLevelWindow", topLevelWindowHandle);
			options.AddAdditionalCapability("appArguments", "RunningAsUITests");

			Session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), options);

			Assert.IsNotNull(Session);
			Session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
			Reset();

			return Session;
		}

		internal static void HandleAppClosed(Exception ex)
		{
			if (ex.IsWindowClosedException())
			{
				Session = null;
			}
		}

		public static void Reset()
		{
			try
			{
				new Actions(Session)
					.SendKeys(Keys.Escape)
					.Perform();
			}
			catch (Exception ex)
			{
				HandleAppClosed(ex);
				Debug.WriteLine($">>>>> WindowsTestBase ConfigureApp 49: {ex}");
				throw;
			}
		}
	}
}
