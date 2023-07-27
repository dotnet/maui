using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests;

public class LabelUITests : _ViewUITests
{
	static readonly string Label = "android.widget.TextView";
	const string LabelGallery = "* marked:'Label Gallery'";

	public LabelUITests(TestDevice device)
		: base(device)
	{
		PlatformViewType = Label;
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
		if (UITestContext.TestConfig.TestDevice == TestDevice.Mac ||
			UITestContext.TestConfig.TestDevice == TestDevice.iOS ||
			UITestContext.TestConfig.TestDevice == TestDevice.Windows)
		{
			Assert.Ignore("This test is failing on iOS/Mac Catalyst/Windows because the feature is not yet implemented: https://github.com/dotnet/maui/issues/4734");
		}

		var remote = new EventViewContainerRemote(UITestContext, Test.FormattedString.SpanTapped, PlatformViewType);
		remote.GoTo();

		var textBeforeClick = remote.GetEventLabel().Text;
		Assert.AreEqual("Event: SpanTapped (none)", textBeforeClick);

		// TODO: This will probably fail on desktops because the tap is in screen coordinates and the
		//       view seems to either be in window or parent coordinates.
		var r = remote.GetView().Rect;
		App.TapCoordinates(r.X + (r.Height * 3), r.CenterY); // 3 "heights" in from the left

		var textAfterClick = remote.GetEventLabel().Text;
		Assert.AreEqual("Event: SpanTapped (fired 1)", textAfterClick);
	}
}
