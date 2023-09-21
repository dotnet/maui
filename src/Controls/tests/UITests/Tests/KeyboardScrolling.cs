using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using TestUtils.Appium.UITests;
using VisualTestUtils;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.AppiumTests
{
	internal static class KeyboardScrolling
	{
		internal static void EntriesScrollingTest(IApp app, IUITestContext? testContext, string galleryName)
		{
			BeginScrollingTest(app, testContext, galleryName, false);
		}

		internal static void EditorsScrollingTest(IApp app, IUITestContext? testContext, string galleryName)
		{
			BeginScrollingTest(app, testContext, galleryName, true);
		}

		static void BeginScrollingTest(IApp app, IUITestContext? testContext, string galleryName, bool isEditor)
		{
			testContext.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
				"These tests take a while and we are more interested in iOS Scrolling Behavior since it is not out-of-the-box.");

			// Entries 6 - 14 hit a group of interesting areas on scrolling
			// depending on the type of iOS device.
			for (int i = 6; i <= 15; i++)
			{
				var shouldContinue = false;
				if (isEditor)
					shouldContinue = NavigateTest(app, $"Editor{i}", "KeyboardScrollingEditorsPage", isEditor);
				else
					shouldContinue = NavigateTest(app, $"Entry{i}", "KeyboardScrollingEntriesPage", isEditor);

				app.NavigateToGallery(galleryName);

				if (!shouldContinue)
					break;
			}
		}

		static bool NavigateTest(IApp app, string marked, string pageName, bool isEditor)
		{
			app.WaitForElement("TargetView");
			app.EnterText("TargetView", pageName);
			app.Tap("GoButton");
			var shouldContinue = CheckIfViewAboveKeyboard(app, marked, isEditor);
			app.NavigateBack();
			return shouldContinue;
		}

		static bool CheckIfViewAboveKeyboard(IApp app, string marked, bool isEditor)
		{
			app.Tap(marked);
			var views = app.WaitForElement(marked);

			// if this view is not on the screen, the keyboard will not be
			// showing and we can skip this view
			if (!app.IsKeyboardShown())
				return false;

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
				CloseiOSEditorKeyboard(testApp?.Driver);
			else
				app.DismissKeyboard();

			return true;
		}

		internal static System.Drawing.Point? FindiOSKeyboardLocation(AppiumDriver? driver)
		{
			if (driver?.IsKeyboardShown() == true)
			{
				var keyboard = driver.FindElement(MobileBy.ClassName("UIAKeyboard"));
				return keyboard.Location;
			}
			return null;
		}

		internal static void CloseiOSEditorKeyboard(AppiumDriver? driver)
		{
			var keyboardDoneButton = driver?.FindElement(MobileBy.Name("Done"));
			keyboardDoneButton?.Click();
		}
	}
}

