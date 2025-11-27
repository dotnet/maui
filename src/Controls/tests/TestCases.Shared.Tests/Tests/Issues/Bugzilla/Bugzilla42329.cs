using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla42329 : _IssuesUITest
{
	const string Page1Title = "Page1";
	const string Page2Title = "Page2";
	const string Page3Title = "Page3";
	const string LabelPage1 = "Open the drawer menu and select Page2";
	const string LabelPage2 = "Open the drawer menu and select Page3";

	public Bugzilla42329(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView in Frame and FormsAppCompatActivity Memory Leak";

	[Test]
#if ANDROID
	[Ignore("Failing on net10 https://github.com/dotnet/maui/issues/27411")]
#endif
	[Category(UITestCategories.ListView)]
	public async Task MemoryLeakB42329()
	{
		App.WaitForElement(LabelPage1);
		App.Tap(LabelPage1);

		await WaitForFlyoutAnimation();
		App.WaitForElement(Page2Title);
		App.Tap(Page2Title);

		await WaitForFlyoutAnimation();
		App.WaitForElement(LabelPage2);
		App.Tap(LabelPage2);

		await WaitForFlyoutAnimation();
		App.WaitForElement(Page3Title);
		App.Tap(Page3Title);

#if ANDROID || WINDOWS //In random scenario, the destructor called upon the fourth navigation. So added one more navigation for Android and Windows to make this test work.
		App.TapInFlyoutPageFlyout(Page2Title);
		App.TapInFlyoutPageFlyout(Page3Title);
#endif
		App.WaitForElement("Destructor called");
	}

	static async Task WaitForFlyoutAnimation()
	{
		// give it time to complete flyout animation
		// sometimes the test runner is too fast and taps on wrong coordinates
		await Task.Delay(100);
	}
}