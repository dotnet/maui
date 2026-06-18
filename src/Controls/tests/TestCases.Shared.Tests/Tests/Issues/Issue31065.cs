using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31065 : _IssuesUITest
{
	public override string Issue => "IndicatorView square shape does not update on load or dynamically";

	public Issue31065(TestDevice device) : base(device)
	{ }

	[Test, Order(0)]
	[Category(UITestCategories.IndicatorView)]
	public void UpdateIndicatorViewSquareShape()
	{
		App.WaitForElement("ChangeIndicatorShapeButton");
		VerifyScreenshot("IndicatorViewSquareShape");
	}

	[Test, Order(1)]
	[Category(UITestCategories.IndicatorView)]
	public void UpdateIndicatorViewCircleShape()
	{
		App.Tap("ChangeIndicatorShapeButton");
		VerifyScreenshot("IndicatorViewCircleShape");
	}

}
