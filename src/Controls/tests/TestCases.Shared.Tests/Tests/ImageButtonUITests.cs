using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Trait("Category", UITestCategories.ImageButton)]
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

#if TEST_FAILS_ON_CATALYST
	[Fact]
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
#endif

	[Fact]
	public void Aspect_AspectFill()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_AspectFill");
	}

	[Fact]
	public void Aspect_AspectFit()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_AspectFit");
	}

	[Fact]
	public void Aspect_Fill()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_Fill");
	}

	[Fact]
	public void Aspect_Center()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_Aspect_Center");
	}

	[Fact]
	public void BorderColor()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderColor");
	}

	[Fact]
	public void CornerRadius()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_CornerRadius");
	}

	[Fact]
	public void BorderWidth()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderWidth");
	}

	[Fact]
	public void BorderColor_WithBackground()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderColor_WithBackground");
	}

	[Fact]
	public void CornerRadius_WithBackground()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_CornerRadius_WithBackground");
	}

	[Fact]
	public void BorderWidth_WithBackground()
	{
		GoToRemote();
		VerifyScreenshot("ImageButtonUITests_BorderWidth_WithBackground");
	}

	[Fact]
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
	//[Fact]
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

	[Fact]
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

	[Fact]
	public void Padding()
	{
		var remote = GoToStateRemote();
		VerifyScreenshot("ImageButtonUITests_Padding_Initial");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Padding_Removed");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Padding_Initial");
	}

	[Fact]
	public void Padding_Add()
	{
		var remote = GoToStateRemote();
		VerifyScreenshot("ImageButtonUITests_Padding_Add_Initial");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Padding_Add_Added");

		remote.TapStateButton();
		VerifyScreenshot("ImageButtonUITests_Padding_Add_Initial");
	}
}
