#if TEST_FAILS_ON_WINDOWS // Test ignored on Windows due to rendering issues. The documentation specifies that TabbedPage should contain NavigationPage or ContentPage, but this sample uses nested TabbedPages. Getting System.InvalidOperationException: 'Collection was modified; enumeration operation may not execute.' exception.
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4303 : _IssuesUITest
{
	public Issue4303(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] TabbedPage's child is appearing before it should be";

	[Fact]
	[Category(UITestCategories.LifeCycle)]
	public void Issue4303Test()
	{
		Assert.Equal("Success", App.WaitForElement("lblAssert").GetText());
		App.WaitForElement("Go to Tab4");
		App.Tap("Go to Tab4");
		Assert.Equal("ChildSuccess", App.WaitForElement("lblChildAssert").GetText());
	}
}
#endif