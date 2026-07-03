using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Border)]
public class BorderFeatureTests : _GalleryUITest
{
	public const string BorderFeatureMatrix = "Border Feature Matrix";

	public override string GalleryPageName => BorderFeatureMatrix;

	public BorderFeatureTests(TestDevice device)
		: base(device)
	{
	}

	// Navigates to the Options page; also resets the ViewModel to clean defaults on each call.
	private void NavigateToOptions()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
	}

	// On Windows, crop the top portion to exclude platform chrome before comparing screenshots.
	private void VerifyBorderScreenshot()
	{
#if WINDOWS
		VerifyScreenshot(cropTop: 100);
#else
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#endif
	}

	// ── Non-DashArray tests (Order 1–9) ──

	[Test]
	[Order(1)]
	public void Border_DefaultValues()
	{
		// Verify the initial Border appearance without applying any options.
		// Defaults: Stroke=Red, StrokeThickness=1, StrokeShape=Rectangle, Padding=5, Content=Label.
		App.WaitForElement("Options");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(2)]
	public void Border_PaddingWithContent_Label()
	{
		NavigateToOptions();

		App.WaitForElement("PaddingEntry");
		App.EnterText("PaddingEntry", "10,20,60,10");

		App.WaitForElement("LabelRadioButton");
		App.Tap("LabelRadioButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(3)]
	public void Border_PaddingWithContent_Image()
	{
		NavigateToOptions();

		App.WaitForElement("PaddingEntry");
		App.EnterText("PaddingEntry", "10,20,60,10");

		App.WaitForElement("ImageRadioButton");
		App.Tap("ImageRadioButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(4)]
	public void Border_StrokeColorWithContent_Button()
	{
		NavigateToOptions();

		App.WaitForElement("BlueColorButton");
		App.Tap("BlueColorButton");

		App.WaitForElement("ButtonRadioButton");
		App.Tap("ButtonRadioButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(5)]
	public void Border_StrokeColorWithStrokeShape_RoundRectangle()
	{
		NavigateToOptions();

		App.WaitForElement("BlueColorButton");
		App.Tap("BlueColorButton");

		App.WaitForElement("RoundRectangleShapeRadio");
		App.Tap("RoundRectangleShapeRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(6)]
	public void Border_StrokeColorWithStrokeThickness()
	{
		NavigateToOptions();

		App.WaitForElement("GreenColorButton");
		App.Tap("GreenColorButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(7)]
	public void Border_StrokeShapeWithStrokeThickness_Ellipse()
	{
		NavigateToOptions();

		App.WaitForElement("EllipseShapeRadio");
		App.Tap("EllipseShapeRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(8)]
	public void Border_StrokeShape_Path()
	{
		// Path shape without a DashArray so this test runs on all platforms.
		// (The DashArray+Path combination is covered separately but is iOS/Catalyst-guarded.)
		NavigateToOptions();

		App.WaitForElement("PathShapeRadio");
		App.Tap("PathShapeRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(9)]
	public void Border_Shadow()
	{
		NavigateToOptions();

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

		VerifyBorderScreenshot();
	}

	// ── Tests that fail on iOS/Catalyst — excluded via preprocessor (Order 10–14) ──

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // For more information, see : https://github.com/dotnet/maui/issues/29743

	[Test]
	[Order(10)]
	public void Border_StrokeMiterLimitWithStrokeLineJoin_Miter()
	{
		NavigateToOptions();

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "20");

		App.WaitForElement("MiterLimitEntry");
		App.EnterText("MiterLimitEntry", "1");

		App.WaitForElement("MiterLineJoinRadio");
		App.Tap("MiterLineJoinRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(11)]
	public void Border_StrokeShapeWithStrokeLineJoin_Bevel()
	{
		NavigateToOptions();

		App.WaitForElement("PolygonShapeRadio");
		App.Tap("PolygonShapeRadio");

		App.WaitForElement("BevelLineJoinRadio");
		App.Tap("BevelLineJoinRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(12)]
	public void Border_StrokeColorWithStrokeLineJoin_Round()
	{
		NavigateToOptions();

		App.WaitForElement("RoundLineJoinRadio");
		App.Tap("RoundLineJoinRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(13)]
	public void Border_StrokeThicknessWithStrokeLineJoin_Bevel()
	{
		NavigateToOptions();

		App.WaitForElement("BevelLineJoinRadio");
		App.Tap("BevelLineJoinRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "15");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(14)]
	public void Border_PolygonShapeWithStrokeLineJoin_Bevel()
	{
		NavigateToOptions();

		App.WaitForElement("PolygonShapeRadio");
		App.Tap("PolygonShapeRadio");

		App.WaitForElement("BevelLineJoinRadio");
		App.Tap("BevelLineJoinRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "15");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#endif

	[Test]
	[Order(15)]
	public void Border_StrokeShapeWithPolygon()
	{
		NavigateToOptions();

		App.WaitForElement("PolygonShapeRadio");
		App.Tap("PolygonShapeRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(16)]
	public void Border_StrokeColorWithRed()
	{
		NavigateToOptions();

		App.WaitForElement("RedColorButton");
		App.Tap("RedColorButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// Explicitly switch to another shape, then back to Rectangle to verify reset behavior.
	[Test]
	[Order(17)]
	public void Border_StrokeShapeRectangle_AfterChange()
	{
		NavigateToOptions();

		// First switch to Ellipse
		App.WaitForElement("EllipseShapeRadio");
		App.Tap("EllipseShapeRadio");

		// Then switch back to Rectangle
		App.WaitForElement("RectangleShapeRadio");
		App.Tap("RectangleShapeRadio");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(18)]
	public void Border_ShadowWithColor()
	{
		NavigateToOptions();

		App.WaitForElement("ShadowRedColorButton");
		App.Tap("ShadowRedColorButton");

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

		VerifyBorderScreenshot();
	}

	[Test]
	[Order(19)]
	public void Border_BackgroundColor()
	{
		NavigateToOptions();

		App.WaitForElement("BackgroundYellowButton");
		App.Tap("BackgroundYellowButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(20)]
	public void Border_StrokeGradientBrush()
	{
		NavigateToOptions();

		App.WaitForElement("GradientStrokeButton");
		App.Tap("GradientStrokeButton");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(21)]
	public void Border_ZeroPadding()
	{
		NavigateToOptions();

		App.WaitForElement("PaddingEntry");
		App.EnterText("PaddingEntry", "0,0,0,0");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ── DashArray tests (Order 15–18) ──

	[Test]
	[Order(22)]
	public void Border_StrokeShapeWithDashArray_Path()
	{
		NavigateToOptions();

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("PathShapeRadio");
		App.Tap("PathShapeRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(23)]
	public void Border_StrokeThicknessWithDashArray()
	{
		NavigateToOptions();

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyBorderScreenshot();
	}

	[Test]
	[Order(24)]
	public void Border_StrokeDashArrayWithDashOffset()
	{
		NavigateToOptions();

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("DashOffsetEntry");
		App.EnterText("DashOffsetEntry", "2");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyBorderScreenshot();
	}

	[Test]
	[Order(25)]
	public void Border_StrokeDashArrayWithStrokeColor()
	{
		NavigateToOptions();

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("BlackColorButton");
		App.Tap("BlackColorButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ── DashArray + StrokeLineCap tests disabled for Windows (Order 19–24) ──

#if TEST_FAILS_ON_WINDOWS // For more information, see : https://github.com/dotnet/maui/issues/29741
	[Test]
	[Order(26)]
	public void Border_StrokeDashArrayWithStrokeLineCap_Flat()
	{
		NavigateToOptions();

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("FlatLineCapRadio");
		App.Tap("FlatLineCapRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(27)]
	public void Border_StrokeDashArrayWithStrokeLineCap_Round()
	{
		NavigateToOptions();

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("RoundLineCapRadio");
		App.Tap("RoundLineCapRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(28)]
	public void Border_StrokeDashArrayWithDashOffsetAndStrokeLineCapRound()
	{
		NavigateToOptions();

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("DashOffsetEntry");
		App.EnterText("DashOffsetEntry", "2");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("RoundLineCapRadio");
		App.Tap("RoundLineCapRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(29)]
	public void Border_StrokeDashArrayWithStrokeLineCap_Square()
	{
		NavigateToOptions();

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement("SquareLineCapRadio");
		App.Tap("SquareLineCapRadio");

		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(30)]
	public void Border_StrokeDashArrayWithEllipseShapeAndStrokeLineCap_Square()
	{
		NavigateToOptions();

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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(31)]
	public void Border_PolygonShapeWithStrokeLineCap_Round()
	{
		NavigateToOptions();

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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	// ── DashArray+Offset (Order 25) — runs on all platforms; issue #29661 (DashArray on iOS/Catalyst) is closed/fixed ──

	[Test]
	[Order(32)]
	public void Border_StrokeColorWithDashArrayAndOffset()
	{
		NavigateToOptions();

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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // For more information, see: https://github.com/dotnet/maui/issues/29898
	[Test]
	[Order(33)]
	public void Border_StrokeDashArray_Reset()
	{
		NavigateToOptions();

		App.WaitForElement("StrokeDashArrayEntry");
		App.EnterText("StrokeDashArrayEntry", "5,3");

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "15"); // Increased thickness to make dashes more visible.

		App.WaitForElement("Apply");
		App.Tap("Apply");

		NavigateToOptions();

		App.WaitForElement("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "5"); // Change thickness to ensure the reset takes effect.

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

}