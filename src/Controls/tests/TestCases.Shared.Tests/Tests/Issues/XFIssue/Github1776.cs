using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Github1776 : _IssuesUITest
{
	public Github1776(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Button Released not being triggered";

	[Fact]
	[Category(UITestCategories.Button)]
	public void GitHub1776Test()
	{
		App.WaitForElement("TheButton");
		App.Tap("TheButton");
		Assert.Equal("Pressed: 1", App.FindElement("PressedLabel").GetText());
		Assert.Equal("Released: 1", App.FindElement("ReleasedLabel").GetText());
		Assert.Equal("Clicked: 1", App.FindElement("ClickedLabel").GetText());
		Assert.Equal("Command: 1", App.FindElement("CommandLabel").GetText());
	}
}
