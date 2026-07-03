#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //Window issue link - https://github.com/dotnet/maui/issues/29125 && iOS and Mac PR - https://github.com/dotnet/maui/pull/34184
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue13258 : _IssuesUITest
{
	public Issue13258(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "MAUI Slider thumb image is big on android";

	[Test]
	[Category(UITestCategories.Slider)]
	public void SliderThumbImageShouldBeScaled()
	{
		App.WaitForElement("ToggleImageBtn");
		App.Tap("ToggleImageBtn");
		VerifyScreenshot();
	}
}
#endif
