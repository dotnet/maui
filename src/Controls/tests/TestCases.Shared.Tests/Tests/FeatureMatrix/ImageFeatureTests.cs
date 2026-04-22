using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class ImageFeatureTests : _GalleryUITest
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
	public const string IsEnabledTrue = "IsEnabledTrue";
	public const string IsEnabledFalse = "IsEnabledFalse";
	public const string BackgroundColorNone = "BackgroundColorNone";
	public const string BackgroundColorYellow = "BackgroundColorYellow";
	public const string OpacityHalf = "OpacityHalf";
	public const string FontColorGreen = "FontColorGreen";
	public const string EntryFontSize = "EntryFontSize";
	public const string ImageControl = "ImageControl";
	public const string TapResultLabel = "TapResultLabel";

	public override string GalleryPageName => ImageFeatureMatrix;

	public ImageFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
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
		App.WaitForElement(ImageControl, timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/30576
	[Test, Order(3)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif
	[Test, Order(5)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(6)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
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
		App.WaitForElement(ImageControl, timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/30576
	[Test, Order(8)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(9)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
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
		App.WaitForElement(ImageControl, timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
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
		App.WaitForElement(ImageControl, timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // Issue Link for Windows: https://github.com/dotnet/maui/issues/29813 and for Android: https://github.com/dotnet/maui/issues/30576
	[Test, Order(15)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(16)]
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
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(17)]
	[Category(UITestCategories.Image)]
	public void VerifyFontImageWithFontColorGreen()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(FontColorGreen);
		App.Tap(FontColorGreen);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/22210
	[Test, Order(18)]
	[Category(UITestCategories.Image)]
	public void VerifyFontImageWithFontSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(EntryFontSize);
		App.ClearText(EntryFontSize);
		App.Tap(EntryFontSize);
		App.EnterText(EntryFontSize, "100");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(19)]
	[Category(UITestCategories.Image)]
	public void VerifyImageWithIsVisibleFalse()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(IsVisibleFalseRadio);
		App.Tap(IsVisibleFalseRadio);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForNoElement(ImageControl);
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/29812
	[Test, Order(20)]
	[Category(UITestCategories.Image)]
	public void VerifyImageWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(ShadowCheckBox);
		App.Tap(ShadowCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(21)]
	[Category(UITestCategories.Image)]
	public void VerifyImageFlowDirectionRTL()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(22)]
	[Category(UITestCategories.Image)]
	public void VerifyImageWithAnimation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsAnimationTrue);
		App.Tap(IsAnimationTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
	}

	[Test, Order(23)]
	[Category(UITestCategories.Image)]
	public void VerifyImageWithoutAnimation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsAnimationFalse);
		App.Tap(IsAnimationFalse);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
	}

	[Test, Order(24)]
	[Category(UITestCategories.Image)]
	public void VerifyImageWithOpacityHalf()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(OpacityHalf);
		App.Tap(OpacityHalf);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(25)]
	[Category(UITestCategories.Image)]
	public void VerifyImageWhenIsEnabledFalse()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(IsEnabledFalse);
		App.Tap(IsEnabledFalse);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
		App.Tap(ImageControl);
		App.WaitForNoElement(TapResultLabel);
	}

	[Test, Order(26)]
	[Category(UITestCategories.Image)]
	public void VerifyImageWhenIsEnabledTrue()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFile);
		App.Tap(SourceTypeFile);
		App.WaitForElement(IsEnabledFalse);
		App.Tap(IsEnabledFalse);
		App.WaitForElement(IsEnabledTrue);
		App.Tap(IsEnabledTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
		App.Tap(ImageControl);
		App.WaitForElement(TapResultLabel);
		var tapResult = App.FindElement(TapResultLabel).GetText();
		Assert.That(tapResult, Is.EqualTo("Tapped"));
	}

#if TEST_FAILS_ON_WINDOWS
	[Test, Order(27)]
	[Category(UITestCategories.Image)]
	public void VerifyImageWithBackgroundColorYellow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(BackgroundColorYellow);
		App.Tap(BackgroundColorYellow);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif
#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
	[Test, Order(28)]
	[Category(UITestCategories.Image)]
	public void VerifyImageWithBackgroundColorNone()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SourceTypeFontImage);
		App.Tap(SourceTypeFontImage);
		App.WaitForElement(BackgroundColorNone);
		App.Tap(BackgroundColorNone);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ImageControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif
}