using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18896 : _IssuesUITest
{
	const string ListView = "TestListView";

	public Issue18896(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Can scroll ListView inside RefreshView";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue18896Test()
	{
		App.WaitForElement("WaitForStubControl");

		App.ScrollDown(ListView);
#if ANDROID
		App.ScrollUp(ListView, ScrollStrategy.Gesture, 0.9);
#else
		App.ScrollUp(ListView);
#endif
		// ListView with HasUnevenRows may have variable height row rendering that requires
		// additional time for images to load and scrollbar to disappear.
		// Use retryTimeout to adaptively wait for the UI to stabilize.
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(3));
	}
}