#if IOS || ANDROID //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue29236 : _IssuesUITest
{
	public Issue29236(TestDevice device) : base(device) { }

	public override string Issue => "Window dimensions not updating after device orientation changes on iOS";

	[Test]
	[Category(UITestCategories.Window)]
	public void WindowDimensionsShouldUpdateAfterOrientationChange()
	{
		App.WaitForElement("Label");
		App.WaitForElement("GetDimensionsButton").Tap();

		var portraitWindowWidth = App.WaitForElement("WindowWidth").GetText();
    	var portraitWindowHeight = App.WaitForElement("WindowHeight").GetText();

		App.SetOrientationLandscape();
		App.WaitForElement("GetDimensionsButton").Tap();

		Assert.That(Math.Round(double.Parse(portraitWindowWidth ?? "0")), Is.EqualTo(Math.Round(double.Parse(App.WaitForElement("WindowHeight").GetText() ?? "0"))));
    	Assert.That(Math.Round(double.Parse(portraitWindowHeight ?? "0")), Is.EqualTo(Math.Round(double.Parse(App.WaitForElement("WindowWidth").GetText() ?? "0"))));
	}
}
#endif
