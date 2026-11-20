#if TEST_FAILS_ON_WINDOWS // Windows SwipeControl does not support programmatic opening (dotnet/maui#14777).
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40474 : _IssuesUITest
{
	public Issue40474(TestDevice device) : base(device) { }

	public override string Issue => "SwipeItem background should reset when cleared";

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeItemBackgroundShouldClearAndRestoreWhileOpen()
	{
		App.WaitForElement("OpenSwipeViewButton");
		App.Tap("OpenSwipeViewButton");
		App.WaitForElement("Action");

		Exception? exception = null;
		VerifyScreenshotOrSetException(ref exception, "Issue40474_OrangeBackground", retryTimeout: TimeSpan.FromSeconds(2));

		App.WaitForElement("ClearBackgroundButton");
		App.Tap("ClearBackgroundButton");
		App.WaitForElement("Action");
		VerifyScreenshotOrSetException(ref exception, "Issue40474_ClearedBackground", retryTimeout: TimeSpan.FromSeconds(2));

		App.WaitForElement("RestoreBackgroundButton");
		App.Tap("RestoreBackgroundButton");
		App.WaitForElement("Action");
		VerifyScreenshotOrSetException(ref exception, "Issue40474_RestoredBackground", retryTimeout: TimeSpan.FromSeconds(2));

		if (exception is not null)
		{
			throw exception;
		}
	}
}
#endif
