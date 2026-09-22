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

		private void VerifyScreenshotOrSetExceptionWithCroppingBottom(ref Exception? exception, string? name = null)
		{
#if IOS
			VerifyScreenshotOrSetException(ref exception, name, cropBottom: CropBottomValue, tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#elif ANDROID
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
