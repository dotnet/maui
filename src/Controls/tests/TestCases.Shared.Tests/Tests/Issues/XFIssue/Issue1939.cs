using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1939 : _IssuesUITest
{
	public Issue1939(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ArgumentOutOfRangeException on clearing a group on a grouped ListView on Android";

	[Fact]
	[Category(UITestCategories.ListView)]
	public void Issue1939Test()
	{
		App.WaitForElement("Group #1");
	}
}