using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31064 : _IssuesUITest
{
	public Issue31064(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Indicator view size should update dynamically";

	[Test]
	[Category(UITestCategories.IndicatorView)]
	public void Issue31064IndicatorViewSizeUpdatesDynamically()
	{
		App.WaitForElement("Issue31064Button");
		App.Tap("Issue31064Button");
		VerifyScreenshot();
	}
}