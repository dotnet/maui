using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34318 : _IssuesUITest
{
	public Issue34318(TestDevice device) : base(device) { }

	public override string Issue => "Shell Navigating event should fire on ShellContent change";

	[Test]
	[Category(UITestCategories.Shell)]
	public void NavigatingFiresWhenShellContentChanges()
	{
		App.WaitForElement("ChangeContentButton");

		App.WaitForElement("ResultLabelB");

		var initialText = App.FindElement("ResultLabelB").GetText() ?? string.Empty;
		Assert.That(initialText, Is.EqualTo("Waiting"));

		App.Tap("ChangeContentButton");

		App.WaitForElement("PageBLabel");

		var result = App.WaitForTextToBePresentInElement("ResultLabelB", "Navigating");

		Assert.That(result, Is.True, "Navigating event should have fired and updated the label text");
	}
}
