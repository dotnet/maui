#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // In the sample, frequent changes to cell height and visibility cause rendering issues for the Group0 element in the UI, resulting in test failures on iOS and Catalyst. However, this does not thrown the reported exception.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9711 : _IssuesUITest
{
	public Issue9711(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] iOS Failed to marshal the Objective-C object HeaderWrapperView";

	[Test]
	[Category(UITestCategories.ListView)]
	public void TestTappingHeaderDoesNotCrash()
	{
		// Usually, tapping one header is sufficient to produce the exception.
		// However, sometimes it takes two taps, and rarely, three.  If the app
		// crashes, one of the RunningApp queries will throw, failing the test.

		Assert.DoesNotThrowAsync(async () =>
		{
			App.WaitForElement("Group2");
			App.Tap("Group2");
			await Task.Delay(3000);
			App.WaitForElement("Group1");
			App.Tap("Group1");
			await Task.Delay(3000);
			App.WaitForElement("Group0");
			App.Tap("Group0");
			await Task.Delay(3000);
			App.WaitForElement("9711TestListView");
		});
	}
}
#endif