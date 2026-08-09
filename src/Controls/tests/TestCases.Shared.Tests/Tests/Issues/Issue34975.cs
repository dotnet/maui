using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34975 : _IssuesUITest
{
	public override string Issue => "Title view memory leak when using Shell TitleView and x Name";

	public Issue34975(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellTitleViewWithXNameShouldNotLeakMemory()
	{
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");

		App.WaitForElement("CheckMemoryButton");
		App.Tap("CheckMemoryButton");

		// GarbageCollectionHelper.WaitForGC runs for up to 10 s inside the app,
		// so allow 2s total for the status label to reflect the final result.
		App.WaitForTextToBePresentInElement("StatusLabel", "Still alive:", timeout: TimeSpan.FromSeconds(2));
		Assert.That(App.FindElement("StatusLabel").GetText(), Is.EqualTo("Still alive: 0"),
			"Memory leak detected: SecondPage instances were not garbage collected.");
	}
}
