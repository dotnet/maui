using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.ProgressBar)]
	public class ProgressBarFeatureTests : _GalleryUITest
	{
		public const string ProgressBarFeatureMatrix = "ProgressBar Feature Matrix";

		public override string GalleryPageName => ProgressBarFeatureMatrix;

		public ProgressBarFeatureTests(TestDevice device)
			: base(device)
		{
		}

		[Test, Order(1)]
		public void ProgressBar_ValidateDefaultProgress()
		{
			App.WaitForElement("ProgressValueLabel");
			Assert.That(App.FindElement("ProgressValueLabel").GetText(), Is.EqualTo("0.50"));
		}

		[Test, Order(2)]
		public void ProgressBar_SetProgress_VerifyLabel()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressEntry");
			App.ClearText("ProgressEntry");
			App.EnterText("ProgressEntry", "0.80");
			App.PressEnter();
			App.WaitForElement("ProgressValueLabel");
			Assert.That(App.FindElement("ProgressValueLabel").GetText(), Is.EqualTo("0.80"));
		}

		[Test, Order(3)]
		public void ProgressBar_ProgressToMethod_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressToEntry");
			App.ClearText("ProgressToEntry");
			App.EnterText("ProgressToEntry", "0.90");
			App.PressEnter();
			App.WaitForElement("ProgressToButton");
			App.Tap("ProgressToButton");
			Task.Delay(1000).Wait();
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(4)]
		public void ProgressBar_SetProgress_Zero()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressEntry");
			App.ClearText("ProgressEntry");
			App.EnterText("ProgressEntry", "0.0");
			App.PressEnter();
			App.WaitForElement("ProgressValueLabel");
			Assert.That(App.FindElement("ProgressValueLabel").GetText(), Is.EqualTo("0.00"));
		}

		[Test, Order(5)]
		public void ProgressBar_SetProgress_One()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressEntry");
			App.ClearText("ProgressEntry");
			App.EnterText("ProgressEntry", "1.0");
			App.PressEnter();
			App.WaitForElement("ProgressValueLabel");
			Assert.That(App.FindElement("ProgressValueLabel").GetText(), Is.EqualTo("1.00"));
		}

		[Test, Order(6)]
		public void ProgressBar_SetProgressOutOfRange()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressEntry");
			App.ClearText("ProgressEntry");
			App.EnterText("ProgressEntry", "1.44");
			App.PressEnter();
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(7)]
		public void ProgressBar_SetProgressNegativeValue()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressEntry");
			App.ClearText("ProgressEntry");
			App.EnterText("ProgressEntry", "-0.44");
			App.PressEnter();
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(8)]
		public void ProgressBar_SetProgressColor_Green_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressColorGreenButton");
			App.Tap("ProgressColorGreenButton");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(9)]
		public void ProgressBar_SetProgressColor_Red_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressColorRedButton");
			App.Tap("ProgressColorRedButton");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(10)]
		public void ProgressBar_SetBackgroundColor_Orange_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("BackgroundColorOrangeButton");
			App.Tap("BackgroundColorOrangeButton");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(11)]
		public void ProgressBar_SetBackgroundColor_LightBlue_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("BackgroundColorLightBlueButton");
			App.Tap("BackgroundColorLightBlueButton");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(12)]
		public void ProgressBar_SetProgressColorAndBackgroundColor_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressEntry");
			App.ClearText("ProgressEntry");
			App.EnterText("ProgressEntry", "0.60");
			App.PressEnter();
			App.WaitForElement("ProgressColorRedButton");
			App.Tap("ProgressColorRedButton");
			App.WaitForElement("BackgroundColorLightBlueButton");
			App.Tap("BackgroundColorLightBlueButton");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(13)]
		public void ProgressBar_SetIsVisibleFalse_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("IsVisibleFalseRadio");
			App.Tap("IsVisibleFalseRadio");
			App.WaitForNoElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(14)]
		public void ProgressBar_SetIsVisibleTrue_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("IsVisibleFalseRadio");
			App.Tap("IsVisibleFalseRadio");
			App.WaitForNoElement("ProgressBarControl");
			App.WaitForElement("IsVisibleTrueRadio");
			App.Tap("IsVisibleTrueRadio");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(15)]
		public void ProgressBar_ChangeFlowDirection_RTL_VerifyVisualState()
		{
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/33969
			}
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(16)]
		public void ProgressBar_ChangeFlowDirection_LTR_VerifyVisualState()
		{
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/33969
			}
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("FlowDirectionLTR");
			App.Tap("FlowDirectionLTR");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(17)]
		public void ProgressBar_ToggleShadow_On_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ShadowTrueRadio");
			App.Tap("ShadowTrueRadio");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(18)]
		public void ProgressBar_ToggleShadow_Off_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ShadowTrueRadio");
			App.Tap("ShadowTrueRadio");
			App.WaitForElement("ShadowFalseRadio");
			App.Tap("ShadowFalseRadio");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(19)]
		public void ProgressBar_SetIsEnabled_False_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("IsEnabledFalseRadio");
			App.Tap("IsEnabledFalseRadio");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(20)]
		public void ProgressBar_SetIsEnabled_True_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("IsEnabledFalseRadio");
			App.Tap("IsEnabledFalseRadio");
			App.WaitForElement("IsEnabledTrueRadio");
			App.Tap("IsEnabledTrueRadio");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}
	}
}
