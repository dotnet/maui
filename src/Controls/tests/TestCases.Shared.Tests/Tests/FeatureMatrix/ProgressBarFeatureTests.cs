using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class ProgressBarFeatureTests : _GalleryUITest
	{
		public const string ProgressBarFeatureMatrix = "ProgressBar Feature Matrix";

		public override string GalleryPageName => ProgressBarFeatureMatrix;

		public ProgressBarFeatureTests(TestDevice device)
			: base(device)
		{
		}

		[Test, Order(1)]
		[Category(UITestCategories.ProgressBar)]
		public void ProgressBar_ValidateDefaultProgress()
		{
			App.WaitForElement("ProgressValueLabel");
			Assert.That(App.FindElement("ProgressValueLabel").GetText(), Is.EqualTo("0.50"));
		}

		[Test, Order(2)]
		[Category(UITestCategories.ProgressBar)]
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
		[Category(UITestCategories.ProgressBar)]
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
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.ProgressBar)]
		public void ProgressBar_SetProgressOutOfRange()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressEntry");
			App.ClearText("ProgressEntry");
			App.EnterText("ProgressEntry", "1.44");
			App.PressEnter();
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.ProgressBar)]
		public void ProgressBar_SetProgressNegativeValue()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ProgressEntry");
			App.ClearText("ProgressEntry");
			App.EnterText("ProgressEntry", "-0.44");
			App.PressEnter();
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.ProgressBar)]
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
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.ProgressBar)]
		public void ProgressBar_SetIsVisibleFalse_VerifyLabel()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("IsVisibleFalseRadio");
			App.Tap("IsVisibleFalseRadio");
			App.WaitForNoElement("ProgressBarControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.ProgressBar)]
		public void ProgressBar_ChangeFlowDirection_RTL_VerifyLabel()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.ProgressBar)]
		public void ProgressBar_ToggleShadow_VerifyVisualState()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			App.WaitForElement("ShadowTrueRadio");
			App.Tap("ShadowTrueRadio");
			App.WaitForElement("ProgressBarControl");
			VerifyScreenshot();
		}
	}
}