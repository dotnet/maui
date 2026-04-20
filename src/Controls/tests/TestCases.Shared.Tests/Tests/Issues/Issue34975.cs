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

		App.WaitForTextToBePresentInElement("StatusLabel", "Test passed", timeout: TimeSpan.FromSeconds(15));
		Assert.That(App.FindElement("StatusLabel").GetText(), Does.Contain("Test passed"),
			"Memory leak detected: SecondPage instances were not garbage collected.");
	}
}
