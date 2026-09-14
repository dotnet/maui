using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36694 : _IssuesUITest
{
	public Issue36694(TestDevice device) : base(device) { }

	public override string Issue => "Setting BackgroundColor on Image via binding causes COMException after navigation";

	[Test]
	[Category(UITestCategories.Image)]
	public void ChangingImageBackgroundColorAfterNavigationShouldNotThrowCOMException()
	{
		App.WaitForElement("TestImage");
		App.Tap("NavigateButton");
		App.WaitForElement("ChangeColorButton");
		App.Tap("ChangeColorButton");
		App.Tap("GoBackButton");
		App.WaitForElement("ResultLabel");
	}
}
