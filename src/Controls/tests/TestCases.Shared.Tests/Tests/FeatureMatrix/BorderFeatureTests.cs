using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class BorderFeatureTests : UITest
	{
		public const string BorderFeatureMatrix = "Border Feature Matrix";

		public BorderFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(BorderFeatureMatrix);
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_Padding()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("PaddingEntry");
			App.EnterText("PaddingEntry", "10,20,60,10");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_StrokeThickness()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("StrokeThicknessEntry");
			App.EnterText("StrokeThicknessEntry", "10");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_StrokeColor()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("BlueColorButton");
			App.Tap("BlueColorButton");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //For more information, see : https://github.com/dotnet/maui/issues/29661
		[Test]
		[Category(UITestCategories.Border)]
		public void Border_StrokrDashArray()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("StrokeDashArrayEntry");
			App.EnterText("StrokeDashArrayEntry", "3,5");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();

			App.WaitForElement("Options");
		}

#endif
		[Test]
		[Category(UITestCategories.Border)]
		public void Border_StrokrDashArrayWithDashOffset()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("StrokeDashArrayEntry");
			App.EnterText("StrokeDashArrayEntry", "3,5");
			App.WaitForElement("DashOffsetEntry");
			App.EnterText("DashOffsetEntry", "2");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_StrokrDashOffset()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("DashOffsetEntry");
			App.EnterText("DashOffsetEntry", "4");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_StrokeShape_RoundRectangle()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("RoundRectangleShapeRadio");
			App.Tap("RoundRectangleShapeRadio");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_StrokeShape_Rectangle()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("RectangleShapeRadio");
			App.Tap("RectangleShapeRadio");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //For more information, see : https://github.com/dotnet/maui/issues/29661
		[Test]
		[Category(UITestCategories.Border)]
		public void Border_LineCap_Flat()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("LineCapFlatRadio");
			App.Tap("LineCapFlatRadio");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_LineCap_Round()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("LineCapRoundRadio");
			App.Tap("LineCapRoundRadio");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_LineCap_Square()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("LineCapSquareRadio");
			App.Tap("LineCapSquareRadio");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}
#endif

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_Shadow()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("OffsetXEntry");
			App.EnterText("OffsetXEntry", "10");
			App.WaitForElement("OffsetYEntry");
			App.EnterText("OffsetYEntry", "10");
			App.WaitForElement("RadiusEntry");
			App.EnterText("RadiusEntry", "10");
			App.WaitForElement("OpacityEntry");
			App.EnterText("OpacityEntry", "0.8");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_Content_Button()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("ButtonRadioButton");
			App.Tap("ButtonRadioButton");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_Content_Image()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("ImageRadioButton");
			App.Tap("ImageRadioButton");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_StrokeDashArray_WithFlatLineCap()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("StrokeDashArrayEntry");
			App.EnterText("StrokeDashArrayEntry", "3,5");

			App.WaitForElement("DashOffsetEntry");
			App.EnterText("DashOffsetEntry", "2");

			App.WaitForElement("LineCapFlatRadio");
			App.Tap("LineCapFlatRadio");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_StrokeDashArray_WithRoundLineCap()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("StrokeDashArrayEntry");
			App.EnterText("StrokeDashArrayEntry", "3,5");

			App.WaitForElement("DashOffsetEntry");
			App.EnterText("DashOffsetEntry", "2");

			App.WaitForElement("LineCapRoundRadio");
			App.Tap("LineCapRoundRadio");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Border)]
		public void Border_StrokeDashArray_WithSquareLineCap()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("StrokeDashArrayEntry");
			App.EnterText("StrokeDashArrayEntry", "3,5");

			App.WaitForElement("DashOffsetEntry");
			App.EnterText("DashOffsetEntry", "2");

			App.WaitForElement("LineCapSquareRadio");
			App.Tap("LineCapSquareRadio");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			VerifyScreenshot();
		}
	}
}
