using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3001 : _IssuesUITest
{
	const string ButtonId = "ClearButton";
	const string ReadyId = "ReadyLabel";

	public Issue3001(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS] Navigating back from a complex page is highly inefficient";

	[Fact]
	[Trait("Category", UITestCategories.Navigation)]
	[Trait("Category", UITestCategories.Compatibility)]
	public void Issue3001Test()
	{
		App.WaitForElement(ButtonId);
		App.Tap(ButtonId);
		App.WaitForElement(ReadyId, timeout: TimeSpan.FromSeconds(5));
	}
}