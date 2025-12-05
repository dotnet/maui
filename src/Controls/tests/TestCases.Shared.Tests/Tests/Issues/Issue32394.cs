#if ANDROID || IOS //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32394 : _IssuesUITest
{
	public Issue32394(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CurrentItem Should not update on Orientation Change";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void Issue32394CurrentItemShouldnotChange()
	{
		App.WaitForElement("Issue32394SetPositionButton");
		App.Tap("Issue32394SetPositionButton");
		App.SetOrientationLandscape();
		VerifyScreenshot();
	}
}
#endif