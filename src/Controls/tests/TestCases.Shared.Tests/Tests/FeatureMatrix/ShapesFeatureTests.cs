using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Shape)]
public class ShapesFeatureTests : _GalleryUITest
{
	public const string ShapesFeatureMatrix = "Shapes Feature Matrix";
	public override string GalleryPageName => ShapesFeatureMatrix;

	public ShapesFeatureTests(TestDevice device)
		: base(device)
	{
	}

	public void VerifyShapeScreenshot()
	{
#if WINDOWS
		VerifyScreenshot(cropTop: 100);
#else
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#endif
	}

#if TEST_FAILS_ON_WINDOWS //For more information see: https://github.com/dotnet/maui/issues/29812
	[Test, Order(1)]
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

	[Test, Order(2)]
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

	[Test, Order(3)]
	public void Line_StrokeColor_Shadow()
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

	[Test, Order(4)]
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

		App.WaitForElement("PolygonPointsEntry");
		App.Tap("PolygonPointsEntry");
		App.ClearText("PolygonPointsEntry");
		App.EnterText("PolygonPointsEntry", "100,20 170,75 100,130 30,75");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(5)]
	public void Polyline_StrokeColor_Shadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolylineRadioButton");
		App.Tap("PolylineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("PolylinePointsEntry");
		App.Tap("PolylinePointsEntry");
		App.ClearText("PolylinePointsEntry");
		App.EnterText("PolylinePointsEntry", "50,100 100,50 150,100 200,50 250,100");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyShapeScreenshot();
	}

	[Test, Order(6)]
	public void Path_FillColorWithStrokeColor_Shadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,84 C 10,84 40,15 100,55 C 160,15 190,84 190,84 C 190,84 100,135 100,135 C 100,135 10,84 10,84 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(30)]
	public void RoundRectangle_FillColorWithStrokeColor_Shadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RoundRectangleRadioButton");
		App.Tap("RoundRectangleRadioButton");

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
#endif

	[Test, Order(7)]
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

	[Test, Order(8)]
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

	[Test, Order(9)]
	public void Polyline_DashArray_DashOffset_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolylineRadioButton");
		App.Tap("PolylineRadioButton");

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

		App.WaitForElement("PolylinePointsEntry");
		App.Tap("PolylinePointsEntry");
		App.ClearText("PolylinePointsEntry");
		App.EnterText("PolylinePointsEntry", "50,100 100,50 150,100 200,50 250,100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(10)]
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

		App.WaitForElement("PolygonPointsEntry");
		App.Tap("PolygonPointsEntry");
		App.ClearText("PolygonPointsEntry");
		App.EnterText("PolygonPointsEntry", "100,20 170,75 100,130 30,75");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(11)]
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
		App.EnterText("StrokeDashOffsetEntry", "3");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,84 C 10,84 40,15 100,55 C 160,15 190,84 190,84 C 190,84 100,135 100,135 C 100,135 10,84 10,84 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(12)]
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

	[Test, Order(13)]
	public void Path_PathData_Thickness()
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

	[Test, Order(14)]
	public void Polyline_Points_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolylineRadioButton");
		App.Tap("PolylineRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "4");

		App.WaitForElement("PolylinePointsEntry");
		App.Tap("PolylinePointsEntry");
		App.ClearText("PolylinePointsEntry");
		App.EnterText("PolylinePointsEntry", "10,15 20,45 25,15 28,75 33,45 45,45 50,15 53,75 58,45 110,45");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(15)]
	public void Polygon_Pentagon()
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


	[Test, Order(16)]
	public void Rectangle_XAndYRadius()
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

	[Test, Order(17)]
	public void Rectangle_HeightAndWidth()
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

	[Test, Order(18)]
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

	[Test, Order(19)]
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

	[Test, Order(20)]
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

	[Test, Order(21)]
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

		App.WaitForElement("PolygonPointsEntry");
		App.Tap("PolygonPointsEntry");
		App.ClearText("PolygonPointsEntry");
		App.EnterText("PolygonPointsEntry", "100,20 170,75 100,130 30,75");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(22)]
	public void Polyline_StrokeColor_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolylineRadioButton");
		App.Tap("PolylineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5");

		App.WaitForElement("PolylinePointsEntry");
		App.Tap("PolylinePointsEntry");
		App.ClearText("PolylinePointsEntry");
		App.EnterText("PolylinePointsEntry", "50,100 100,50 150,100 200,50 250,100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(23)]
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

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,84 C 10,84 40,15 100,55 C 160,15 190,84 190,84 C 190,84 100,135 100,135 C 100,135 10,84 10,84 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(24)]
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

	[Test, Order(25)]
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

	[Test, Order(26)]
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

	[Test, Order(27)]
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

		App.WaitForElement("PolygonPointsEntry");
		App.Tap("PolygonPointsEntry");
		App.ClearText("PolygonPointsEntry");
		App.EnterText("PolygonPointsEntry", "100,20 170,75 100,130 30,75");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(28)]
	public void Polyline_StrokeColor_DashArray_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolylineRadioButton");
		App.Tap("PolylineRadioButton");

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

		App.WaitForElement("PolylinePointsEntry");
		App.Tap("PolylinePointsEntry");
		App.ClearText("PolylinePointsEntry");
		App.EnterText("PolylinePointsEntry", "50,100 100,50 150,100 200,50 250,100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(29)]
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

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,84 C 10,84 40,15 100,55 C 160,15 190,84 190,84 C 190,84 100,135 100,135 C 100,135 10,84 10,84 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(31)]
	public void RoundRectangle_CornerRadius()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RoundRectangleRadioButton");
		App.Tap("RoundRectangleRadioButton");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "3");

		App.WaitForElement("CornerRadiusEntry");
		App.Tap("CornerRadiusEntry");
		App.ClearText("CornerRadiusEntry");
		App.EnterText("CornerRadiusEntry", "30");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(32)]
	public void RoundRectangle_DashArray_DashOffset_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RoundRectangleRadioButton");
		App.Tap("RoundRectangleRadioButton");

		App.WaitForElement("CornerRadiusEntry");
		App.Tap("CornerRadiusEntry");
		App.ClearText("CornerRadiusEntry");
		App.EnterText("CornerRadiusEntry", "20");

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

	[Test, Order(33)]
	public void Line_StrokeLineCap_Round()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("LineRadioButton");
		App.Tap("LineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "12");

		App.WaitForElement("StrokeLineCapRoundRadioButton");
		App.Tap("StrokeLineCapRoundRadioButton");

		App.WaitForElement("X1Entry");
		App.Tap("X1Entry");
		App.ClearText("X1Entry");
		App.EnterText("X1Entry", "50");

		App.WaitForElement("Y1Entry");
		App.Tap("Y1Entry");
		App.ClearText("Y1Entry");
		App.EnterText("Y1Entry", "100");

		App.WaitForElement("X2Entry");
		App.Tap("X2Entry");
		App.ClearText("X2Entry");
		App.EnterText("X2Entry", "250");

		App.WaitForElement("Y2Entry");
		App.Tap("Y2Entry");
		App.ClearText("Y2Entry");
		App.EnterText("Y2Entry", "100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(34)]
	public void Line_StrokeLineCap_Square()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("LineRadioButton");
		App.Tap("LineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "12");

		App.WaitForElement("StrokeLineCapSquareRadioButton");
		App.Tap("StrokeLineCapSquareRadioButton");

		App.WaitForElement("X1Entry");
		App.Tap("X1Entry");
		App.ClearText("X1Entry");
		App.EnterText("X1Entry", "50");

		App.WaitForElement("Y1Entry");
		App.Tap("Y1Entry");
		App.ClearText("Y1Entry");
		App.EnterText("Y1Entry", "100");

		App.WaitForElement("X2Entry");
		App.Tap("X2Entry");
		App.ClearText("X2Entry");
		App.EnterText("X2Entry", "250");

		App.WaitForElement("Y2Entry");
		App.Tap("Y2Entry");
		App.ClearText("Y2Entry");
		App.EnterText("Y2Entry", "100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(35)]
	public void Polyline_StrokeLineJoin_Round()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolylineRadioButton");
		App.Tap("PolylineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "20");

		App.WaitForElement("StrokeLineJoinRoundRadioButton");
		App.Tap("StrokeLineJoinRoundRadioButton");

		App.WaitForElement("PolylinePointsEntry");
		App.Tap("PolylinePointsEntry");
		App.ClearText("PolylinePointsEntry");
		App.EnterText("PolylinePointsEntry", "50,100 100,50 150,100 200,50 250,100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(36)]
	public void Polyline_StrokeLineJoin_Bevel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolylineRadioButton");
		App.Tap("PolylineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "20");

		App.WaitForElement("StrokeLineJoinBevelRadioButton");
		App.Tap("StrokeLineJoinBevelRadioButton");

		App.WaitForElement("PolylinePointsEntry");
		App.Tap("PolylinePointsEntry");
		App.ClearText("PolylinePointsEntry");
		App.EnterText("PolylinePointsEntry", "50,100 100,50 150,100 200,50 250,100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(37)]
	public void Path_Aspect_Uniform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("AspectUniformRadioButton");
		App.Tap("AspectUniformRadioButton");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,100 C 10,100 40,-20 100,50 C 160,-20 190,100 190,100 C 190,100 100,190 100,190 C 100,190 10,100 10,100 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(38)]
	public void Path_Aspect_Fill()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("AspectFillRadioButton");
		App.Tap("AspectFillRadioButton");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,100 C 10,100 40,-20 100,50 C 160,-20 190,100 190,100 C 190,100 100,190 100,190 C 100,190 10,100 10,100 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(39)]
	public void Path_Aspect_UniformToFill()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("AspectUniformToFillRadioButton");
		App.Tap("AspectUniformToFillRadioButton");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,100 C 10,100 40,-20 100,50 C 160,-20 190,100 190,100 C 190,100 100,190 100,190 C 100,190 10,100 10,100 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(40)]
	public void RoundRectangle_HeightAndWidth()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RoundRectangleRadioButton");
		App.Tap("RoundRectangleRadioButton");

		App.WaitForElement("RoundRectangleHeightEntry");
		App.Tap("RoundRectangleHeightEntry");
		App.ClearText("RoundRectangleHeightEntry");
		App.EnterText("RoundRectangleHeightEntry", "100");

		App.WaitForElement("RoundRectangleWidthEntry");
		App.Tap("RoundRectangleWidthEntry");
		App.ClearText("RoundRectangleWidthEntry");
		App.EnterText("RoundRectangleWidthEntry", "200");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(41)]
	public void Ellipse_HeightAndWidth()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EllipseRadioButton");
		App.Tap("EllipseRadioButton");

		App.WaitForElement("EllipseHeightEntry");
		App.Tap("EllipseHeightEntry");
		App.ClearText("EllipseHeightEntry");
		App.EnterText("EllipseHeightEntry", "80");

		App.WaitForElement("EllipseWidthEntry");
		App.Tap("EllipseWidthEntry");
		App.ClearText("EllipseWidthEntry");
		App.EnterText("EllipseWidthEntry", "220");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(42)]
	public void RoundRectangle_StrokeColor_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RoundRectangleRadioButton");
		App.Tap("RoundRectangleRadioButton");

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

	[Test, Order(43)]
	public void RoundRectangle_StrokeColor_DashArray_Thickness()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RoundRectangleRadioButton");
		App.Tap("RoundRectangleRadioButton");

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

	[Test, Order(44)]
	public void Line_StrokeLineCap_Flat()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("LineRadioButton");
		App.Tap("LineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "12");

		App.WaitForElement("StrokeLineCapFlatRadioButton");
		App.Tap("StrokeLineCapFlatRadioButton");

		App.WaitForElement("X1Entry");
		App.Tap("X1Entry");
		App.ClearText("X1Entry");
		App.EnterText("X1Entry", "50");

		App.WaitForElement("Y1Entry");
		App.Tap("Y1Entry");
		App.ClearText("Y1Entry");
		App.EnterText("Y1Entry", "100");

		App.WaitForElement("X2Entry");
		App.Tap("X2Entry");
		App.ClearText("X2Entry");
		App.EnterText("X2Entry", "250");

		App.WaitForElement("Y2Entry");
		App.Tap("Y2Entry");
		App.ClearText("Y2Entry");
		App.EnterText("Y2Entry", "100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(45)]
	public void Polyline_StrokeLineCap_Round()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolylineRadioButton");
		App.Tap("PolylineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "12");

		App.WaitForElement("StrokeLineCapRoundRadioButton");
		App.Tap("StrokeLineCapRoundRadioButton");

		App.WaitForElement("PolylinePointsEntry");
		App.Tap("PolylinePointsEntry");
		App.ClearText("PolylinePointsEntry");
		App.EnterText("PolylinePointsEntry", "50,100 100,50 150,100 200,50 250,100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(46)]
	public void Polyline_StrokeLineCap_Square()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolylineRadioButton");
		App.Tap("PolylineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "12");

		App.WaitForElement("StrokeLineCapSquareRadioButton");
		App.Tap("StrokeLineCapSquareRadioButton");

		App.WaitForElement("PolylinePointsEntry");
		App.Tap("PolylinePointsEntry");
		App.ClearText("PolylinePointsEntry");
		App.EnterText("PolylinePointsEntry", "50,100 100,50 150,100 200,50 250,100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(47)]
	public void Path_StrokeLineCap_Round()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "8");

		App.WaitForElement("StrokeLineCapRoundRadioButton");
		App.Tap("StrokeLineCapRoundRadioButton");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,84 C 10,84 40,15 100,55 C 160,15 190,84 190,84 C 190,84 100,135 100,135 C 100,135 10,84 10,84 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(48)]
	public void Path_StrokeLineCap_Square()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "8");

		App.WaitForElement("StrokeLineCapSquareRadioButton");
		App.Tap("StrokeLineCapSquareRadioButton");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,84 C 10,84 40,15 100,55 C 160,15 190,84 190,84 C 190,84 100,135 100,135 C 100,135 10,84 10,84 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(49)]
	public void Polygon_StrokeLineJoin_Round()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolygonRadioButton");
		App.Tap("PolygonRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "20");

		App.WaitForElement("StrokeLineJoinRoundRadioButton");
		App.Tap("StrokeLineJoinRoundRadioButton");

		App.WaitForElement("PolygonPointsEntry");
		App.Tap("PolygonPointsEntry");
		App.ClearText("PolygonPointsEntry");
		App.EnterText("PolygonPointsEntry", "100,20 170,75 100,130 30,75");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(50)]
	public void Polygon_StrokeLineJoin_Bevel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolygonRadioButton");
		App.Tap("PolygonRadioButton");



		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "20");

		App.WaitForElement("StrokeLineJoinBevelRadioButton");
		App.Tap("StrokeLineJoinBevelRadioButton");

		App.WaitForElement("PolygonPointsEntry");
		App.Tap("PolygonPointsEntry");
		App.ClearText("PolygonPointsEntry");
		App.EnterText("PolygonPointsEntry", "100,20 170,75 100,130 30,75");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(51)]
	public void Path_StrokeLineJoin_Round()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "20");

		App.WaitForElement("StrokeLineJoinRoundRadioButton");
		App.Tap("StrokeLineJoinRoundRadioButton");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,84 C 10,84 40,15 100,55 C 160,15 190,84 190,84 C 190,84 100,135 100,135 C 100,135 10,84 10,84 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(52)]
	public void Path_StrokeLineJoin_Bevel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.Tap("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "20");

		App.WaitForElement("StrokeLineJoinBevelRadioButton");
		App.Tap("StrokeLineJoinBevelRadioButton");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,84 C 10,84 40,15 100,55 C 160,15 190,84 190,84 C 190,84 100,135 100,135 C 100,135 10,84 10,84 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(53)]
	public void Polygon_FillRule_Nonzero()
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
		App.EnterText("PolygonPointsEntry", "150,15 60,135 210,58 90,58 240,135");

		App.WaitForElement("FillRuleNonzeroRadioButton");
		App.Tap("FillRuleNonzeroRadioButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

	[Test, Order(54)]
	public void Polyline_FillRule_Nonzero()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PolylineRadioButton");
		App.Tap("PolylineRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("FillRuleNonzeroRadioButton");
		App.Tap("FillRuleNonzeroRadioButton");

		App.WaitForElement("PolylinePointsEntry");
		App.Tap("PolylinePointsEntry");
		App.ClearText("PolylinePointsEntry");
		App.EnterText("PolylinePointsEntry", "50,100 100,50 150,100 200,50 250,100");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}
	[Test, Order(55)]
	public void Path_Aspect_None()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("PathRadioButton");
		App.Tap("PathRadioButton");

		App.WaitForElement("FillColorBlueRadioButton");
		App.Tap("FillColorBlueRadioButton");

		App.WaitForElement("StrokeColorRedRadioButton");
		App.Tap("StrokeColorRedRadioButton");

		App.WaitForElement("AspectNoneRadioButton");
		App.Tap("AspectNoneRadioButton");

		App.WaitForElement("PathDataEntry");
		App.Tap("PathDataEntry");
		App.ClearText("PathDataEntry");
		App.EnterText("PathDataEntry", "M 10,100 C 10,100 40,-20 100,50 C 160,-20 190,100 190,100 C 190,100 100,190 100,190 C 100,190 10,100 10,100 Z");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyShapeScreenshot();
	}

}
