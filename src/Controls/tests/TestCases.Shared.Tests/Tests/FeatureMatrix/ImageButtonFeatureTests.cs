using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class ImageButtonFeatureTests : _GalleryUITest
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
	public const string BackgroundColorNone = "BackgroundColorNone";
	public const string BackgroundColorYellow = "BackgroundColorYellow";
	public const string OpacityHalf = "OpacityHalf";
	public const string InputTransparentTrue = "InputTransparentTrue";
	public const string RotationEntry = "RotationEntry";
	public const string ScaleEntry = "ScaleEntry";

	// IsPressed and IsLoading are readonly properties (BindableProperty.CreateReadOnly),
	// so they cannot be set from the UI and are not testable via UI tests.

	public override string GalleryPageName => ImageButtonFeatureMatrix;

	public ImageButtonFeatureTests(TestDevice device)
	: base(device)
	{
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/30576
	[Test, Order(3)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(5)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/29956
	[Test, Order(6)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(7)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/30576 , Issue Link: https://github.com/dotnet/maui/issues/29959
	[Test, Order(10)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

#if TEST_FAILS_ON_WINDOWS  // Issue Link: https://github.com/dotnet/maui/issues/29959
	[Test, Order(12)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

#if TEST_FAILS_ON_ANDROID  && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/29956 , // Issue Link: https://github.com/dotnet/maui/issues/29959
    [Test, Order(15)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
	
	[Test, Order(16)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(17)]
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

	[Test, Order(18)]
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
	[Test, Order(19)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(20)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(21)]
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

	[Test, Order(22)]
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

	[Test, Order(23)]
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

	[Test, Order(24)]
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

	[Test, Order(25)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(26)]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithCornerRadius()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(CornerRadiusEntry);
		App.ClearText(CornerRadiusEntry);
		App.EnterText(CornerRadiusEntry, "50");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(27)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(28)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(29)]
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(30)]
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
		App.EnterText(CornerRadiusEntry, "30");
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(31)]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithOpacityHalf()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OpacityHalf);
		App.Tap(OpacityHalf);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(32)]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithInputTransparent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(InputTransparentTrue);
		App.Tap(InputTransparentTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		App.Tap("ImageButtonControl");
		App.WaitForNoElement("ImageButton Clicked: 1");
	}

	[Test, Order(33)]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithRotation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(RotationEntry);
		App.ClearText(RotationEntry);
		App.EnterText(RotationEntry, "180");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(34)]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithScale()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ScaleEntry);
		App.ClearText(ScaleEntry);
		App.EnterText(ScaleEntry, "0.5");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_WINDOWS
	[Test, Order(35)]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithBackgroundColorYellow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(BackgroundColorYellow);
		App.Tap(BackgroundColorYellow);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif 

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS
	[Test, Order(36)]
	[Category(UITestCategories.ImageButton)]
	public void VerifyImageButtonWithBackgroundColorNone()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(BackgroundColorNone);
		App.Tap(BackgroundColorNone);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ImageButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif
}