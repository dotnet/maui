using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12500 : _IssuesUITest
{
	public override string Issue => "Shell does not always raise Navigating event on Windows";

	public Issue12500(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellNavigatingShouldTrigger()
	{
		App.WaitForElement("Issue12500MainPage");
		App.WaitForElement("Events");
		App.Tap("Events");
		var result = App.WaitForElement("Issue12500EventPage").GetText();
		Assert.That(result, Is.EqualTo("Navigating to //EventPage"));
	}
}
