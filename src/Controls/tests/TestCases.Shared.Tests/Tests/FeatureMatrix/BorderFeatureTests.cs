using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

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
	public void Border_PaddingWithContent_Label()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PaddingEntry");
		App.EnterText("PaddingEntry", "10,20,60,10");

		App.WaitForElement("LabelRadioButton");
		App.Tap("LabelRadioButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeColorWithPaddingAndContent_Image()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RedColorButton");
		App.Tap("RedColorButton");

		App.WaitForElement("PaddingEntry");
		App.EnterText("PaddingEntry", "10,20,60,10");

		App.WaitForElement("ImageRadioButton");
		App.Tap("ImageRadioButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeColorWithStrokeShape_RoundRectangle()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("BlueColorButton");
		App.Tap("BlueColorButton");

		App.WaitForElement("RoundRectangleShapeRadio");
		App.Tap("RoundRectangleShapeRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeColorWithStrokeThickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("GreenColorButton");
		App.Tap("GreenColorButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeShapeWithStrokeThickness_Ellipse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EllipseShapeRadio");
		App.Tap("EllipseShapeRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}


#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //For more information, see : https://github.com/dotnet/maui/issues/29661 , https://github.com/dotnet/maui/issues/29743

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeShapeWithStrokeLineJoin_Bevel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolygonShapeRadio");
		App.Tap("PolygonShapeRadio");

		App.WaitForElement("BevelLineJoinRadio");
		App.Tap("BevelLineJoinRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}


	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeShapeWithDashArray_Path()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("PathShapeRadio");
		App.Tap("PathShapeRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeThicknessWithDashArray()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("Apply");
		App.Tap("Apply");

#if WINDOWS
		VerifyScreenshot(cropTop: 100);
#else
		VerifyScreenshot();
#endif
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeDashArrayWithDashOffset()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("DashOffsetEntry");
		App.EnterText("DashOffsetEntry", "2");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");

#if WINDOWS
		VerifyScreenshot(cropTop: 100);
#else
		VerifyScreenshot();
#endif
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeColorWithStrokeLineJoin_Round()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RectangleShapeRadio");
		App.Tap("RectangleShapeRadio");

		App.WaitForElement("RoundLineJoinRadio");
		App.Tap("RoundLineJoinRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}
#if TEST_FAILS_ON_WINDOWS // For more information, see : https://github.com/dotnet/maui/issues/29741
	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeDashArrayWithStrokeLineCap_Round()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("RoundLineCapRadio");
		App.Tap("RoundLineCapRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeDashArrayWithDashOffsetAndStrokeLineCapRound()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("RoundLineCapRadio");
		App.Tap("RoundLineCapRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeDashArrayWithStrokeLineCap_Square()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("SquareLineCapRadio");
		App.Tap("SquareLineCapRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeDashArrayWithEllipseShapeAndStrokeLineCap_Square()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("EllipseShapeRadio");
		App.Tap("EllipseShapeRadio");

		App.WaitForElement("SquareLineCapRadio");
		App.Tap("SquareLineCapRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_PolygonShapeWithStrokeLineCap_Round()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "15");

		App.WaitForElement("PolygonShapeRadio");
		App.Tap("PolygonShapeRadio");

		App.WaitForElement("RoundLineCapRadio");
		App.Tap("RoundLineCapRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

#endif

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeDashArrayWithStrokeColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("RedColorButton");
		App.Tap("RedColorButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeThicknessWithStrokeLineJoin_Bevel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("BevelLineJoinRadio");
		App.Tap("BevelLineJoinRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "15");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void Border_PolygonShapeWithStrokeLineJoin_Bevel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RectangleShapeRadio");
		App.Tap("RectangleShapeRadio");

		App.WaitForElement("BevelLineJoinRadio");
		App.Tap("BevelLineJoinRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "15");

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
		App.WaitForElement("OpacityEntry");
		App.EnterText("OpacityEntry", "0.8");
		App.WaitForElement("RadiusEntry");
		App.EnterText("RadiusEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");

#if WINDOWS
		VerifyScreenshot(cropTop: 100);
#else
		VerifyScreenshot();
#endif
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // For more information, see : https://github.com/dotnet/maui/issues/29898
	[Test]
	[Category(UITestCategories.Border)]
	public void Border_StrokeColorWithDashArrayAndOffset()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("DashOffsetEntry");
		App.EnterText("DashOffsetEntry", "2");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "15");

		App.WaitForElement("BlackColorButton");
		App.Tap("BlackColorButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}
#endif

}
