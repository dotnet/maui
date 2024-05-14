using Maui.Controls.Sample;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests;

[Category(UITestCategories.Image)]
internal class ImageButtonUITests : _ViewUITests
{
	const string ImageButtonGallery = "Image Button Gallery";

	public ImageButtonUITests(TestDevice device)
		: base(device)
	{
	}

	protected override void NavigateToGallery()
	{
		App.NavigateToGallery(ImageButtonGallery);
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void Aspect()
	{
		var remote = GoToStateRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_State_AspectFit");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Aspect_State_AspectFill");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Aspect_State_Fill");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Aspect_State_Center");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Aspect_State_AspectFit");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void Aspect_AspectFill()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_AspectFill");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void Aspect_AspectFit()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_AspectFit");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void Aspect_Fill()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_Fill");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void Aspect_Center()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_Center");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void BorderColor()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderColor");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void CornerRadius()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_CornerRadius");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void BorderWidth()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderWidth");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void BorderColor_WithBackground()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderColor_WithBackground");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void CornerRadius_WithBackground()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_CornerRadius_WithBackground");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void BorderWidth_WithBackground()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderWidth_WithBackground");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void Clicked()
	{
		var remote = GoToEventRemote();

		var textBeforeClick = remote.GetEventLabel().GetText();
		Assert.AreEqual("Event: Clicked (none)", textBeforeClick);

		// Click ImageButton
		remote.TapView();

		var textAfterClick = remote.GetEventLabel().GetText();
		Assert.AreEqual("Event: Clicked (fired 1)", textAfterClick);
	}

	// TODO: TouchAndHoldView is missing
	//[Test]
	//[Category(UITestCategories.ImageButton)]
	//public void Pressed()
	//{
	//	var remote = GoToEventRemote();

	//	var textBeforeClick = remote.GetEventLabel().GetText();
	//	Assert.AreEqual("Event: Pressed (none)", textBeforeClick);

	//	// Press ImageButton
	//	remote.TouchAndHoldView();

	//	var textAfterClick = remote.GetEventLabel().GetText();
	//	Assert.AreEqual("Event: Pressed (fired 1)", textAfterClick);
	//}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void Command()
	{
		var remote = GoToEventRemote();

		var textBeforeClick = remote.GetEventLabel().GetText();
		Assert.AreEqual("Event: Clicked (none)", textBeforeClick);

		// Click ImageButton
		remote.TapView();

		var textAfterClick = remote.GetEventLabel().GetText();
		Assert.AreEqual("Event: Clicked (fired 1)", textAfterClick);
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void Padding()
	{
		var remote = GoToStateRemote();
		VerifyScreenshot("ImageButtonUITests_Padding_Initial");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Padding_Removed");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Padding_Initial");
	}

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void Padding_Add()
	{
		var remote = GoToStateRemote();
		VerifyScreenshot("ImageButtonUITests_Padding_Add_Initial");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Padding_Add_Removed");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Padding_Add_Initial");
	}
}
