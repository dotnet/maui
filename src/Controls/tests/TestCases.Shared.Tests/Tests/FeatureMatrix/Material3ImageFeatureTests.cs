#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3ImageFeatureTests : _GalleryUITest
{
	public const string ImageFeatureMatrix = "Image Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string ImageAspectFit = "ImageAspectFit";
	public const string ImageAspectFill = "ImageAspectFill";
	public const string ImageFill = "ImageFill";
	public const string ImageCenter = "ImageCenter";
	public const string SourceTypeFile = "SourceTypeFile";
	public const string SourceTypeFontImage = "SourceTypeFontImage";
	public const string SourceTypeStream = "SourceTypeStream";
	public const string SourceTypeUri = "SourceTypeUri";
	public const string IsVisibleFalseRadio = "IsVisibleFalseRadio";
	public const string FlowDirectionRTL = "FlowDirectionRTL";
	public const string ShadowCheckBox = "ShadowCheckBox";
	public const string IsAnimationTrue = "IsAnimationTrue";
	public const string IsAnimationFalse = "IsAnimationFalse";

	public override string GalleryPageName => ImageFeatureMatrix;

	public Material3ImageFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_AspectFitWithFileSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFit);
		App.Tap(ImageAspectFit);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_AspectFitWithUriSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFit);
		App.Tap(ImageAspectFit);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl", timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_AspectFitWithStreamSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFit);
		App.Tap(ImageAspectFit);
		App.WaitForElement(SourceTypeStream);
		App.Tap(SourceTypeStream);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_AspectFitWithFontImageSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFit);
		App.Tap(ImageAspectFit);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_AspectFillWithFileSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFill);
		App.Tap(ImageAspectFill);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_AspectFillWithUriSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFill);
		App.Tap(ImageAspectFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl", timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_AspectFillWithStreamSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFill);
		App.Tap(ImageAspectFill);
		App.WaitForElement(SourceTypeStream);
		App.Tap(SourceTypeStream);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_AspectFillWithFontImageSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFill);
		App.Tap(ImageAspectFill);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_FillWithFileSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_FillWithUriSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl", timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_FillWithStreamSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeStream);
		App.Tap(SourceTypeStream);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_FillWithFontImageSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_CenterWithFileSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageCenter);
		App.Tap(ImageCenter);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_CenterWithUriSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageCenter);
		App.Tap(ImageCenter);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl", timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_CenterWithStreamSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageCenter);
		App.Tap(ImageCenter);
		App.WaitForElement(SourceTypeStream);
		App.Tap(SourceTypeStream);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageAspect_CenterWithFontImageSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageCenter);
		App.Tap(ImageCenter);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3FontImageWithFontColorGreen()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement("FontColorGreen");
		App.Tap("FontColorGreen");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3FontImageWithFontSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement("EntryFontSize");
		App.ClearText("EntryFontSize");
		App.Tap("EntryFontSize");
		App.EnterText("EntryFontSize", "100");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(ShadowCheckBox);
		App.Tap(ShadowCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3ImageFlowDirectionRTL()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
