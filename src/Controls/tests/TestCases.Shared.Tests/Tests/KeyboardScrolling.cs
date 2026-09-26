using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	internal static class KeyboardScrolling
	{
		const string DoneButtonId = "Done";
		internal static readonly string IgnoreMessage = "These tests take a while and we are more interested in iOS Scrolling Behavior since it is not out-of-the-box.";

		internal static void EntriesScrollingTest(IApp app, string galleryName)
		{
			RunScrollingTest(app, galleryName, false);
		}

		internal static void EditorsScrollingTest(IApp app, string galleryName)
		{
			RunScrollingTest(app, galleryName, true);
		}

		static void RunScrollingTest(IApp app, string galleryName, bool isEditor)
		{
			var App = (app as AppiumApp);
			if (App is null)
				return;

			app.WaitForElement("TargetView");
			if (isEditor)
				app.EnterText("TargetView", "KeyboardScrollingEditorsPage");
			else
				app.EnterText("TargetView", "KeyboardScrollingEntriesPage");

			App.Tap("GoButton");

			// Entries 6 - 14 hit a group of interesting areas on scrolling
			// depending on the type of iOS device.
			for (int i = 6; i <= 14; i++)
			{
				var didReachEndofPage = false;
				if (isEditor)
					ClickText(app, $"Editor{i}", isEditor, out didReachEndofPage);
				else
					ClickText(app, $"Entry{i}", isEditor, out didReachEndofPage);

				// Scroll to the top of the page			
				OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
				var scrollSequence = new ActionSequence(touchDevice, 0);
				scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 300, TimeSpan.Zero));
				scrollSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				scrollSequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(500)));
				scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 650, TimeSpan.FromMilliseconds(250)));
				scrollSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
				App.Driver.PerformActions([scrollSequence]);

				if (!didReachEndofPage)
					break;
			}
		}

		static void ClickText(IApp app, string marked, bool isEditor, out bool didReachEndofPage)
		{
			app.Tap(marked);
			didReachEndofPage = CheckIfViewAboveKeyboard(app, marked, isEditor);
			if (didReachEndofPage)
				HideKeyboard(app, (app as AppiumApp)?.Driver, isEditor);
		}

		// will return a bool showing if the view is visible
		internal static bool CheckIfViewAboveKeyboard(IApp app, string marked, bool isEditor)
		{
			app.WaitForElement(marked);

			// if this view is not on the screen, the keyboard will not be
			// showing and we can skip this view
			if (!app.IsKeyboardShown())
				return false;

			app.RetryAssert(() =>
			{
				var rect = app.WaitForElementAndGetRect(marked);
				var keyboardPositionNullable = FindiOSKeyboardLocation((app as AppiumApp)?.Driver);
				Assert.That(keyboardPositionNullable, Is.Not.Null);
				var keyboardPosition = keyboardPositionNullable!.Value;
				if (isEditor)
				{
					// The editor's accessory view sits above the keyboard.
					keyboardPosition.Y -= 44;
				}

				Assert.That(rect.CenterY(), Is.LessThan(keyboardPosition.Y));
			});

			return true;
		}

		internal static void HideKeyboard(IApp app, AppiumDriver? driver, bool isEditor)
		{
			if (isEditor)
				CloseiOSEditorKeyboard(driver);
			else
				app.DismissKeyboard();

			Assert.That(app.WaitForKeyboardToHide(), Is.True, "Keyboard dismissal must finish before the next scroll");
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
			var keyboardDoneButton = driver?.FindElement(MobileBy.Name(DoneButtonId));
			keyboardDoneButton?.Click();
		}

		internal static void EntryNextEditorScrollingTest(IApp app, string galleryName)
		{
			app.WaitForElement("TargetView");
			app.EnterText("TargetView", "KeyboardScrollingEntryNextEditorPage");
			app.Tap("GoButton");

			app.WaitForElement("Entry1");
			app.Tap("Entry1");
			CheckIfViewAboveKeyboard(app, "Entry1", false);
			NextiOSKeyboardPress((app as AppiumApp)?.Driver);

			CheckIfViewAboveKeyboard(app, "Entry2", false);
			NextiOSKeyboardPress((app as AppiumApp)?.Driver);

			CheckIfViewAboveKeyboard(app, "Entry3", false);
			NextiOSKeyboardPress((app as AppiumApp)?.Driver);

			CheckIfViewAboveKeyboard(app, "Editor", true);
		}

		// Unintentionally types a 'V' but also presses the next keyboard key
		internal static void NextiOSKeyboardPress(AppiumDriver? driver)
		{
			var keyboard = driver?.FindElement(MobileBy.ClassName("UIAKeyboard"));
			keyboard?.SendKeys("\n");
		}

		internal static void GridStarRowScrollingTest(IApp app)
		{
			for (int i = 1; i <= 7; i++)
			{
				var entry = $"Entry{i}";
				app.WaitForElement(entry);
				app.Tap(entry);
				CheckIfViewAboveKeyboard(app, entry, false);
				HideKeyboard(app, (app as AppiumApp)?.Driver, false);
			}
		}
	}
}
