#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS// https://github.com/dotnet/maui/issues/28910
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8632 : _IssuesUITest
{
	public Issue8632(TestDevice device) : base(device) { }

	public override string Issue => "ScalingCanvas.SetBlur not working on Android";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void CanvasShouldHonorBlur()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}
#endif