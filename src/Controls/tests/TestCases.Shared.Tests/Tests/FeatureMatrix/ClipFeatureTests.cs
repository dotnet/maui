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
	[Category(UITestCategories.Border)]
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
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.Border)]
	public void Border_ClipWithStrokeColorBlue()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("BlueColorButton");
		App.Tap("BlueColorButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.Border)]
	public void Border_ClipWithStrokeColorGreen()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement("GreenColorButton");
		App.Tap("GreenColorButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(4)]
	[Category(UITestCategories.Border)]
	public void Border_ClipWithStrokeShapeRoundRectangle()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);

		App.WaitForElement("RoundRectangleShapeRadio");
		App.Tap("RoundRectangleShapeRadio");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(5)]
	[Category(UITestCategories.Border)]
	public void Border_ClipWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(ArcSegment);
		App.Tap(ArcSegment);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	// ==================== BoxView Tests ====================

	[Test, Order(6)]
	[Category(UITestCategories.BoxView)]
	public void BoxView_ClipWithColorGreen()
	{
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("BoxViewButton");
		App.Tap("BoxViewButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement("GreenColorButton");
		App.Tap("GreenColorButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(7)]
	[Category(UITestCategories.BoxView)]
	public void BoxView_ClipWithCornerRadius()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("CornerRadiusEntry");
		App.ClearText("CornerRadiusEntry");
		App.EnterText("CornerRadiusEntry", "30");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(8)]
	[Category(UITestCategories.BoxView)]
	public void BoxView_ClipWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	// ==================== Button Tests ====================

	[Test, Order(9)]
	[Category(UITestCategories.Button)]
	public void Button_ClipWithImageSource()
	{
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
		VerifyScreenshot();
	}

	[Test, Order(10)]
	[Category(UITestCategories.Button)]
	public void Button_ClipWithText()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Clipped Button");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(11)]
	[Category(UITestCategories.Button)]
	public void Button_ClipWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	// ==================== Image Tests (Geometry Variations) ====================

	[Test, Order(12)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithRectangleGeometry()
	{
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ImageButton");
		App.Tap("ImageButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(13)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithEllipseGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(14)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithRoundRectangleGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(15)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithGeometryGroup()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithLineSegmentPath()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(LineSegment);
		App.Tap(LineSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(17)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithArcSegmentPath()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(ArcSegment);
		App.Tap(ArcSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(18)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithBezierSegmentPath()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(BezierSegment);
		App.Tap(BezierSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithPolyLineSegmentPath()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(PolyLineSegment);
		App.Tap(PolyLineSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(20)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithPolyBezierSegmentPath()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(PolyBezierSegment);
		App.Tap(PolyBezierSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(21)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithQuadraticBezierSegmentPath()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(QuadraticBezierSegment);
		App.Tap(QuadraticBezierSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(22)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithPolyQuadraticBezierSegmentPath()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(PolyQuadraticBezierSegment);
		App.Tap(PolyQuadraticBezierSegment);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	// ==================== Label Tests ====================

	[Test, Order(23)]
	[Category(UITestCategories.Label)]
	public void Label_ClipWithLongText() // all clip in iOS not working
	{
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("LabelButton");
		App.Tap("LabelButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(24)]
	[Category(UITestCategories.Label)]
	public void Label_ClipWithDifferentFontSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "36");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(25)]
	[Category(UITestCategories.Label)]
	public void Label_ClipWithFormattedText()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	// ==================== ContentView Tests ====================

	[Test, Order(26)]
	[Category(UITestCategories.Layout)]
	public void ContentView_ClipWithRectangleGeometry()
	{
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ContentViewButton");
		App.Tap("ContentViewButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(27)]
	[Category(UITestCategories.Layout)]
	public void ContentView_ClipWithEllipseGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(28)]
	[Category(UITestCategories.Layout)]
	public void ContentView_ClipWithRoundRectangleGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(29)]
	[Category(UITestCategories.Layout)]
	public void ContentView_ClipWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	// ==================== ImageButton Tests ====================

	[Test, Order(30)]
	[Category(UITestCategories.ImageButton)]
	public void ImageButton_ClipWithRectangleGeometry()
	{
		App.TapBackArrow(Device == TestDevice.iOS || Device == TestDevice.Mac ? "ClipControlPage" : "");
		App.WaitForElement("ImageButtonButton");
		App.Tap("ImageButtonButton");

		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(31)]
	[Category(UITestCategories.ImageButton)]
	public void ImageButton_ClipWithEllipseGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(32)]
	[Category(UITestCategories.ImageButton)]
	public void ImageButton_ClipWithRoundRectangleGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(33)]
	[Category(UITestCategories.ImageButton)]
	public void ImageButton_ClipWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);

		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");

		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	// ==================== Negative Tests ====================

	[Test, Order(34)]
	[Category(UITestCategories.Border)]
	public void Border_ClipNone_NoCrash()
	{
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
		VerifyScreenshot();
	}

	[Test, Order(35)]
	[Category(UITestCategories.Image)]
	public void Image_ClipNull_NoCrash()
	{
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
		VerifyScreenshot();
	}

	[Test, Order(36)]
	[Category(UITestCategories.Button)]
	public void Button_ClipNull_NoCrash()
	{
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
		VerifyScreenshot();
	}

	[Test, Order(37)]
	[Category(UITestCategories.Layout)]
	public void ContentView_ClipNull_NoCrash()
	{
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
		VerifyScreenshot();
	}

	// ==================== Clip with Rotation Tests ====================

	[Test, Order(38)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithRotation()
	{
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
		VerifyScreenshot();
	}

	[Test, Order(39)]
	[Category(UITestCategories.Border)]
	public void Border_ClipWithRotation()
	{
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
		VerifyScreenshot();
	}

	[Test, Order(40)]
	[Category(UITestCategories.Label)]
	public void Label_ClipWithRotation()
	{
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
		VerifyScreenshot();
	}

	// ==================== Clip with Scale Tests ====================

	[Test, Order(41)]
	[Category(UITestCategories.Button)]
	public void Button_ClipWithScale()
	{
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
		VerifyScreenshot();
	}

	[Test, Order(42)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithScale()
	{
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
		VerifyScreenshot();
	}

	[Test, Order(43)]
	[Category(UITestCategories.ImageButton)]
	public void ImageButton_ClipWithScale()
	{
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
		VerifyScreenshot();
	}

	// ==================== Nested Clip Tests ====================

	[Test, Order(44)]
	[Category(UITestCategories.Layout)]
	public void ContentView_ClipWithNestedClippedContent()
	{
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
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(45)]
	[Category(UITestCategories.Border)]
	public void Border_ClipWithNestedContent()
	{
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
		VerifyScreenshot();
	}
#endif

	// ==================== Complex Geometry Tests ====================

#if TEST_FAILS_ON_WINDOWS // Issue: https://github.com/dotnet/maui/issues/30778

	[Test, Order(46)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithComplexPolyLineGeometry()
	{
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
		VerifyScreenshot();
	}
#endif

	[Test, Order(47)]
	[Category(UITestCategories.Image)]
	public void Image_ClipWithComplexPolyBezierAndRotation()
	{
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
		VerifyScreenshot();
	}

	// ==================== Clip with Combined Transforms ====================

	[Test, Order(48)]
	[Category(UITestCategories.Border)]
	public void Border_ClipWithRotationAndScale()
	{
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
		VerifyScreenshot();
	}

	[Test, Order(49)]
	[Category(UITestCategories.BoxView)]
	public void BoxView_ClipWithRotation()
	{
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
		VerifyScreenshot();
	}
}