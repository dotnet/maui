using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ListViewNRE : _IssuesUITest
{
	public ListViewNRE(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView crashes when disposed on ItemSelected";

	[Fact]
	[Category(UITestCategories.ListView)]
	public void ListViewNRETest()
	{
		App.WaitForElement("1");
		App.Tap("1");
		App.WaitForElement("Success");
	}
}