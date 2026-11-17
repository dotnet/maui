using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ImageButton)]
internal class ImageButtonUITests : _ViewUITests
{
	const string ImageButtonGallery = "Image Button Gallery";

	public override string GalleryPageName => ImageButtonGallery;
	
	public ImageButtonUITests(TestDevice device)
		: base(device)
	{
	}

	protected override void NavigateToGallery()
	{
		App.NavigateToGallery(ImageButtonGallery);
	}

#if TEST_FAILS_ON_CATALYST
	[Test]
	public void Aspect()
	{
		Exception? exception = null;

		var remote = GoToStateRemote();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Aspect_State_AspectFit");

		remote.TapStateButton();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Aspect_State_AspectFill");

		remote.TapStateButton();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Aspect_State_Fill");

		remote.TapStateButton();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Aspect_State_Center");

		remote.TapStateButton();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Aspect_State_AspectFit");

		if (exception is not null)
		{
			throw exception;
		}
	}
#endif

	[Test]
	public void Aspect_AspectFill()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_AspectFill");
	}

	[Test]
	public void Aspect_AspectFit()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_AspectFit");
	}

	[Test]
	public void Aspect_Fill()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_Fill");
	}

	[Test]
	public void Aspect_Center()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_Center");
	}

	[Test]
	public void BorderColor()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderColor");
	}

	[Test]
	public void CornerRadius()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_CornerRadius");
	}

	[Test]
	public void BorderWidth()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderWidth");
	}

	[Test]
	public void BorderColor_WithBackground()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderColor_WithBackground");
	}

	[Test]
	public void CornerRadius_WithBackground()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_CornerRadius_WithBackground");
	}

	[Test]
	public void BorderWidth_WithBackground()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderWidth_WithBackground");
	}

	[Test]
	public void Clicked()
	{
		var remote = GoToEventRemote();

		var textBeforeClick = remote.GetEventLabel().GetText();
		Assert.That(textBeforeClick, Is.EqualTo("Event: Clicked (none)"));

		// Click ImageButton
		remote.TapView();

		var textAfterClick = remote.GetEventLabel().GetText();
		Assert.That(textAfterClick, Is.EqualTo("Event: Clicked (fired 1)"));
	}

	// TODO: TouchAndHoldView is missing
	//[Test]
	//public void Pressed()
	//{
	//	var remote = GoToEventRemote();

	//	var textBeforeClick = remote.GetEventLabel().GetText();
	//	Assert.That(textBeforeClick, Is.EqualTo("Event: Pressed (none)"));

	//	// Press ImageButton
	//	remote.TouchAndHoldView();

	//	var textAfterClick = remote.GetEventLabel().GetText();
	//	Assert.That(textAfterClick, Is.EqualTo("Event: Pressed (fired 1)"));
	//}

	[Test]
	public void Command()
	{
		var remote = GoToEventRemote();

		var textBeforeClick = remote.GetEventLabel().GetText();
		Assert.That(textBeforeClick, Is.EqualTo("Event: Command (none)"));

		// Click ImageButton
		remote.TapView();

		var textAfterClick = remote.GetEventLabel().GetText();
		Assert.That(textAfterClick, Is.EqualTo("Event: Command (fired 1)"));
	}

	[Test]
	public void Padding()
	{
		Exception? exception = null;

		var remote = GoToStateRemote();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Padding_Initial");

		remote.TapStateButton();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Padding_Removed");

		remote.TapStateButton();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Padding_Initial");

		if (exception is not null)
		{
			throw exception;
		}
	}

	[Test]
	public void Padding_Add()
	{
		Exception? exception = null;

		var remote = GoToStateRemote();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Padding_Add_Initial");

		remote.TapStateButton();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Padding_Add_Added");

		remote.TapStateButton();
		VerifyScreenshotOrSetException(ref exception, "ImageButtonUITests_Padding_Add_Initial");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
