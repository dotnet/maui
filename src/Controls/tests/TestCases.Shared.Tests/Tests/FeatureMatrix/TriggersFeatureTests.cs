using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Triggers)]
	public class TriggersFeatureTests : _GalleryUITest
	{
		public const string TriggerFeatureMatrix = "Triggers Feature Matrix";
		public override string GalleryPageName => TriggerFeatureMatrix;

		public TriggersFeatureTests(TestDevice device)
			: base(device)
		{
		}

		private void SelectTriggerType(string triggerButtonAutomationId)
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement(triggerButtonAutomationId);
			App.Tap(triggerButtonAutomationId);
			App.WaitForElement("Apply");
			App.Tap("Apply");
		}

		[Test]
		[Order(1)]
		public void PropertyTriggerChangesBackgroundOnFocus()
		{
			Exception? exception = null;

			SelectTriggerType("PropertyTriggerButton");

			// Tap to focus
			App.WaitForElement("PropertyTriggerEntry");
			App.Tap("PropertyTriggerEntry");
			VerifyScreenshotOrSetException(ref exception, "PropertyTrigger_Focused");

			App.WaitForElement("PropertyTriggerDummyEntry");
			App.Tap("PropertyTriggerDummyEntry"); 
			VerifyScreenshotOrSetException(ref exception, "PropertyTrigger_UnFocused");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Order(2)]
		public void DataTriggerEnablesButtonWhenTextEntered()
		{
			Exception? exception = null;

			SelectTriggerType("DataTriggerButton");
			App.WaitForElement("DataTriggerEntry");

			// Initial state - button disabled (opacity 0.5)
			VerifyScreenshotOrSetException(ref exception, "DataTrigger_ButtonDisabled");

			// Enter text to enable button
			App.WaitForElement("DataTriggerEntry");
			App.Tap("DataTriggerEntry");
			App.EnterText("DataTriggerEntry", "Test");
			VerifyScreenshotOrSetException(ref exception, "DataTrigger_ButtonEnabled");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Order(3)]
		public void EventTriggerValidatesNumericInput()
		{
			Exception? exception = null;

			SelectTriggerType("EventTriggerButton");

			// Enter valid numeric text
			App.WaitForElement("EventTriggerEntry");
			App.Tap("EventTriggerEntry");
			App.EnterText("EventTriggerEntry", "123");
			VerifyScreenshotOrSetException(ref exception, "EventTrigger_ValidNumeric");

			// Enter invalid text (should trigger validation)
			App.ClearText("EventTriggerEntry");
			App.EnterText("EventTriggerEntry", "abc");
			VerifyScreenshotOrSetException(ref exception, "EventTrigger_InvalidText");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Order(4)]
		public void StateTriggerChangesBackgroundOnToggle()
		{
			Exception? exception = null;

			SelectTriggerType("StateTriggerButton");
			App.WaitForElement("StateTriggerSwitch");

			// Initial state - switch off, background white
			VerifyScreenshotOrSetException(ref exception, "StateTrigger_SwitchOff");

			// Toggle switch on - background black
			App.WaitForElement("StateTriggerSwitch");
			App.Tap("StateTriggerSwitch");
			VerifyScreenshotOrSetException(ref exception, "StateTrigger_SwitchOn");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Order(5)]
		public void CompareStateTriggerChangesBackgroundOnCheck()
		{
			Exception? exception = null;

			SelectTriggerType("CompareStateTriggerButton");
			App.WaitForElement("CompareStateCheckBox");

			// Initial state - unchecked, background LightGray
			VerifyScreenshotOrSetException(ref exception, "CompareStateTrigger_Unchecked");

			// Check the checkbox - background DarkGreen
			App.WaitForElement("CompareStateCheckBox");
			App.Tap("CompareStateCheckBox");
			VerifyScreenshotOrSetException(ref exception, "CompareStateTrigger_Checked");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Order(6)]
		public void DeviceStateTriggerShowsPlatformSpecificBackground()
		{
			SelectTriggerType("DeviceStateTriggerButton");
			App.WaitForElement("DeviceStateTriggerGrid");

			// Verify platform label is displayed correctly
			var platformText = App.FindElement("PlatformLabel").GetText();
#if ANDROID
			Assert.That(platformText, Is.EqualTo("Running on: Android"));
#elif IOS
			Assert.That(platformText, Is.EqualTo("Running on: iOS"));
#elif MACCATALYST
			Assert.That(platformText, Is.EqualTo("Running on: MacCatalyst"));
#else
			Assert.That(platformText, Does.Contain("Running on: WinUI"));
#endif
		}
#if ANDROID || IOS // These platforms support orientation changes which is required to test the OrientationStateTrigger
		[Test]
		[Order(7)]
		public void OrientationStateTriggerShowsCorrectBackground()
		{
			SelectTriggerType("OrientationStateTriggerButton");
			App.WaitForElement("OrientationStateTriggerGrid");

			// Verify orientation label is displayed
			var orientationPotraitText = App.FindElement("OrientationLabel").GetText();
			Assert.That(orientationPotraitText, Is.EqualTo("Current orientation: Portrait"));

			App.SetOrientationLandscape();

			var orientationLandScapeText = App.FindElement("OrientationLabel").GetText();
			Assert.That(orientationLandScapeText, Is.EqualTo("Current orientation: Landscape"));
		}
#endif

		[Test]
		[Order(8)]
		public void MultiTriggerComplexConditions()
		{
			Exception? exception = null;

			SelectTriggerType("MultiTriggerButton");

			App.WaitForElement("MultiTriggerEmailEntry");
			App.Tap("MultiTriggerEmailEntry");
			App.EnterText("MultiTriggerEmailEntry", "user@test.com");
			VerifyScreenshotOrSetException(ref exception, "MultiTrigger_EmailOnly_Filled");

			App.ClearText("MultiTriggerEmailEntry");
			App.WaitForElement("MultiTriggerPhoneEntry");
			App.Tap("MultiTriggerPhoneEntry");
			App.EnterText("MultiTriggerPhoneEntry", "555-1234");
			VerifyScreenshotOrSetException(ref exception, "MultiTrigger_PhoneOnly_Filled");

			App.WaitForElement("MultiTriggerEmailEntry");
			App.Tap("MultiTriggerEmailEntry");
			App.EnterText("MultiTriggerEmailEntry", "user@test.com");
			VerifyScreenshotOrSetException(ref exception, "MultiTrigger_Both_Filled");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Order(9)]
		public void AdaptiveTriggerChangesOrientation()
		{
			Exception? exception = null;

			SelectTriggerType("AdaptiveTriggerButton");

			VerifyScreenshotOrSetException(ref exception, "AdaptiveTrigger_Portrait");
#if ANDROID || IOS // On mobile platforms, we can change orientation to test the adaptive trigger
			App.SetOrientationLandscape();
#elif WINDOWS || MACCATALYST
			// On desktop platforms, we can resize the window to trigger the adaptive trigger
			App.EnterFullScreen();
#endif
			VerifyScreenshotOrSetException(ref exception, "AdaptiveTrigger_Landscape");

			if (exception != null)
			{
				throw exception;
			}
		}
	}
}