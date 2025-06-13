using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2767 : _IssuesUITest
{
	public Issue2767(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ArgumentException: NaN not valid for height";

	[Fact]
	[Category(UITestCategories.Layout)]

	public void Issue2767Test()
	{
		App.WaitForElement("Label 1:1");
		Assert.Equal(0, App.FindElements("Collapsed").Count);
	}
}