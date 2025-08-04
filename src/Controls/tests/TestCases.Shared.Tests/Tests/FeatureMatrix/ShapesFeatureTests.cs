using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class ShapesFeatureTests : UITest
{
	public const string ShapesFeatureMatrix = "Shapes Feature Matrix";

	public ShapesFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ShapesFeatureMatrix);
	}

	public void VerifyShapeScreenshot()
	{
#if WINDOWS
		VerifyScreenshot(cropTop: 100);
#else
		VerifyScreenshot();
#endif
	}

#if TEST_FAILS_ON_WINDOWS //For more information see: https://github.com/dotnet/maui/issues/29812
	[Test]
	[Category(UITestCategories.Shape)]
	public void Rectangle_FillColorWithStrokeColor_Shadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Ellipse_FillColorWithStrokeColor_Shadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EllipseRadioButton");
		App.Tap("EllipseRadioButton");

		App.WaitForElement("FillColorGreenRadioButton");
		App.Tap("FillColorGreenRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Line_FillColorWithStrokeColor_Shadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("LineRadioButton");
		App.Tap("LineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("X1Entry");
		App.Tap("X1Entry");
		App.ClearText("X1Entry");
		App.EnterText("X1Entry", "50");

		App.WaitForElement("Y1Entry");
		App.Tap("Y1Entry");
		App.ClearText("Y1Entry");
		App.EnterText("Y1Entry", "50");

		App.WaitForElement("X2Entry");
		App.Tap("X2Entry");
		App.ClearText("X2Entry");
		App.EnterText("X2Entry", "200");

		App.WaitForElement("Y2Entry");
		App.Tap("Y2Entry");
		App.ClearText("Y2Entry");
		App.EnterText("Y2Entry", "150");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Polygon_FillColorWithStrokeColor_Shadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolygonRadioButton");
		App.Tap("PolygonRadioButton");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void PolyLine_FillColorWithStrokeColor_Shadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolyLineRadioButton");
		App.Tap("PolyLineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Path_FillColorWithStrokeColor_Shadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.Shape)]
	public void Rectangle_DashArray_DashOffset_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeDashOffsetEntry");
		App.Tap("StrokeDashOffsetEntry");
		App.ClearText("StrokeDashOffsetEntry");
		App.EnterText("StrokeDashOffsetEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Ellipse_DashArray_DashOffset_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EllipseRadioButton");
		App.Tap("EllipseRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeDashOffsetEntry");
		App.Tap("StrokeDashOffsetEntry");
		App.ClearText("StrokeDashOffsetEntry");
		App.EnterText("StrokeDashOffsetEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void PolyLine_DashArray_DashOffset_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolyLineRadioButton");
		App.Tap("PolyLineRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeDashOffsetEntry");
		App.Tap("StrokeDashOffsetEntry");
		App.ClearText("StrokeDashOffsetEntry");
		App.EnterText("StrokeDashOffsetEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Polygon_DashArray_DashOffset_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolygonRadioButton");
		App.Tap("PolygonRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeDashOffsetEntry");
		App.Tap("StrokeDashOffsetEntry");
		App.ClearText("StrokeDashOffsetEntry");
		App.EnterText("StrokeDashOffsetEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Path_DashArray_DashOffset_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeDashOffsetEntry");
		App.Tap("StrokeDashOffsetEntry");
		App.ClearText("StrokeDashOffsetEntry");
		App.EnterText("StrokeDashOffsetEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Line_DashArray_DashOffset_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("LineRadioButton");
		App.Tap("LineRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeDashOffsetEntry");
		App.Tap("StrokeDashOffsetEntry");
		App.ClearText("StrokeDashOffsetEntry");
		App.EnterText("StrokeDashOffsetEntry", "5");

		App.WaitForElement("X1Entry");
		App.Tap("X1Entry");
		App.ClearText("X1Entry");
		App.EnterText("X1Entry", "50");

		App.WaitForElement("Y1Entry");
		App.Tap("Y1Entry");
		App.ClearText("Y1Entry");
		App.EnterText("Y1Entry", "50");

		App.WaitForElement("X2Entry");
		App.Tap("X2Entry");
		App.ClearText("X2Entry");
		App.EnterText("X2Entry", "200");

		App.WaitForElement("Y2Entry");
		App.Tap("Y2Entry");
		App.ClearText("Y2Entry");
		App.EnterText("Y2Entry", "150");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Path_Points_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "4");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 20,100 Q 40,60 60,100 T 100,100 T 140,100 T 180,100 T 220,100 T 260,100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void PolyLine_Points_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolyLineRadioButton");
		App.Tap("PolyLineRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "4");

		App.WaitForElement("PolyLinePointsEntry");
		App.Tap("PolyLinePointsEntry");
		App.ClearText("PolyLinePointsEntry");
		App.EnterText("PolyLinePointsEntry", "0,0 10,30 15,0 18,60 23,30 35,30 40,0 43,60 48,30 100,30");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Shape_Polygon_Pentagon()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolygonRadioButton");
		App.Tap("PolygonRadioButton");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("PolygonPointsEntry");
		App.Tap("PolygonPointsEntry");
		App.ClearText("PolygonPointsEntry");
		App.EnterText("PolygonPointsEntry", "150,50 195,90 175,140 125,140 105,90");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}


	[Test]
	[Category(UITestCategories.Shape)]
	public void Shape_Rectangle_XAndYRadius()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("RadiusXEntry");
		App.Tap("RadiusXEntry");
		App.ClearText("RadiusXEntry");
		App.EnterText("RadiusXEntry", "50");

		App.WaitForElement("RadiusYEntry");
		App.Tap("RadiusYEntry");
		App.ClearText("RadiusYEntry");
		App.EnterText("RadiusYEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Shape_Rectangle_HeightAndWidth()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RectangleHeightEntry");
		App.Tap("RectangleHeightEntry");
		App.ClearText("RectangleHeightEntry");
		App.EnterText("RectangleHeightEntry", "100");

		App.WaitForElement("RectangleWidthEntry");
		App.Tap("RectangleWidthEntry");
		App.ClearText("RectangleWidthEntry");
		App.EnterText("RectangleWidthEntry", "200");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Rectangle_StrokeColor_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Ellipse_StrokeColor_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EllipseRadioButton");
		App.Tap("EllipseRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}
	[Test]
	[Category(UITestCategories.Shape)]
	public void Line_StrokeColor_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("LineRadioButton");
		App.Tap("LineRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("X1Entry");
		App.Tap("X1Entry");
		App.ClearText("X1Entry");
		App.EnterText("X1Entry", "50");

		App.WaitForElement("Y1Entry");
		App.Tap("Y1Entry");
		App.ClearText("Y1Entry");
		App.EnterText("Y1Entry", "50");

		App.WaitForElement("X2Entry");
		App.Tap("X2Entry");
		App.ClearText("X2Entry");
		App.EnterText("X2Entry", "200");

		App.WaitForElement("Y2Entry");
		App.Tap("Y2Entry");
		App.ClearText("Y2Entry");
		App.EnterText("Y2Entry", "150");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}
	[Test]
	[Category(UITestCategories.Shape)]
	public void Polygon_StrokeColor_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolygonRadioButton");
		App.Tap("PolygonRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("FillColorGreenRadioButton");
		App.Tap("FillColorGreenRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}
	[Test]
	[Category(UITestCategories.Shape)]
	public void PolyLine_StrokeColor_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolyLineRadioButton");
		App.Tap("PolyLineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Path_StrokeColor_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("FillColorGreenRadioButton");
		App.Tap("FillColorGreenRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Rectangle_StrokeColor_DashArray_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shape)]
	public void Ellipse_StrokeColor_DashArray_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EllipseRadioButton");
		App.Tap("EllipseRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}
	[Test]
	[Category(UITestCategories.Shape)]
	public void Line_StrokeColor_DashArray_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("LineRadioButton");
		App.Tap("LineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("X1Entry");
		App.Tap("X1Entry");
		App.ClearText("X1Entry");
		App.EnterText("X1Entry", "50");

		App.WaitForElement("Y1Entry");
		App.Tap("Y1Entry");
		App.ClearText("Y1Entry");
		App.EnterText("Y1Entry", "50");

		App.WaitForElement("X2Entry");
		App.Tap("X2Entry");
		App.ClearText("X2Entry");
		App.EnterText("X2Entry", "200");

		App.WaitForElement("Y2Entry");
		App.Tap("Y2Entry");
		App.ClearText("Y2Entry");
		App.EnterText("Y2Entry", "150");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}
	[Test]
	[Category(UITestCategories.Shape)]
	public void Polygon_StrokeColor_DashArray_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolygonRadioButton");
		App.Tap("PolygonRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}
	[Test]
	[Category(UITestCategories.Shape)]
	public void PolyLine_StrokeColor_DashArray_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolyLineRadioButton");
		App.Tap("PolyLineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}
	[Test]
	[Category(UITestCategories.Shape)]
	public void Path_StrokeColor_DashArray_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeDashArrayEntry");
		App.Tap("StrokeDashArrayEntry");
		App.ClearText("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,2");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

}
