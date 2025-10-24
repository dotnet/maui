using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31584 : _IssuesUITest
{
	public Issue31584(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Page OnAppearing triggered twice when navigating via ShellItem change with PresentationMode set to Modal";

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifyModalPageOnAppearingTriggeredOnceWithShellItemChange()
	{
		App.WaitForElement("NavigateToModalPageBtn");
		App.Tap("NavigateToModalPageBtn");

		App.WaitForElement("Issue31584StatusLabel");
		var resultLabel = App.WaitForElement("Issue31584StatusLabel").GetText();
		Assert.That(resultLabel, Is.EqualTo("Success"));
    }
}