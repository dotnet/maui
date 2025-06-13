using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1439 : _IssuesUITest
{
	public Issue1439(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ItemTapped event for a grouped ListView is not working as expected.";

	[Fact]
	[Category(UITestCategories.TableView)]
	public void Issue1439Test()
	{
		App.WaitForElement("A");
		App.Tap("A");

		Assert.Equal("A", App.FindElement("lblItem").GetText());
		Assert.Equal("Group 1", App.FindElement("lblGroup").GetText());

		App.Tap("B");

		Assert.Equal("B", App.FindElement("lblItem").GetText());
		Assert.Equal("Group 1", App.FindElement("lblGroup").GetText());

		App.Tap("C");

		Assert.Equal("C", App.FindElement("lblItem").GetText());
		Assert.Equal("Group 2", App.FindElement("lblGroup").GetText());

		App.Tap("D");

		Assert.Equal("D", App.FindElement("lblItem").GetText());
		Assert.Equal("Group 2", App.FindElement("lblGroup").GetText());
	}
}