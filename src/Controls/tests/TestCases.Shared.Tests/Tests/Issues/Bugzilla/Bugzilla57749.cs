#if TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57749 : _IssuesUITest
{
	public Bugzilla57749(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "After enabling a disabled button it is not clickable";

	[Test]
	[Category(UITestCategories.Button)]
	[FailsOnIOSWhenRunningOnXamarinUITest]
	public async Task Bugzilla57749Test()
	{
		await Task.Delay(500);
		App.Tap("btnClick");
		App.WaitForElement("Button was clicked");
		App.Tap("Ok");
	}
}
#endif
