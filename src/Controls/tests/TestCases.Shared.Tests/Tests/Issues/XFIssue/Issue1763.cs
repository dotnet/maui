#if TEST_FAILS_ON_WINDOWS // The test fails on Windows because the ListView text is not visible, Issue: https://github.com/dotnet/maui/issues/26488
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1763 : _IssuesUITest
{
	public Issue1763(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "First item of grouped ListView not firing ItemTapped";

	[Test]
	[Category(UITestCategories.ListView)]
	public void TestIssue1763ItemTappedFiring()
	{
		App.WaitForElement("Egor1");
		App.Tap("Egor1");
		App.WaitForElement("Tapped a List item");
		App.Tap("Destruction");
		App.WaitForElement("Egor1");
	}
}
#endif