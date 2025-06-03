using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
	public class BoxViewFeatureTests : UITest
	{
		public const string BoxViewFeatureMatrix = "BoxView Feature Matrix";

		public BoxViewFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(BoxViewFeatureMatrix);
		}


		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithOpacity()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");


			VerifyScreenshot();
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

			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithShadow()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

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

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithOpacityAndSahdow()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("OpacityEntry");
			App.EnterText("OpacityEntry", "0.5");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_CornerRadiusWithColorAndSahdow()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("CornerRadiusEntry");
			App.EnterText("CornerRadiusEntry", "60,10,20,40");

			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxView_ColorWithOpacityAndSahdow()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("RedRadioButton");
			App.Tap("RedRadioButton");

			App.WaitForElement("OpacityEntry");
			App.EnterText("OpacityEntry", "0.5");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			VerifyScreenshot();
		}
	}
}