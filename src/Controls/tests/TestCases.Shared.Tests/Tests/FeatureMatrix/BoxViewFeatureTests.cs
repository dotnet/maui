using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
	public class BoxViewFeatureTests : _GalleryUITest
	{
		public const string BoxViewFeatureMatrix = "BoxView Feature Matrix";

		public override string GalleryPageName => BoxViewFeatureMatrix;

		public BoxViewFeatureTests(TestDevice device)
			: base(device)
		{
		}


		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_UniformCornerRadius()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "30");

			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_WidthAndHeight()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("WidthEntry");
			App.ClearText("WidthEntry");
			App.EnterText("WidthEntry", "300");

			App.WaitForElement("HeightEntry");
			App.ClearText("HeightEntry");
			App.EnterText("HeightEntry", "150");

			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_Color()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_Reset()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			// Change multiple properties
			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "30,30,30,30");

			// Reset back to default state and verify
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_GreenColor()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("GreenRadioButton");
			App.Tap("GreenRadioButton");

			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_OpacityZero()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("OpacityEntry");
			App.ClearText("OpacityEntry");
			App.EnterText("OpacityEntry", "0");

			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_IsVisible()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("VisibilityCheckBox");
			App.Tap("VisibilityCheckBox");

			// Use retryTimeout to allow UI to settle after visibility change
			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithColor()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			// Use retryTimeout to allow UI to settle after color change
			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_ColorWithOpacity()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			App.WaitForElement("OpacityEntry");
			App.ClearText("OpacityEntry");
			App.EnterText("OpacityEntry", "0.5");

			// Use retryTimeout to allow UI to settle after opacity change
			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithFlowDirection()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("FlowDirectionRTLCheckBox");
			App.Tap("FlowDirectionRTLCheckBox");

			// Use retryTimeout to allow UI to settle after flow direction change
			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}
#if TEST_FAILS_ON_WINDOWS // For more information see: https://github.com/dotnet/maui/issues/27732
		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithOpacityAndShadow()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("OpacityEntry");
			App.ClearText("OpacityEntry");
			App.EnterText("OpacityEntry", "0.5");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			// Use retryTimeout to allow UI to settle after shadow change
			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithColorAndShadow()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			// Use retryTimeout to allow UI to settle after shadow change
			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_ColorWithOpacityAndShadow()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			App.WaitForElement("OpacityEntry");
			App.ClearText("OpacityEntry");
			App.EnterText("OpacityEntry", "0.5");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			// Use retryTimeout to allow UI to settle after shadow change
			App.WaitForElement("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}
#endif
	}
}