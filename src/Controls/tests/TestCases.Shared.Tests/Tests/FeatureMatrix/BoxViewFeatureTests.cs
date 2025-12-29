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
		public void BoxView_IsVisible()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("VisibilityCheckBox");
			App.Tap("VisibilityCheckBox");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithColor()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("CornerRadiusLabel");
			App.Tap("CornerRadiusLabel");

			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			VerifyScreenshot();
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
			App.EnterText("OpacityEntry", "0.5");

			App.WaitForElement("OpacityLabel");
			App.Tap("OpacityLabel");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithFlowDirection()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("CornerRadiusLabel");
			App.Tap("CornerRadiusLabel");

			App.WaitForElement("FlowDirectionRTLCheckBox");
			App.Tap("FlowDirectionRTLCheckBox");

			VerifyScreenshot();
		}
#if TEST_FAILS_ON_WINDOWS // For more information see: https://github.com/dotnet/maui/issues/27732
		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithOpacityAndShadow()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("CornerRadiusLabel");
			App.Tap("CornerRadiusLabel");

			App.WaitForElement("OpacityEntry");
			App.EnterText("OpacityEntry", "0.5");

			App.WaitForElement("OpacityLabel");
			App.Tap("OpacityLabel");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithColorAndShadow()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("CornerRadiusLabel");
			App.Tap("CornerRadiusLabel");

			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			VerifyScreenshot();
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
			App.EnterText("OpacityEntry", "0.5");

			App.WaitForElement("OpacityLabel");
			App.Tap("OpacityLabel");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			VerifyScreenshot();
		}
#endif
	}
}