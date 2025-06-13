#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Need to Implement App.PressEnter() for Catalyst and Windows.
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Github1650 : _IssuesUITest
{
	public Github1650(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS] Completed event of Entry raised on Tab key";

	[Fact]
	[Category(UITestCategories.Entry)]
	public void GitHub1650Test()
	{
		App.WaitForElement("CompletedTargetEntry");
		App.Tap("CompletedTargetEntry");
		Assert.Equal("Completed count: 0", App.FindElement("CompletedCountLabel").GetText());
		App.PressEnter();
		Assert.Equal("Completed count: 1", App.FindElement("CompletedCountLabel").GetText());
	}
}
#endif