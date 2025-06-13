using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57749 : _IssuesUITest
{
	public Bugzilla57749(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "After enabling a disabled button it is not clickable";

	[Fact]
	[Category(UITestCategories.Button)]
	public void Bugzilla57749Test()
	{
		App.WaitForElement("btnClick");
		App.Tap("btnClick");
		App.WaitForElement("Button was clicked");
		App.Tap("Ok");
	}
}