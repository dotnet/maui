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

			// Verify platform label is displayed correctly
			var platformText = App.FindElement("PlatformLabel").GetText();
#if ANDROID
			Assert.That(platformText, Is.EqualTo("Running on: Android"));
#elif IOS
            Assert.That(platformText, Is.EqualTo("Running on: iOS"));
#elif MACCATALYST
            Assert.That(platformText, Is.EqualTo("Running on: MacCatalyst"));
#else
            Assert.That(platformText, Does.Contain("Running on: Windows"));
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

		[Test]
		[Order(8)]
		public void AdaptiveTriggerChangesOrientation()
		{
			Exception? exception = null;

			SelectTriggerType("AdaptiveTriggerButton");

			// Verify initial orientation (should be portrait for narrow width)
			VerifyScreenshotOrSetException(ref exception, "AdaptiveTrigger_Portrait");
			App.SetOrientationLandscape();
			// Verify landscape orientation after change
			VerifyScreenshotOrSetException(ref exception, "AdaptiveTrigger_Landscape");

			if (exception != null)
			{
				throw exception;
			}
		}
#endif

		[Test]
		[Order(9)]
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
		[Order(10)]
		public void EnterExitActionsTriggerEntryFocusUnfocus()
		{
			SelectTriggerType("EnterExitActionsButton");

			// Verify both entries exist
			App.WaitForElement("EnterExitActionsEntry");
			App.WaitForElement("EnterExitActionsDummyEntry");

			// Focus the entry - triggers EnterAction (fade animation from 0)
			App.Tap("EnterExitActionsEntry");
			App.WaitForElement("EnterExitActionsEntry");

			// Unfocus by tapping dummy entry - triggers ExitAction (fade animation from 1)
			App.Tap("EnterExitActionsDummyEntry");
			App.WaitForElement("EnterExitActionsEntry");

			// Verify info label confirms trigger type
			var infoText = App.FindElement("EnterExitActionsInfo").GetText();
			Assert.That(infoText, Is.EqualTo("Tests: EnterActions, ExitActions, animations"));
		}

		[Test]
		[Order(11)]
		public void DataTriggerVerifiesButtonTextAndEntryInput()
		{
			SelectTriggerType("DataTriggerButton");
			App.WaitForElement("DataTriggerEntry");

			// Verify save button has correct text
			var buttonText = App.FindElement("DataTriggerSaveButton").GetText();
			Assert.That(buttonText, Is.EqualTo("Save"));

			// Enter text and verify entry accepts input
			App.Tap("DataTriggerEntry");
			App.EnterText("DataTriggerEntry", "Test");
			App.WaitForElement("DataTriggerSaveButton");

			// Clear text and verify entry is reset
			App.ClearText("DataTriggerEntry");
			App.WaitForElement("DataTriggerSaveButton");

			// Verify info label text
			var infoText = App.FindElement("DataTriggerInfo").GetText();
			Assert.That(infoText, Is.EqualTo("Tests: DataTrigger, binding-based condition"));
		}

		[Test]
		[Order(12)]
		public void EventTriggerVerifiesEntryInput()
		{
			SelectTriggerType("EventTriggerButton");
			App.WaitForElement("EventTriggerEntry");

			// Enter valid numeric text and verify entry accepts it
			App.Tap("EventTriggerEntry");
			App.EnterText("EventTriggerEntry", "123");
			App.WaitForElement("EventTriggerEntry");

			// Clear and enter non-numeric text to trigger validation
			App.ClearText("EventTriggerEntry");
			App.EnterText("EventTriggerEntry", "abc");
			App.WaitForElement("EventTriggerEntry");

			// Verify info label text
			var infoText = App.FindElement("EventTriggerInfo").GetText();
			Assert.That(infoText, Is.EqualTo("Tests: TextChanged event, custom TriggerAction"));
		}

		[Test]
		[Order(13)]
		public void MultiTriggerVerifiesFieldsAndSubmitButton()
		{
			SelectTriggerType("MultiTriggerButton");

			App.WaitForElement("MultiTriggerEmailEntry");
			App.WaitForElement("MultiTriggerPhoneEntry");

			// Verify submit button text
			var buttonText = App.FindElement("MultiTriggerSubmitButton").GetText();
			Assert.That(buttonText, Is.EqualTo("Submit"));

			// Fill email only
			App.Tap("MultiTriggerEmailEntry");
			App.EnterText("MultiTriggerEmailEntry", "user@test.com");
			App.WaitForElement("MultiTriggerSubmitButton");

			// Fill phone field too (both filled - multi-trigger condition met)
			App.Tap("MultiTriggerPhoneEntry");
			App.EnterText("MultiTriggerPhoneEntry", "555-1234");
			App.WaitForElement("MultiTriggerSubmitButton");

			// Clear email (condition no longer met)
			App.ClearText("MultiTriggerEmailEntry");
			App.WaitForElement("MultiTriggerSubmitButton");

			// Verify info label text
			var infoText = App.FindElement("MultiTriggerInfo").GetText();
			Assert.That(infoText, Is.EqualTo("Tests: Multiple conditions, combined logic"));
		}

		[Test]
		[Order(14)]
		public void StateTriggerVerifiesGridLabelTextAfterToggle()
		{
			SelectTriggerType("StateTriggerButton");
			App.WaitForElement("StateTriggerSwitch");

			// Verify grid label text
			var labelText = App.FindElement("StateTriggerGridLabel").GetText();
			Assert.That(labelText, Is.EqualTo("Grid background changes based on switch"));

			// Toggle switch on and verify label still present
			App.Tap("StateTriggerSwitch");
			App.WaitForElement("StateTriggerGridLabel");

			// Toggle switch off and verify label text unchanged
			App.Tap("StateTriggerSwitch");
			var labelTextAfter = App.FindElement("StateTriggerGridLabel").GetText();
			Assert.That(labelTextAfter, Is.EqualTo("Grid background changes based on switch"));

			// Verify info label text
			var infoText = App.FindElement("StateTriggerInfo").GetText();
			Assert.That(infoText, Is.EqualTo("Tests: StateTrigger with IsActive binding"));
		}

		[Test]
		[Order(15)]
		public void CompareStateTriggerVerifiesLabelAfterCheckToggle()
		{
			SelectTriggerType("CompareStateTriggerButton");
			App.WaitForElement("CompareStateCheckBox");

			// Verify label text
			var labelText = App.FindElement("CompareStateLabel").GetText();
			Assert.That(labelText, Is.EqualTo("Check to change background"));

			// Check the checkbox and verify label still present
			App.Tap("CompareStateCheckBox");
			App.WaitForElement("CompareStateLabel");

			// Uncheck and verify label text unchanged
			App.Tap("CompareStateCheckBox");
			var labelTextAfter = App.FindElement("CompareStateLabel").GetText();
			Assert.That(labelTextAfter, Is.EqualTo("Check to change background"));

			// Verify info label text
			var infoText = App.FindElement("CompareStateTriggerInfo").GetText();
			Assert.That(infoText, Is.EqualTo("Tests: CompareStateTrigger with Property binding"));
		}

		[Test]
		[Order(16)]
		public void PropertyTriggerVerifiesEntryFocusInteraction()
		{
			SelectTriggerType("PropertyTriggerButton");

			App.WaitForElement("PropertyTriggerEntry");
			App.WaitForElement("PropertyTriggerDummyEntry");

			// Verify info label text
			var infoText = App.FindElement("PropertyTriggerInfo").GetText();
			Assert.That(infoText, Is.EqualTo("Tests: IsFocused property, multiple setters"));

			// Focus entry (triggers IsFocused=True property trigger)
			App.Tap("PropertyTriggerEntry");
			App.WaitForElement("PropertyTriggerEntry");

			// Unfocus by tapping dummy (triggers IsFocused=False, reverts setters)
			App.Tap("PropertyTriggerDummyEntry");
			App.WaitForElement("PropertyTriggerEntry");

			// Re-focus to verify trigger can be re-activated
			App.Tap("PropertyTriggerEntry");
			App.WaitForElement("PropertyTriggerEntry");
		}

		[Test]
		[Order(17)]
		public void AdaptiveTriggerVerifiesWindowSizeLabelAndElements()
		{
			SelectTriggerType("AdaptiveTriggerButton");

			// Verify window size label contains expected text
			var sizeText = App.FindElement("WindowSizeLabel").GetText();
			Assert.That(sizeText, Does.Contain("Window size"));

			// Verify all colored boxes exist within the adaptive layout
			App.WaitForElement("AdaptiveBoxRed");
			App.WaitForElement("AdaptiveBoxGreen");
			App.WaitForElement("AdaptiveBoxBlue");

			// Verify info label text
			var infoText = App.FindElement("AdaptiveTriggerInfo").GetText();
			Assert.That(infoText, Is.EqualTo("Tests: MinWindowWidth, responsive layout"));
		}
	}
}