using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Trigger)]
	public class TriggersFeatureTests : _GalleryUITest
	{
		public const string TriggerFeatureMatrix = "Triggers Feature Matrix";
		public override string GalleryPageName => TriggerFeatureMatrix;

		public TriggersFeatureTests(TestDevice device)
			: base(device)
		{
		}

		/// <summary>
		/// Navigates to the Options page, selects a trigger type, and applies it
		/// </summary>
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

		public void PropertyTrigger_FocusChangesEntryBackground()
		{
			// Navigate to Property Trigger section
			SelectTriggerType("PropertyTriggerButton");

			// Wait for the Property Trigger section to be visible
			App.WaitForElement("PropertyTriggerSection");
			App.WaitForElement("PropertyTriggerEntry");

			// Take screenshot before focus
			VerifyScreenshot("PropertyTrigger_BeforeFocus");

			// Focus on the entry to trigger the property trigger
			App.Tap("PropertyTriggerEntry");

			// Wait a moment for the trigger to apply
			Task.Delay(300).Wait();

			// Take screenshot after focus - background should be yellow
			VerifyScreenshot("PropertyTrigger_AfterFocus");
		}

		[Test]

		public void DataTrigger_EmptyTextDisablesButton()
		{
			// Navigate to Data Trigger section
			SelectTriggerType("DataTriggerButton");

			// Wait for the Data Trigger section to be visible
			App.WaitForElement("DataTriggerSection");
			App.WaitForElement("DataTriggerEntry");
			App.WaitForElement("DataTriggerSaveButton");

			// Clear the entry to ensure button is disabled
			App.ClearText("DataTriggerEntry");
			App.DismissKeyboard();

			// Take screenshot with button disabled (opacity should be 0.5)
			VerifyScreenshot("DataTrigger_ButtonDisabled");

			// Enter text to enable the button
			App.Tap("DataTriggerEntry");
			App.EnterText("DataTriggerEntry", "Some text");
			App.DismissKeyboard();

			// Take screenshot with button enabled (opacity should be 1.0)
			VerifyScreenshot("DataTrigger_ButtonEnabled");
		}

		[Test]
		public void MultiTrigger_BothFieldsRequiredToEnableButton()
		{
			// Navigate to Multi Trigger section
			SelectTriggerType("MultiTriggerButton");

			// Wait for the Multi Trigger section to be visible
			App.WaitForElement("MultiTriggerSection");
			App.WaitForElement("MultiTriggerEmailEntry");
			App.WaitForElement("MultiTriggerPhoneEntry");
			App.WaitForElement("MultiTriggerSubmitButton");

			// Clear both entries
			App.ClearText("MultiTriggerEmailEntry");
			App.ClearText("MultiTriggerPhoneEntry");
			App.DismissKeyboard();

			// Take screenshot with button disabled
			VerifyScreenshot("MultiTrigger_BothEmpty_ButtonDisabled");

			// Enter only email
			App.Tap("MultiTriggerEmailEntry");
			App.EnterText("MultiTriggerEmailEntry", "test@example.com");
			App.DismissKeyboard();

			// Button should still be disabled
			VerifyScreenshot("MultiTrigger_OnlyEmail_ButtonDisabled");

			// Enter phone as well
			App.Tap("MultiTriggerPhoneEntry");
			App.EnterText("MultiTriggerPhoneEntry", "1234567890");
			App.DismissKeyboard();

			// Button should now be enabled
			VerifyScreenshot("MultiTrigger_BothFilled_ButtonEnabled");
		}

		[Test]
		public void StateTrigger_SwitchChangesGridBackground()
		{
			// Navigate to State Trigger section
			SelectTriggerType("StateTriggerButton");

			// Wait for the State Trigger section to be visible
			App.WaitForElement("StateTriggerSection");
			App.WaitForElement("StateTriggerGrid");
			App.WaitForElement("StateTriggerSwitch");

			// Take screenshot with switch off (grid background should be white)
			VerifyScreenshot("StateTrigger_SwitchOff");

			// Toggle the switch
			App.Tap("StateTriggerSwitch");

			// Wait for the state change to apply
			Task.Delay(300).Wait();

			// Take screenshot with switch on (grid background should be black)
			VerifyScreenshot("StateTrigger_SwitchOn");
		}

		[Test]
		public void CompareStateTrigger_CheckBoxChangesGridBackground()
		{
			// Navigate to Compare State Trigger section
			SelectTriggerType("CompareStateTriggerButton");

			// Wait for the Compare State Trigger section to be visible
			App.WaitForElement("CompareStateTriggerSection");
			App.WaitForElement("CompareStateTriggerGrid");
			App.WaitForElement("CompareStateCheckBox");

			// Take screenshot with checkbox unchecked (grid background should be LightGray)
			VerifyScreenshot("CompareStateTrigger_Unchecked");

			// Check the checkbox
			App.Tap("CompareStateCheckBox");

			// Wait for the state change to apply
			Task.Delay(300).Wait();

			// Take screenshot with checkbox checked (grid background should be DarkGreen)
			VerifyScreenshot("CompareStateTrigger_Checked");
		}

		[Test]
		public void DeviceStateTrigger_ShowsPlatformSpecificBackground()
		{
			// Navigate to Device State Trigger section
			SelectTriggerType("DeviceStateTriggerButton");

			// Wait for the Device State Trigger section to be visible
			App.WaitForElement("DeviceStateTriggerSection");
			App.WaitForElement("DeviceStateTriggerGrid");
			App.WaitForElement("PlatformLabel");

			// Verify platform label is displayed
			var platformText = App.FindElement("PlatformLabel").GetText();
			Assert.That(platformText, Does.Contain("Running on:"), "Platform label should display current platform");

			// Take screenshot showing platform-specific styling
			VerifyScreenshot("DeviceStateTrigger_PlatformSpecific");
		}

		[Test]
		public void OrientationStateTrigger_ShowsOrientationBackground()
		{
			// Navigate to Orientation State Trigger section
			SelectTriggerType("OrientationStateTriggerButton");

			// Wait for the Orientation State Trigger section to be visible
			App.WaitForElement("OrientationStateTriggerSection");
			App.WaitForElement("OrientationStateTriggerGrid");
			App.WaitForElement("OrientationLabel");

			// Verify orientation label is displayed
			var orientationText = App.FindElement("OrientationLabel").GetText();
			Assert.That(orientationText, Does.Contain("Current orientation:"), "Orientation label should display current orientation");

			// Take screenshot showing orientation-specific styling
			VerifyScreenshot("OrientationStateTrigger_CurrentOrientation");
		}

		[Test]
		public void EnterExitActions_FocusTriggersFadeAnimation()
		{
			// Navigate to EnterExitActions section
			SelectTriggerType("EnterExitActionsButton");

			// Wait for the EnterExitActions section to be visible
			App.WaitForElement("EnterExitActionsSection");
			App.WaitForElement("EnterExitActionsEntry");

			// Take screenshot before focus
			VerifyScreenshot("EnterExitActions_BeforeFocus");

			// Focus on the entry to trigger enter action
			App.Tap("EnterExitActionsEntry");

			// Wait for animation to complete
			Task.Delay(500).Wait();

			// Take screenshot after focus (should have faded in)
			VerifyScreenshot("EnterExitActions_AfterFocus");
		}

		[Test]
		public void AdaptiveTrigger_DisplaysWindowSize()
		{
			// Navigate to Adaptive Trigger section
			SelectTriggerType("AdaptiveTriggerButton");

			// Wait for the Adaptive Trigger section to be visible
			App.WaitForElement("AdaptiveTriggerSection");
			App.WaitForElement("AdaptiveStackLayout");
			App.WaitForElement("WindowSizeLabel");

			// Verify window size label is displayed
			var windowSizeText = App.FindElement("WindowSizeLabel").GetText();
			Assert.That(windowSizeText, Does.Contain("Window size") | Does.Contain("size"),
				"Window size label should display window dimensions");

			// Take screenshot showing adaptive layout
			VerifyScreenshot("AdaptiveTrigger_CurrentLayout");
		}

		[Test]
		public void EventTrigger_InvalidInputShowsRedText()
		{
			// Navigate to Event Trigger section
			SelectTriggerType("EventTriggerButton");

			// Wait for the Event Trigger section to be visible
			App.WaitForElement("EventTriggerSection");
			App.WaitForElement("EventTriggerEntry");

			// Enter valid number - text should be black
			App.Tap("EventTriggerEntry");
			App.EnterText("EventTriggerEntry", "123");
			App.DismissKeyboard();

			// Take screenshot with valid input
			VerifyScreenshot("EventTrigger_ValidNumber");

			// Clear and enter invalid text - text should turn red
			App.ClearText("EventTriggerEntry");
			App.Tap("EventTriggerEntry");
			App.EnterText("EventTriggerEntry", "abc");
			App.DismissKeyboard();

			// Take screenshot with invalid input (text should be red)
			VerifyScreenshot("EventTrigger_InvalidInput");
		}

		[Test]
		public void OptionsPage_ReturningClearsEntriesAndFocus()
		{
			// Navigate to Property Trigger section
			SelectTriggerType("PropertyTriggerButton");

			// Wait for the Property Trigger section to be visible
			App.WaitForElement("PropertyTriggerSection");
			App.WaitForElement("PropertyTriggerEntry");

			// Enter text and focus the entry
			App.Tap("PropertyTriggerEntry");
			App.EnterText("PropertyTriggerEntry", "Test");

			// Take screenshot with focused entry
			VerifyScreenshot("BeforeNavigation_EntryFocused");

			// Navigate to options and back
			App.Tap("Options");
			App.WaitForElement("PropertyTriggerButton");
			App.Tap("PropertyTriggerButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			// Wait for reset to complete
			Task.Delay(200).Wait();

			// Verify entry is cleared and unfocused
			App.WaitForElement("PropertyTriggerEntry");
			VerifyScreenshot("AfterNavigation_EntryCleared");
		}
	}
}