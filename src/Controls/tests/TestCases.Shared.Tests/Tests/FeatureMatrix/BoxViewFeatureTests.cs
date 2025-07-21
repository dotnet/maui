using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.BoxView)]
	public class BoxViewFeatureTests : _GalleryUITest
	{
		public const string BoxViewFeatureMatrix = "BoxView Feature Matrix";

		public override string GalleryPageName => BoxViewFeatureMatrix;

		public BoxViewFeatureTests(TestDevice device)
			: base(device)
		{
		}

		void ResetBoxView()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
		}

		// ── Color tests (Order 1–3) ──

		[Test, Order(1)]
		public void BoxView_Color()
		{
			ResetBoxView();
			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(2)]
		public void BoxView_GreenColor()
		{
			ResetBoxView();
			App.WaitForElement("GreenRadioButton");
			App.Tap("GreenRadioButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(3)]
		public void BoxView_BlueColor()
		{
			ResetBoxView();
			// Switch away from the default Blue then back to explicitly verify the Blue radio button
			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");
			App.WaitForElement("BlueRadioButton");
			App.Tap("BlueRadioButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		// ── CornerRadius tests (Order 4) ──

		[Test, Order(4)]
		public void BoxView_UniformCornerRadius()
		{
			ResetBoxView();
			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "30");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		// ── Dimension tests (Order 5) ──

		[Test, Order(5)]
		public void BoxView_WidthAndHeight()
		{
			ResetBoxView();
			App.WaitForElement("WidthEntry");
			App.ClearText("WidthEntry");
			App.EnterText("WidthEntry", "300");
			App.WaitForElement("HeightEntry");
			App.ClearText("HeightEntry");
			App.EnterText("HeightEntry", "150");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		// ── Opacity tests (Order 6) ──

		[Test, Order(6)]
		public void BoxView_OpacityZero()
		{
			ResetBoxView();
			App.WaitForElement("OpacityEntry");
			App.ClearText("OpacityEntry");
			App.EnterText("OpacityEntry", "0");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		// ── Visibility tests (Order 7) ──

		[Test, Order(7)]
		public void BoxView_IsVisible()
		{
			ResetBoxView();
			App.WaitForElement("VisibilityCheckBox");
			App.Tap("VisibilityCheckBox");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		// ── Combined property tests (Order 8–11) ──

		[Test, Order(8)]
		public void BoxView_CornerRadiusWithColor()
		{
			ResetBoxView();
			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");
			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(9)]
		public void BoxView_ColorWithOpacity()
		{
			ResetBoxView();
			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");
			App.WaitForElement("OpacityEntry");
			App.ClearText("OpacityEntry");
			App.EnterText("OpacityEntry", "0.5");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(10)]
		[Ignore("Fails on all platforms, related issue link: https://github.com/dotnet/maui/issues/34402")]
		public void BoxView_CornerRadiusWithFlowDirection()
		{
			ResetBoxView();
			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");
			App.WaitForElement("FlowDirectionRTLCheckBox");
			App.Tap("FlowDirectionRTLCheckBox");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(11)]
		public void BoxView_Reset()
		{
			ResetBoxView();
			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");
			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "30,30,30,30");

			// Reset back to default state and verify
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

#if TEST_FAILS_ON_WINDOWS // For more information see: https://github.com/dotnet/maui/issues/27732
		// ── Shadow tests - disabled on Windows (Order 12–14) ──

		[Test, Order(12)]
		public void BoxView_CornerRadiusWithOpacityAndShadow()
		{
			ResetBoxView();
			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");
			App.WaitForElement("OpacityEntry");
			App.ClearText("OpacityEntry");
			App.EnterText("OpacityEntry", "0.5");
			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(13)]
		public void BoxView_CornerRadiusWithColorAndShadow()
		{
			ResetBoxView();
			App.WaitForElement("CornerRadiusEntry");
			App.ClearText("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");
			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");
			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}

		[Test, Order(14)]
		public void BoxView_ColorWithOpacityAndShadow()
		{
			ResetBoxView();
			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");
			App.WaitForElement("OpacityEntry");
			App.ClearText("OpacityEntry");
			App.EnterText("OpacityEntry", "0.5");
			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}
		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_FillWithSolidColor()
		{
			App.WaitForElement("SolidRadioButton");
			App.Tap("SolidRadioButton");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_FillWithLinearGradient()
		{
			App.WaitForElement("LinearRadioButton");
			App.Tap("LinearRadioButton");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_FillWithRadialGradient()
		{
			App.WaitForElement("RadialRadioButton");
			App.Tap("RadialRadioButton");

			VerifyScreenshot();
		}
#endif
	}
}