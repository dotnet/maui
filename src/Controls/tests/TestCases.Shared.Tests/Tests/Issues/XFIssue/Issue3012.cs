using NUnit.Framework;
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

	[Test]
	[Category(UITestCategories.Entry)]
	public void Issue3012Test()
	{
		App.WaitForElement(OtherEntry);
		App.Tap(OtherEntry);
		App.Tap(FocusTargetEntry);
		Assert.That(App.FindElement("UnfocusedCountLabel").GetText(), Is.EqualTo($"Unfocused count: {0}"));
		App.Tap(OtherEntry);
		Assert.That(App.FindElement("UnfocusedCountLabel").GetText(), Is.EqualTo($"Unfocused count: {1}"));
	}
}