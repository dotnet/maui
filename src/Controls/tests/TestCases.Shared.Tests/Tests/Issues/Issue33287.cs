using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33287 : _IssuesUITest
{
	public override string Issue => "DisplayAlertAsync throws NullReferenceException when page is no longer displayed";

	public Issue33287(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Page)]
	public void DisplayAlertAsyncShouldNotCrashWhenPageUnloaded()
	{
		App.WaitForElement("NavigateButton");

		App.Tap("NavigateButton");

		var backButton = AppiumQuery.ByAccessibilityId("GoBackButton");
		App.WaitForElement(backButton);
		App.Tap(backButton);

		App.WaitForElement("MainPageLabel");
		App.WaitForTextToBePresentInElement("AlertResultLabel", "Completed");

		Assert.That(App.FindElement("AlertResultLabel").GetText(), Is.EqualTo("Completed"),
			"DisplayAlertAsync must complete after the second page is unloaded and detached");
		Assert.That(App.FindElement("MainPageLabel").GetText(), Is.EqualTo("MainPage"),
			"App should remain responsive after DisplayAlertAsync on an unloaded page");
	}
}
