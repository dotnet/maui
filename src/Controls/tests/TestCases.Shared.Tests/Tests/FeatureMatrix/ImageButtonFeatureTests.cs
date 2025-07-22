using Microsoft.Maui.Controls.Shapes;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class ImageButtonFeatureTests : UITest
{
	public const string ImageButtonFeatureMatrix = "ImageButton Feature Matrix";
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
	public const string FlowDirectionRTL = "FlowDirectionRTL";
	public const string ShadowTrue = "ShadowTrue";
	public const string BorderGreen = "BorderGreen";
	public const string BorderWidthEntry = "BorderWidthEntry";
	public const string CornerRadiusEntry = "CornerRadiusEntry";
	public const string IsVisibleFalse = "IsVisibleFalse";
	public const string IsEnabledFalse = "IsEnabledFalse";
	public const string PaddingEntry = "PaddingEntry";


	public ImageButtonFeatureTests(TestDevice device)
	: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ImageButtonFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_AspectFitWithImageSourceFromFile()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFit);
		App.Tap(ImageAspectFit);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_AspectFitWithImageSourceFromUri()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFit);
		App.Tap(ImageAspectFit);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl", timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot();
	}
#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/30576
	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_AspectFitWithImageSourceFromStream()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFit);
		App.Tap(ImageAspectFit);
		App.WaitForElement(SourceTypeStream);
		App.Tap(SourceTypeStream);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_FillWithImageSourceFromStream()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeStream);
		App.Tap(SourceTypeStream);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}
#endif
	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_AspectFitWithImageSourceFromFontImage()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFit);
		App.Tap(ImageAspectFit);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/29956
	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_AspectFillWithImageSourceFromUri()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFill);
		App.Tap(ImageAspectFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl", timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_FillWithImageSourceFromFile()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_FillWithImageSourceFromUri()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl", timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_CenterWithImageSourceFromFile()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageCenter);
		App.Tap(ImageCenter);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}


#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/29959

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/30576
	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_AspectFillWithImageSourceFromStream()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFill);
		App.Tap(ImageAspectFill);
		App.WaitForElement(SourceTypeStream);
		App.Tap(SourceTypeStream);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_CenterWithImageSourceFromStream()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageCenter);
		App.Tap(ImageCenter);
		App.WaitForElement(SourceTypeStream);
		App.Tap(SourceTypeStream);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_AspectFillWithImageSourceFromFontImage()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFill);
		App.Tap(ImageAspectFill);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_FillWithImageSourceFromFontImage()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageFill);
		App.Tap(ImageFill);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_CenterWithImageSourceFromFontImage()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageCenter);
		App.Tap(ImageCenter);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/29956
    [Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_AspectFillWithImageSourceFromFile()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageAspectFill);
		App.Tap(ImageAspectFill);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}
	
	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonAspect_CenterWithImageSourceFromUri()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ImageCenter);
		App.Tap(ImageCenter);
		App.WaitForElement(SourceTypeUri);
		App.Tap(SourceTypeUri);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl", timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot();
	}
#endif
#endif

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithIsVisibleFalse()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsVisibleFalse);
		App.Tap(IsVisibleFalse);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForNoElement("ImageButtonControl");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithIsEnabledFalse()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsEnabledFalse);
		App.Tap(IsEnabledFalse);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		App.Tap("ImageButtonControl");
		App.WaitForNoElement("ImageButton Clicked: 1");
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/29812
	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ShadowTrue);
		App.Tap(ShadowTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonFlowDirectionRTL()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithClickedEvent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		App.Tap("ImageButtonControl");
		App.WaitForElement("ImageButton Clicked: 1");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithPressedEvent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		App.Tap("ImageButtonControl");
		App.WaitForElement("ImageButton Pressed: 1");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithReleasedEvent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		App.Tap("ImageButtonControl");
		App.WaitForElement("ImageButton Released: 1");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithCommandParameter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		App.Tap("ImageButtonControl");
		App.WaitForElement("ImageButton: CommandParameter");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithPadding()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(PaddingEntry);
		App.ClearText(PaddingEntry);
		App.EnterText(PaddingEntry, "40,40,40,40");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithCornerRadius()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(CornerRadiusEntry);
		App.ClearText(CornerRadiusEntry);
		App.EnterText(CornerRadiusEntry, "50,50,50,50");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithBorderColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(BorderGreen);
		App.Tap(BorderGreen);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithBorderWidth()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(BorderWidthEntry);
		App.ClearText(BorderWidthEntry);
		App.EnterText(BorderWidthEntry, "10");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithBorderColorAndWidth()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(BorderGreen);
		App.Tap(BorderGreen);
		App.WaitForElement(BorderWidthEntry);
		App.ClearText(BorderWidthEntry);
		App.EnterText(BorderWidthEntry, "15");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithBorderWidthAndCornerRadius()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(BorderWidthEntry);
		App.ClearText(BorderWidthEntry);
		App.EnterText(BorderWidthEntry, "10");
		App.WaitForElement(CornerRadiusEntry);
		App.ClearText(CornerRadiusEntry);
		App.EnterText(CornerRadiusEntry, "30,30,30,30");
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot();
	}
}