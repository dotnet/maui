using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Label)]
public class LabelUITests : _ViewUITests
{
	const string LabelGallery = "Label Gallery";

	public override string GalleryPageName => LabelGallery;

	public LabelUITests(TestDevice device)
		: base(device)
	{
	}

	protected override void NavigateToGallery() =>
		App.NavigateToGallery(LabelGallery);

	public override void IsEnabled()
	{
		Assert.Ignore("Labels do not really have a concept of being \"disabled\".");
	}

	[Test]
	public void SpanTapped()
	{
		if (Device == TestDevice.Mac)
		{
			Assert.Ignore("Click (x, y) pointer type mouse is not implemented.");
		}

		var remote = new EventViewContainerRemote(UITestContext, Test.FormattedString.SpanTapped);
		remote.GoTo();

		var textBeforeClick = remote.GetEventLabel().GetText();
		ClassicAssert.AreEqual("Event: SpanTapped (none)", textBeforeClick);

		// TODO: This will probably fail on desktops because the tap is in screen coordinates and the
		//       view seems to either be in window or parent coordinates.
		var r = remote.GetView().GetRect();
		App.TapCoordinates(r.X + (r.Height * 3), r.CenterY()); // 3 "heights" in from the left

		var textAfterClick = remote.GetEventLabel().GetText();
		ClassicAssert.AreEqual("Event: SpanTapped (fired 1)", textAfterClick);
	}

	[Test]
	public void FontFamilyLoadsDynamically()
	{
		var remote = GoToStateRemote("FontFamily");

		remote.TapStateButton();
		VerifyScreenshot("LabelUITests_FontFamily_Ionicons");
	}

#if WINDOWS
	[Ignore("Windows App SDK 1.6 broke this test. See more details in https://github.com/dotnet/maui/issues/26749")]
#endif
	[Test]
	public void FontFamily()
	{
		var remote = GoToStateRemote();

		VerifyScreenshot("LabelUITests_FontFamily_FontAwesome");

		remote.TapStateButton();
		VerifyScreenshot("LabelUITests_FontFamily_Ionicons");

		remote.TapStateButton();
		VerifyScreenshot("LabelUITests_FontFamily_FontAwesome");
	}
}
