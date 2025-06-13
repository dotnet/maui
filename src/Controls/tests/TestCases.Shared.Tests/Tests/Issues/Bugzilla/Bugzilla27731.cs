using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla27731 : _IssuesUITest
{
	public Bugzilla27731(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Action Bar can not be controlled reliably on FlyoutPage";

	[Fact]
	[Category(UITestCategories.InputTransparent)]
	public void Bugzilla27731Test()
	{
		App.WaitForElement("Click");
		App.WaitForElement("PageLabel");
	}
}