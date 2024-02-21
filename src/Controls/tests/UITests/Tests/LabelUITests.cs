using Maui.Controls.Sample;
using Microsoft.Maui.AppiumTests;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests;

public class LabelUITests : _ViewUITests
{
	const string LabelGallery = "Label Gallery";

	public LabelUITests(TestDevice device)
		: base(device)
	{
	}

	protected override void NavigateToGallery() =>
		App.NavigateToGallery(LabelGallery);

	public override void _IsEnabled()
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
		Assert.AreEqual("Event: SpanTapped (none)", textBeforeClick);

		// TODO: This will probably fail on desktops because the tap is in screen coordinates and the
		//       view seems to either be in window or parent coordinates.
		var r = remote.GetView().GetRect();
		App.Click(r.X + (r.Height * 3), r.CenterY()); // 3 "heights" in from the left

		var textAfterClick = remote.GetEventLabel().GetText();
		Assert.AreEqual("Event: SpanTapped (fired 1)", textAfterClick);
	}
}
