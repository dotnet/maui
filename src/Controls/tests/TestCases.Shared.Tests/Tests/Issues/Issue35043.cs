#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35043 : _IssuesUITest
{
	public Issue35043(TestDevice device) : base(device) { }

	public override string Issue => "Landscape orientation with full screen no longer rotates";

	[Test]
	[Category(UITestCategories.Layout)]
	public void LandscapeOnlyContentRotatesWhileDeviceIsPortrait()
	{
		App.WaitForElement("EvaluateOrientation");
		App.SetOrientationPortrait();
		App.Tap("EvaluateOrientation");

		var orientation = App.WaitForElement("OrientationResult").GetText();

		Assert.That(orientation, Is.EqualTo("Landscape"),
			"Landscape-only content should rotate to landscape while the device is held in portrait.");
	}
}
#endif
