#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27711 : _IssuesUITest
{
	public Issue27711(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "FlowDirection = `RightToLeft` doesn't work with CV1 & CV2";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void RightToLeftFlowDirectionShouldWork()
	{
		App.WaitForElement("switch");
		App.Click("switch");
		VerifyScreenshot();
	}
}
#endif