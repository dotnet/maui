using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class ClipFeatureTests : _GalleryUITest
{
	public const string ClipFeatureMatrix = "Clip Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string RectangleGeometry = "RectangleGeometry";
	public const string EllipseGeometry = "EllipseGeometry";
	public const string RoundRectangleGeometry = "RoundRectangleGeometry";
	public const string GeometryGroup = "GeometryGroup";
	public const string LineSegment = "LineSegment";
	public const string ArcSegment = "ArcSegment";
	public const string BezierSegment = "BezierSegment";
	public const string PolyLineSegment = "PolyLineSegment";
	public const string PolyBezierSegment = "PolyBezierSegment";
	public const string QuadraticBezierSegment = "QuadraticBezierSegment";
	public const string PolyQuadraticBezierSegment = "PolyQuadraticBezierSegment";

	public override string GalleryPageName => ClipFeatureMatrix;

	public ClipFeatureTests(TestDevice device)
		: base(device)
	{
	}

	// ==================== Border Tests ====================

	[Test, Order(1)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Border_ClipWithStrokeThickness()
	{
		App.WaitForElement("BorderButton");
		App.Tap("BorderButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement("StrokeThicknessEntry");
		App.ClearText("StrokeThicknessEntry");
		App.EnterText("StrokeThicknessEntry", "10");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Border_ClipWithStrokeColorBlue()
	{
		TapButtonIfOnClipControlPage("BorderButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("BlueColorButton");
		App.Tap("BlueColorButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Border_ClipWithStrokeColorGreen()
	{
		TapButtonIfOnClipControlPage("BorderButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement("BorderGreenColorButton");
		App.Tap("BorderGreenColorButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Border_ClipWithStrokeShapeRoundRectangle()
	{
		TapButtonIfOnClipControlPage("BorderButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);

		App.WaitForElement("RoundRectangleShapeRadio");
		App.Tap("RoundRectangleShapeRadio");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(5)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Border_ClipWithShadow()
	{
		TapButtonIfOnClipControlPage("BorderButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(ArcSegment);
		App.Tap(ArcSegment);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	// ==================== BoxView Tests ====================

	[Test, Order(6)]
	[Category(UITestCategories.ViewBaseTests)]
	public void BoxView_ClipWithColorGreen()
	{
		TapButtonIfOnClipControlPage("BoxViewButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("BoxViewButton");
		App.Tap("BoxViewButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement("BoxViewGreenColorButton");
		App.Tap("BoxViewGreenColorButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	[Category(UITestCategories.ViewBaseTests)]
	public void BoxView_ClipWithCornerRadius()
	{
		TapButtonIfOnClipControlPage("BoxViewButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("CornerRadiusEntry");
		App.ClearText("CornerRadiusEntry");
		App.EnterText("CornerRadiusEntry", "30");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(8)]
	[Category(UITestCategories.ViewBaseTests)]
	public void BoxView_ClipWithShadow()
	{
		TapButtonIfOnClipControlPage("BoxViewButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	// ==================== Button Tests ====================

	[Test, Order(9)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Button_ClipWithImageSource()
	{
		TapButtonIfOnClipControlPage("ButtonId");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ButtonId");
		App.Tap("ButtonId");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement("ImageSource");
		App.Tap("ImageSource");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Button_ClipWithText()
	{
		TapButtonIfOnClipControlPage("ButtonId");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Clipped Button");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(11)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Button_ClipWithShadow()
	{
		TapButtonIfOnClipControlPage("ButtonId");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	// ==================== Image Tests (Geometry Variations) ====================

	[Test, Order(12)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithRectangleGeometry()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ImageButton");
		App.Tap("ImageButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithEllipseGeometry()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithRoundRectangleGeometry()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(15)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithGeometryGroup()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(16)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithLineSegmentPath()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(LineSegment);
		App.Tap(LineSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(17)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithArcSegmentPath()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(ArcSegment);
		App.Tap(ArcSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(18)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithBezierSegmentPath()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(BezierSegment);
		App.Tap(BezierSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(19)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithPolyLineSegmentPath()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(PolyLineSegment);
		App.Tap(PolyLineSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(20)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithPolyBezierSegmentPath()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(PolyBezierSegment);
		App.Tap(PolyBezierSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(21)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithQuadraticBezierSegmentPath()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(QuadraticBezierSegment);
		App.Tap(QuadraticBezierSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(22)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithPolyQuadraticBezierSegmentPath()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(PolyQuadraticBezierSegment);
		App.Tap(PolyQuadraticBezierSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ==================== Label Tests ====================

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/34114

	[Test, Order(23)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Label_ClipWithLongText()
	{
		TapButtonIfOnClipControlPage("LabelButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("LabelButton");
		App.Tap("LabelButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(24)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Label_ClipWithDifferentFontSize()
	{
		TapButtonIfOnClipControlPage("LabelButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "36");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(25)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Label_ClipWithFormattedText()
	{
		TapButtonIfOnClipControlPage("LabelButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	// ==================== ContentView Tests ====================

	[Test, Order(26)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ContentView_ClipWithRectangleGeometry()
	{
		TapButtonIfOnClipControlPage("ContentViewButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ContentViewButton");
		App.Tap("ContentViewButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(27)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ContentView_ClipWithEllipseGeometry()
	{
		TapButtonIfOnClipControlPage("ContentViewButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(28)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ContentView_ClipWithRoundRectangleGeometry()
	{
		TapButtonIfOnClipControlPage("ContentViewButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(29)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ContentView_ClipWithShadow()
	{
		TapButtonIfOnClipControlPage("ContentViewButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	// ==================== ImageButton Tests ====================

	[Test, Order(30)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ImageButton_ClipWithRectangleGeometry()
	{
		TapButtonIfOnClipControlPage("ImageButtonButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ImageButtonButton");
		App.Tap("ImageButtonButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(31)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ImageButton_ClipWithEllipseGeometry()
	{
		TapButtonIfOnClipControlPage("ImageButtonButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(32)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ImageButton_ClipWithRoundRectangleGeometry()
	{
		TapButtonIfOnClipControlPage("ImageButtonButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(33)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ImageButton_ClipWithShadow()
	{
		TapButtonIfOnClipControlPage("ImageButtonButton");
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	// ==================== Negative Tests ====================

	[Test, Order(34)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Border_ClipNull_NoCrash()
	{
		TapButtonIfOnClipControlPage("BorderButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("BorderButton");
		App.Tap("BorderButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);

		// Now clear the clip by setting it to null
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement("ClearClipButton");
		App.Tap("ClearClipButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(35)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipNull_NoCrash()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ImageButton");
		App.Tap("ImageButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);

		// Now clear the clip by setting it to null
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement("ClearClipButton");
		App.Tap("ClearClipButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(36)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Button_ClipNull_NoCrash()
	{
		TapButtonIfOnClipControlPage("ButtonId");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ButtonId");
		App.Tap("ButtonId");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);

		// Now clear the clip by setting it to null
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement("ClearClipButton");
		App.Tap("ClearClipButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(37)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ContentView_ClipNull_NoCrash()
	{
		TapButtonIfOnClipControlPage("ContentViewButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ContentViewButton");
		App.Tap("ContentViewButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);

		App.WaitForElement(Apply);
		App.Tap(Apply);

		// Now clear the clip by setting it to null
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement("ClearClipButton");
		App.Tap("ClearClipButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ==================== Clip with Rotation Tests ====================

	[Test, Order(38)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithRotation()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ImageButton");
		App.Tap("ImageButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "45");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(39)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Border_ClipWithRotation()
	{
		TapButtonIfOnClipControlPage("BorderButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("BorderButton");
		App.Tap("BorderButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "30");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/34114

	[Test, Order(40)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Label_ClipWithRotation()
	{
		TapButtonIfOnClipControlPage("LabelButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("LabelButton");
		App.Tap("LabelButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "90");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	// ==================== Clip with Scale Tests ====================

	[Test, Order(41)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Button_ClipWithScale()
	{
		TapButtonIfOnClipControlPage("ButtonId");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ButtonId");
		App.Tap("ButtonId");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement("ScaleXEntry");
		App.ClearText("ScaleXEntry");
		App.EnterText("ScaleXEntry", "0.5");

		App.WaitForElement("ScaleYEntry");
		App.ClearText("ScaleYEntry");
		App.EnterText("ScaleYEntry", "0.5");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(42)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithScale()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ImageButton");
		App.Tap("ImageButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement("ScaleXEntry");
		App.ClearText("ScaleXEntry");
		App.EnterText("ScaleXEntry", "1.5");

		App.WaitForElement("ScaleYEntry");
		App.ClearText("ScaleYEntry");
		App.EnterText("ScaleYEntry", "0.75");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(43)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ImageButton_ClipWithScale()
	{
		TapButtonIfOnClipControlPage("ImageButtonButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ImageButtonButton");
		App.Tap("ImageButtonButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("ScaleXEntry");
		App.ClearText("ScaleXEntry");
		App.EnterText("ScaleXEntry", "0.8");

		App.WaitForElement("ScaleYEntry");
		App.ClearText("ScaleYEntry");
		App.EnterText("ScaleYEntry", "0.8");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ==================== Nested Clip Tests ====================

	[Test, Order(44)]
	[Category(UITestCategories.ViewBaseTests)]
	public void ContentView_ClipWithNestedClippedContent()
	{
		TapButtonIfOnClipControlPage("ContentViewButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ContentViewButton");
		App.Tap("ContentViewButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		// Apply an ellipse clip to the ContentView (which contains child elements)
		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(45)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Border_ClipWithNestedContent()
	{
		TapButtonIfOnClipControlPage("BorderButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("BorderButton");
		App.Tap("BorderButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		// Apply a geometry group clip to Border (which has child Label)
		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	// ==================== Complex Geometry Tests ====================

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(46)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithComplexPolyLineGeometry()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ImageButton");
		App.Tap("ImageButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		// PolyLine creates a complex star shape - tests performance with many points
		App.WaitForElement(PolyLineSegment);
		App.Tap(PolyLineSegment);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(47)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Image_ClipWithComplexPolyBezierAndRotation()
	{
		TapButtonIfOnClipControlPage("ImageButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ImageButton");
		App.Tap("ImageButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		// Combine complex geometry with transform for stress test
		App.WaitForElement(PolyBezierSegment);
		App.Tap(PolyBezierSegment);

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "15");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ==================== Clip with Combined Transforms ====================

	[Test, Order(48)]
	[Category(UITestCategories.ViewBaseTests)]
	public void Border_ClipWithRotationAndScale()
	{
		TapButtonIfOnClipControlPage("BorderButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("BorderButton");
		App.Tap("BorderButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "25");

		App.WaitForElement("ScaleXEntry");
		App.ClearText("ScaleXEntry");
		App.EnterText("ScaleXEntry", "0.7");

		App.WaitForElement("ScaleYEntry");
		App.ClearText("ScaleYEntry");
		App.EnterText("ScaleYEntry", "0.7");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(49)]
	[Category(UITestCategories.ViewBaseTests)]
	public void BoxView_ClipWithRotation()
	{
		TapButtonIfOnClipControlPage("BoxViewButton");
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("BoxViewButton");
		App.Tap("BoxViewButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "60");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	public void TapButtonIfOnClipControlPage(string buttonAutomationId)
	{
		var button = App.FindElement(buttonAutomationId);
		if (button != null)
		{
			App.WaitForElement(buttonAutomationId);
			App.Tap(buttonAutomationId);
		}
	}
}