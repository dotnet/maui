using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3012 : _IssuesUITest
{
	const string OtherEntry = "OtherEntry";
	const string FocusTargetEntry = "FocusTargetEntry";

	public Issue3012(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS] Entry focus / unfocus behavior";

	[Fact]
	[Category(UITestCategories.Entry)]
	public void Issue3012Test()
	{
		App.WaitForElement(OtherEntry);
		App.Tap(OtherEntry);
		App.Tap(FocusTargetEntry);
		Assert.Equal($"Unfocused count: {0}", App.FindElement("UnfocusedCountLabel").GetText());
		App.Tap(OtherEntry);
		Assert.Equal($"Unfocused count: {1}", App.FindElement("UnfocusedCountLabel").GetText());
	}
}