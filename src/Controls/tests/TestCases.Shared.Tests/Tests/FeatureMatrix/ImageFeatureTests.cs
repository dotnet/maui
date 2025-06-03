using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class ImageFeatureTests : UITest
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


	public ImageFeatureTests(TestDevice device)
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
	public void VerifyImageAspect_AspectFitWithImageSourceFromFile()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_AspectFitWithImageSourceFromUri()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFit);
		App.Tap(ImageAspectFit);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_AspectFitWithImageSourceFromStream()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_AspectFitWithImageSourceFromFontImage()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_AspectFillWithImageSourceFromFile()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_AspectFillWithImageSourceFromUri()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFill);
		App.Tap(ImageAspectFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_AspectFillWithImageSourceFromStream()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_AspectFillWithImageSourceFromFontImage()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithImageSourceFromFile()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithImageSourceFromUri()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithImageSourceFromStream()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_FillWithImageSourceFromFontImage()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_CenterWithImageSourceFromFile()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_CenterWithImageSourceFromUri()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageCenter);
		App.Tap(ImageCenter);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_CenterWithImageSourceFromStream()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyImageAspect_CenterWithImageSourceFromFontImage()
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyFontImageWithFontColorRed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement("FontColorRed");
		App.Tap("FontColorRed");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyFontImageWithFontColorGreen()
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
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/22210
	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyFontImageWithFontSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement("EntryFontSize");
		App.ClearText("EntryFontSize");
		App.EnterText("EntryFontSize", "50");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageControl");
		VerifyScreenshot();
	}
#endif
}