using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	internal static class KeyboardScrolling
	{
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

			app.Click("GoButton");

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
			app.Click(marked);
			didReachEndofPage = CheckIfViewAboveKeyboard(app, marked, isEditor);
			if (didReachEndofPage)
				HideKeyboard(app, (app as AppiumApp)?.Driver, isEditor);
		}

		// will return a bool showing if the view is visible
		static bool CheckIfViewAboveKeyboard(IApp app, string marked, bool isEditor)
		{
			var views = app.WaitForElement(marked);

			// if this view is not on the screen, the keyboard will not be
			// showing and we can skip this view
			if (!app.IsKeyboardShown())
				return false;

			Assert.NotNull(views);
			var rect = views.GetRect();

			var testApp = app as AppiumApp;
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
			Assert.Less(rect.CenterY(), keyboardPosition.Y);

			return true;
		}

		internal static void HideKeyboard(IApp app, AppiumDriver? driver, bool isEditor)
		{
			if (isEditor)
				CloseiOSEditorKeyboard(driver);
			else
				app.DismissKeyboard();
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

		internal static void EntryNextEditorScrollingTest(IApp app, string galleryName)
		{
			app.WaitForElement("TargetView");
			app.EnterText("TargetView", "KeyboardScrollingEntryNextEditorPage");
			app.Click("GoButton");

			app.WaitForElement("Entry1");
			app.Click("Entry1");
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
				app.Click(entry);
				CheckIfViewAboveKeyboard(app, entry, false);
				HideKeyboard(app, (app as AppiumApp)?.Driver, false);
			}
		}
	}
}
