#if ANDROID
// Reproduces the Android only PixelCopy deadlock triggered by Sentry's synchronous screenshot attachment without adding the third-party SDK to the shared HostApp.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37638 : _IssuesUITest
{
	public Issue37638(TestDevice device) : base(device)
	{
	}

	public override string Issue =>
		"Screenshot.CaptureAsync deadlocks when awaited synchronously from the UI thread";

	[Test]
	[Category(UITestCategories.Essentials)]
	public void GenerateErrorIsCapturedWithoutDeadlockingUIThread()
	{
		App.WaitForElement("GenerateErrorButton");
		App.Tap("GenerateErrorButton");
		Assert.That(
			App.WaitForTextToBePresentInElement("StatusLabel", "Error captured"),
			Is.True);
	}
}
#endif
