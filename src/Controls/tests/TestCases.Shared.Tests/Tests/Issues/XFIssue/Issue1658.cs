#if WINDOWS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1658 : _IssuesUITest
{
	public Issue1658(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS] GestureRecognizer on ListView Item not working";

	[Test]
	[Category(UITestCategories.ActivityIndicator)]
	public void ContextActionsIconImageSource()
	{
		App.WaitForElement("ListViewItem");
		App.ContextActions("ListViewItem");
		VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "WithCoffeeIcon");
		App.WaitForElement("ColorBox");
		App.Tap("ColorBox");
		VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "InYellowBox");
	}
}
#endif