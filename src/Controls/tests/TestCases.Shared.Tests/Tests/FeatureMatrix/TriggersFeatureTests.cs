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

#if IOS
		private const int CropBottomValue = 1200;
#elif ANDROID
		private const int CropBottomValue = 1100;
#endif

		public TriggersFeatureTests(TestDevice device)
			: base(device)
		{
		}

		private void VerifyScreenshotOrSetExceptionWithCroppingBottom(ref Exception? exception, string? name = null, bool isKeyBoardNotShown = false)
		{
#if IOS
			VerifyScreenshotOrSetException(ref exception, name, cropBottom: CropBottomValue, tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#elif ANDROID
			if (isKeyBoardNotShown)
				VerifyScreenshotOrSetException(ref exception, name, tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
			else
				VerifyScreenshotOrSetException(ref exception, name, cropBottom: CropBottomValue, tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#else
			VerifyScreenshotOrSetException(ref exception, name, tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#endif
		}

		private void VerifyScreenshotOrSetExceptionWithCroppingLeft(ref Exception? exception, string? name = null)
		{
#if ANDROID
			VerifyScreenshotOrSetException(ref exception, name, cropLeft: 125, tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#else
			VerifyScreenshotOrSetException(ref exception, name, tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#endif
		}

		private void SelectTriggerType(string triggerButtonAutomationId)
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement(triggerButtonAutomationId);
			App.Tap(triggerButtonAutomationId);
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Options");
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
			VerifyScreenshotOrSetExceptionWithCroppingBottom(ref exception, "PropertyTrigger_Focused");

			App.WaitForElement("PropertyTriggerDummyEntry");
			App.Tap("PropertyTriggerDummyEntry");
			VerifyScreenshotOrSetExceptionWithCroppingBottom(ref exception, "PropertyTrigger_UnFocused");

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

			// Initial state - button disabled (opacity 0.5)
			VerifyScreenshotOrSetException(ref exception, "DataTrigger_ButtonDisabled", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

			// Enter text to enable button
			App.WaitForElement("DataTriggerEntry");
			App.Tap("DataTriggerEntry");
			App.EnterText("DataTriggerEntry", "Test");
			VerifyScreenshotOrSetException(ref exception, "DataTrigger_ButtonEnabled", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

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
			VerifyScreenshotOrSetExceptionWithCroppingBottom(ref exception, "EventTrigger_ValidNumeric", isKeyBoardNotShown: true);

			// Enter invalid text (should trigger validation)
			App.ClearText("EventTriggerEntry");
			App.EnterText("EventTriggerEntry", "abc");
			VerifyScreenshotOrSetExceptionWithCroppingBottom(ref exception, "EventTrigger_InvalidText", isKeyBoardNotShown: true);

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

			// Initial state - switch off, background white
			VerifyScreenshotOrSetException(ref exception, "StateTrigger_SwitchOff", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

			// Toggle switch on - background black
			App.WaitForElement("StateTriggerSwitch");
			App.Tap("StateTriggerSwitch");
			VerifyScreenshotOrSetException(ref exception, "StateTrigger_SwitchOn", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

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

			// Initial state - unchecked, background LightGray
			VerifyScreenshotOrSetException(ref exception, "CompareStateTrigger_Unchecked", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

			// Check the checkbox - background DarkGreen
			App.WaitForElement("CompareStateCheckBox");
			App.Tap("CompareStateCheckBox");
			VerifyScreenshotOrSetException(ref exception, "CompareStateTrigger_Checked", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

			if (exception != null)
			{
				throw exception;
			}
		}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // DeviceStateTrigger is currently not supported on Windows and Catalyst platform
		[Test]
		[Order(6)]
		public void DeviceStateTriggerShowsPlatformSpecificBackground()
		{
			SelectTriggerType("DeviceStateTriggerButton");
			// Verify platform updated with correct background color for current platform
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // These platforms do not support orientation changes which is required to test the OrientationStateTrigger
#if TEST_FAILS_ON_ANDROID // This test is currently failing on Android in Automation, but can be passed manually.
		[Test]
		[Order(7)]
		public void OrientationStateTriggerShowsCorrectBackground()
		{
			SelectTriggerType("OrientationStateTriggerButton");
			App.WaitForElement("OrientationStateTriggerGrid");

			// Verify orientation label is displayed
			var orientationPortraitText = App.FindElement("OrientationLabel").GetText();
			Assert.That(orientationPortraitText, Is.EqualTo("Current orientation: Portrait"));

			App.SetOrientationLandscape();

			App.WaitForElement("Options"); // Wait for page to stabilize after orientation change
			var orientationLandscapeText = App.FindElement("OrientationLabel").GetText();
			Assert.That(orientationLandscapeText, Is.EqualTo("Current orientation: Landscape"));

			App.SetOrientationPortrait();
			App.WaitForElement("Options"); // Verify orientation label returns to portrait
		}
#endif

		[Test]
		[Order(8)]
		public void AdaptiveTriggerChangesOrientation()
		{
			Exception? exception = null;

			SelectTriggerType("AdaptiveTriggerButton");

			// Verify initial orientation (should be portrait for narrow width)
			VerifyScreenshotOrSetException(ref exception, "AdaptiveTrigger_Portrait", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
			App.SetOrientationLandscape();

			App.WaitForElement("Options"); // Wait for page to stabilize after orientation change
			VerifyScreenshotOrSetExceptionWithCroppingLeft(ref exception, "AdaptiveTrigger_Landscape");

			App.SetOrientationPortrait();
			App.WaitForElement("Options"); // Wait for page to stabilize after orientation change

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
			VerifyScreenshotOrSetException(ref exception, "MultiTrigger_EmailOnly_Filled", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

			App.ClearText("MultiTriggerEmailEntry");
			App.WaitForElement("MultiTriggerPhoneEntry");
			App.Tap("MultiTriggerPhoneEntry");
			App.EnterText("MultiTriggerPhoneEntry", "555-1234");
			VerifyScreenshotOrSetException(ref exception, "MultiTrigger_PhoneOnly_Filled", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

			App.WaitForElement("MultiTriggerEmailEntry");
			App.Tap("MultiTriggerEmailEntry");
			App.EnterText("MultiTriggerEmailEntry", "user@test.com");
			VerifyScreenshotOrSetException(ref exception, "MultiTrigger_Both_Filled", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

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
		// On Windows AutomationId is not working for BoxView. For more details see: https://github.com/dotnet/maui/issues/4715
#if TEST_FAILS_ON_WINDOWS
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
#endif
	}
}
