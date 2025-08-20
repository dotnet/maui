#if TEST_FAILS_ON_ANDROID // https://github.com/dotnet/maui/issues/19568 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31239(TestDevice testDevice) : _IssuesUITest(testDevice)
{
	const string _changeBackgroundButtonId = "changeBackgroundButton";
	public override string Issue => "[iOS, Mac, Windows] GraphicsView does not change the Background/BackgroundColor";

	[Test, Order(1)]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewBackgroundShouldBeApplied()
	{
		App.WaitForElement(_changeBackgroundButtonId);
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewBackgroundShouldBeChanged()
	{
		App.WaitForElement(_changeBackgroundButtonId);
		App.Tap(_changeBackgroundButtonId);
		VerifyScreenshot();
	}
}
#endif