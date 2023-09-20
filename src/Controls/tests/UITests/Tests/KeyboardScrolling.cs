using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using TestUtils.Appium.UITests;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.AppiumTests
{
	internal static class KeyboardScrolling
	{
		internal static void EntriesScrollingTest(IApp app, IUITestContext? testContext, string galleryName)
		{
			testContext.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
				"These tests take a while and we are more interested in iOS Scrolling Behavior since it is not out-of-the-box.");

			// Entries 6 - 13 hit a group of interesting areas on scrolling
			// depending on the type of iOS device.
			for (int i = 6; i <= 15; i++)
			{
				// Until we can scroll back to the top between scrolls,
				// call the different entry tests separately while reseting each time
				ViewScrollingTest(app, $"Entry{i}", "KeyboardScrollingEntriesPage", false);
				app.NavigateToGallery(galleryName);
			}
		}

		internal static void EditorsScrollingTest(IApp app, IUITestContext? testContext, string galleryName)
		{
			testContext.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
				"These tests take a while and we are more interested in iOS Scrolling Behavior since it is not out-of-the-box.");

			// Editors 6 - 14 hit a group of interesting areas on scrolling
			// depending on the type of iOS device.
			for (int i = 6; i <= 15; i++)
			{
				ViewScrollingTest(app, $"Editor{i}", "KeyboardScrollingEditorsPage", true);
				app.NavigateToGallery(galleryName);
			}
		}

		static void ViewScrollingTest(IApp app, string marked, string pageName, bool isEditor)
		{
			app.WaitForElement("TargetView");
			app.EnterText("TargetView", pageName);
			app.Tap("GoButton");
			CheckIfViewAboveKeyboard(app, marked, isEditor);
			app.NavigateBack();
		}

		static void CheckIfViewAboveKeyboard(IApp app, string marked, bool isEditor)
		{
			app.Tap(marked);
			var views = app.WaitForElement(marked);

			// if this view is not on the screen, the keyboard will not be
			// showing and we can skip this view
			if (!app.IsKeyboardShown())
				return;

			Assert.IsNotEmpty(views);
			var rect = views[0].Rect;

			var testApp = app as AppiumUITestApp;
			var keyboardPositionNullable = FindiOSKeyboardLocation(testApp?.Driver);
			Assert.NotNull(keyboardPositionNullable);

			var keyboardPosition = (System.Drawing.Point)keyboardPositionNullable!;
			if (isEditor)
			{
				// Until we can get the default Maui accessory view added on the editors,
				// let's just use the default height added for the accessory view
				var defaultSizeAccessoryView = 44;
				keyboardPosition.Y -= defaultSizeAccessoryView;

			}
			Assert.Less(rect.CenterY, keyboardPosition.Y);

			if (isEditor)
				TryCloseiOSEditorKeyboard(testApp?.Driver);
			else
				app.DismissKeyboard();
		}

		//internal static void EditorsScrollingTest(IApp app, IUITestContext? testContext, string galleryName)
		//{
		//	testContext.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
		//		"These tests take a while and we are more interested in iOS Scrolling Behavior since it is not out-of-the-box.");

		//	// Editors 6 - 13 hit a group of interesting areas on scrolling
		//	// depending on the type of iOS device.
		//	//for (int i = 6; i <= 13; i++)
		//	for (int i = 15; i <= 16; i++)
		//	{
		//		// Until we can scroll back to the top between scrolls,
		//		// call the different editor tests separately while reseting each time
		//		ViewScrollingTest(app, $"Editor{i}", "KeyboardScrollingEditorsPage", true);
		//		app.NavigateToGallery(galleryName);
		//	}
		//}

		//internal static void ViewScrollingTest(IApp app, string marked, string pageName, bool isEditor)
		//{
		//	app.WaitForElement("TargetView");
		//	app.EnterText("TargetView", pageName);
		//	app.Tap("GoButton");
		//	CheckIfViewAboveKeyboard(app, marked, isEditor);

		//	app.NavigateBack();
		//	//app.WaitForElement("DismissButton");
		//	//app.Tap("DismissButton");
		//}

		//internal static void CheckIfViewAboveKeyboard(IApp app, string marked, bool isEditor)
		//{
		//	var views = app.WaitForElement(marked);
		//	app.Tap(marked);
		//	// give time for scrolling
		//	views = app.WaitForElement(marked);
		//	Assert.IsNotEmpty(views);
		//	var rect = views[0].Rect;

		//	var testApp = app as AppiumUITestApp;
		//	var keyboardPositionNullable = FindiOSKeyboardLocation(testApp?.Driver);
		//	Assert.NotNull(keyboardPositionNullable);

		//	var keyboardPosition = (System.Drawing.Point)keyboardPositionNullable!;
		//	if (isEditor)
		//	{
		//		// Until we can get the default Maui accessory view added on the editors,
		//		// let's just use the default height added for the accessory view
		//		var defaultSizeAccessoryView = 44;
		//		keyboardPosition.Y -= defaultSizeAccessoryView;

		//	}
		//	Assert.Less(rect.CenterY, keyboardPosition.Y);

		//	if (isEditor)
		//		TryCloseiOSEditorKeyboard(testApp?.Driver);
		//	else
		//		app.DismissKeyboard();
		//	//app.ScrollUpTo("DismissButton");
		//}

		internal static System.Drawing.Point? FindiOSKeyboardLocation(AppiumDriver? driver)
		{
			//public bool IsiOS => Platform.Equals("iOS", StringComparison.OrdinalIgnoreCase);
			//public bool IsMac => Platform.Equals("mac", StringComparison.OrdinalIgnoreCase);
			//var platform = driver?.Capabilities.GetCapability(MobileCapabilityType.PlatformName).ToString() ?? "";
			//var isiOS = platform.Equals("iOS", StringComparison.OrdinalIgnoreCase);

			if (driver?.IsKeyboardShown() == true)// && isiOS)
			{
				try
				{
					var keyboard = driver.FindElement(MobileBy.ClassName("UIAKeyboard"));
					return keyboard.Location;
				}
				catch (InvalidElementStateException)
				{
					// Appium iOS driver does not have a consistent way to dismiss the keyboard
				}
			}
			return null;
		}

		internal static void TryCloseiOSEditorKeyboard(AppiumDriver? driver)
		{
			var keyboardDoneButton = driver?.FindElement(MobileBy.Name("Done"));
			keyboardDoneButton?.Click();
			////public bool IsiOS => Platform.Equals("iOS", StringComparison.OrdinalIgnoreCase);
			////public bool IsMac => Platform.Equals("mac", StringComparison.OrdinalIgnoreCase);
			//var platform = driver?.Capabilities.GetCapability(MobileCapabilityType.PlatformName).ToString() ?? "";
			//var isiOS = platform.Equals("iOS", StringComparison.OrdinalIgnoreCase);

			//if (driver?.IsKeyboardShown() == true && isiOS)
			//{
			//	try
			//	{
			//		var keyboardDoneButton = driver.FindElement(MobileBy.Name("Done"));
			//		keyboardDoneButton.Click();
			//		//var items = keyboard.FindElements(By.Name("Done"));
			//		//items[0].Click();
			//	}
			//	catch (InvalidElementStateException)
			//	{
			//		// Appium iOS driver does not have a consistent way to dismiss the keyboard
			//	}
			//}
		}
	}
}

