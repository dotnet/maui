using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Image)]
internal class ImageUITests : _ViewUITests
{
	const string ImageGallery = "Image Gallery";

	public override string GalleryPageName => ImageGallery;
	
	public ImageUITests(TestDevice device)
		: base(device)
	{
	}

	protected override void NavigateToGallery()
	{
		App.NavigateToGallery(ImageGallery);
	}

	public override void IsEnabled()
	{
		Assert.Ignore("Image elements do not really have a concept of being \"disabled\".");
	}

#if TEST_FAILS_ON_CATALYST
	[Test]
	public void Source_FontImageSource()
	{
		Exception? exception = null;

		var remote = GoToStateRemote();
		VerifyScreenshotOrSetException(ref exception, "ImageUITests_Source_FontImageSource_FontAwesome");

		remote.TapStateButton();
		VerifyScreenshotOrSetException(ref exception, "ImageUITests_Source_FontImageSource_Ionicons");

		remote.TapStateButton();
		VerifyScreenshotOrSetException(ref exception, "ImageUITests_Source_FontImageSource_FontAwesome");

		if (exception is not null)
		{
			throw exception;
		}
	}
#endif

	[Test]
	public async Task IsAnimationPlaying()
	{
		Exception? exception = null;

		var remote = GoToStateRemote();
		await Task.Delay(500); // make sure the gif is NOT playing
		VerifyScreenshotOrSetException(ref exception, "ImageUITests_IsAnimationPlaying_No");

		remote.TapStateButton();
		await Task.Delay(500); // make sure the gif IS playing
		VerifyScreenshotOrSetException(ref exception, "ImageUITests_IsAnimationPlaying_Yes");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
