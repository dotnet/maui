using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue10222 : _IssuesUITest
{
	public Issue10222(TestDevice device) : base(device)
	{
	}
	public override string Issue => "[CollectionView] ObjectDisposedException if the page is closed during scrolling";

	[Fact]
	[Trait("Category", UITestCategories.LifeCycle)]
	public void Issue10222Test()
	{
		App.WaitForElement("goTo");
		App.Tap("goTo");
		App.WaitForElement("items1");
		App.Tap("items1");
		App.WaitForElement("goTo", timeout: TimeSpan.FromSeconds(2));
	}
}