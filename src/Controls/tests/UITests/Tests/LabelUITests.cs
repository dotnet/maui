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
}
