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

	//[Test]
	//[Category(UITestCategories.ListView)]
	//[FailsOnMauiIOS]
	//public void TestTappingHeaderDoesNotCrash()
	//{
	//	// Usually, tapping one header is sufficient to produce the exception.
	//	// However, sometimes it takes two taps, and rarely, three.  If the app
	//	// crashes, one of the RunningApp queries will throw, failing the test.
	//	Assert.DoesNotThrowAsync(async () =>
	//	{
	//		App.Tap(x => x.Marked("Group2"));
	//		await Task.Delay(3000);
	//		App.Tap(x => x.Marked("Group1"));
	//		await Task.Delay(3000);
	//		App.Tap(x => x.Marked("Group0"));
	//		await Task.Delay(3000);
	//		App.Query(x => x.Marked("9711TestListView"));
	//	});
	//}
}