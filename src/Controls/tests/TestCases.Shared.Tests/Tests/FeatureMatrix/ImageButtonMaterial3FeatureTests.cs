#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class ImageButtonMaterial3FeatureTests : _GalleryUITest
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

    public override string GalleryPageName => ImageButtonFeatureMatrix;

    public ImageButtonMaterial3FeatureTests(TestDevice device)
    : base(device)
    {
    }

    [Test, Order(1)]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_AspectFitWithImageSourceFromFile()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_AspectFitWithImageSourceFromUri()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/30576
    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_AspectFitWithImageSourceFromStream()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_FillWithImageSourceFromStream()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
#endif

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_AspectFitWithImageSourceFromFontImage()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/29956
    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_AspectFillWithImageSourceFromUri()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
#endif

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_FillWithImageSourceFromFile()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_FillWithImageSourceFromUri()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_CenterWithImageSourceFromFile()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }


#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/29959

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/30576
    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_AspectFillWithImageSourceFromStream()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_CenterWithImageSourceFromStream()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
#endif

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_AspectFillWithImageSourceFromFontImage()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_FillWithImageSourceFromFontImage()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_CenterWithImageSourceFromFontImage()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/29956
    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_AspectFillWithImageSourceFromFile()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonAspect_CenterWithImageSourceFromUri()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
#endif
#endif

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonFlowDirectionRTL()
    {
        App.WaitForElement(Options);
        App.Tap(Options);
        App.WaitForElement(FlowDirectionRTL);
        App.Tap(FlowDirectionRTL);
        App.WaitForElement(Apply);
        App.Tap(Apply);
        App.WaitForElement("ImageButtonControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonWithPadding()
    {
        App.WaitForElement(Options);
        App.Tap(Options);
        App.WaitForElement(PaddingEntry);
        App.ClearText(PaddingEntry);
        App.EnterText(PaddingEntry, "40,40,40,40");
        App.WaitForElement(Apply);
        App.Tap(Apply);
        App.WaitForElement("ImageButtonControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonWithCornerRadius()
    {
        App.WaitForElement(Options);
        App.Tap(Options);
        App.WaitForElement(CornerRadiusEntry);
        App.ClearText(CornerRadiusEntry);
        App.EnterText(CornerRadiusEntry, "50,50,50,50");
        App.WaitForElement(Apply);
        App.Tap(Apply);
        App.WaitForElement("ImageButtonControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonWithBorderColor()
    {
        App.WaitForElement(Options);
        App.Tap(Options);
        App.WaitForElement(BorderGreen);
        App.Tap(BorderGreen);
        App.WaitForElement(Apply);
        App.Tap(Apply);
        App.WaitForElement("ImageButtonControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonWithBorderWidth()
    {
        App.WaitForElement(Options);
        App.Tap(Options);
        App.WaitForElement(BorderWidthEntry);
        App.ClearText(BorderWidthEntry);
        App.EnterText(BorderWidthEntry, "10");
        App.WaitForElement(Apply);
        App.Tap(Apply);
        App.WaitForElement("ImageButtonControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonWithBorderColorAndWidth()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void VerifyMaterial3ImageButtonWithBorderWidthAndCornerRadius()
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
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
}
#endif