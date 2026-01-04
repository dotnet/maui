using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class ClipFeatureTests : UITest
{
	public const string ImageFeatureMatrix = "Clip Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string ImageAspectFit = "ImageAspectFit";
	public const string ImageAspectFill = "ImageAspectFill";
	public const string ImageFill = "ImageFill";
	public const string SourceTypeFile = "SourceTypeFile";
	public const string SourceTypeFontImage = "SourceTypeFontImage";
	public const string SourceTypeStream = "SourceTypeStream";
	public const string SourceTypeUri = "SourceTypeUri";
	public const string SourceTypeClipImage = "SourceTypeClipImage";
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

	public ClipFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ImageFeatureMatrix);
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_EllipseGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_RectangleGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_RoundedRectangleGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_GeometryGroup()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_PathGeometry_LineSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(LineSegment);
		App.Tap(LineSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_PathGeometry_ArcSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(ArcSegment);
		App.Tap(ArcSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_PathGeometry_BezierSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(BezierSegment);
		App.Tap(BezierSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_PathGeometry_PolyLineSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(PolyLineSegment);
		App.Tap(PolyLineSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_PathGeometry_PolyBezierSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(PolyBezierSegment);
		App.Tap(PolyBezierSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_PathGeometry_QuadraticBezierSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(QuadraticBezierSegment);
		App.Tap(QuadraticBezierSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithClip_PathGeometry_PolyQuadraticBezierSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeClipImage);
		App.Tap(SourceTypeClipImage);
		App.WaitForElement(PolyQuadraticBezierSegment);
		App.Tap(PolyQuadraticBezierSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_EllipseGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(EllipseGeometry);
		App.Tap(EllipseGeometry);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_RectangleGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(RectangleGeometry);
		App.Tap(RectangleGeometry);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_RoundedRectangleGeometry()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(RoundRectangleGeometry);
		App.Tap(RoundRectangleGeometry);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_GeometryGroup()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(GeometryGroup);
		App.Tap(GeometryGroup);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_PathGeometry_LineSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(LineSegment);
		App.Tap(LineSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_PathGeometry_ArcSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(ArcSegment);
		App.Tap(ArcSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_PathGeometry_BezierSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(BezierSegment);
		App.Tap(BezierSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_PathGeometry_PolyLineSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(PolyLineSegment);
		App.Tap(PolyLineSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_PathGeometry_PolyBezierSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(PolyBezierSegment);
		App.Tap(PolyBezierSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_PathGeometry_QuadraticBezierSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(QuadraticBezierSegment);
		App.Tap(QuadraticBezierSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillFromUriWithClip_PathGeometry_PolyQuadraticBezierSegment()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(PolyQuadraticBezierSegment);
		App.Tap(PolyQuadraticBezierSegment);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}
}