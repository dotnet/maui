using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19513 : _IssuesUITest
{
	public Issue19513(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "HorizontalStackLayout Crashes Debugger on Negative Spacing";
	[Test]
	[Category(UITestCategories.Layout)]
	public void NegativeSpacingCrashes()
	{
		App.WaitForElement("Image1");
	}
}
