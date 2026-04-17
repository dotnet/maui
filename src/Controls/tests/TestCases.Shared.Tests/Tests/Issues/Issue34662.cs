using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34662 : _IssuesUITest
{
	public Issue34662(TestDevice device) : base(device) { }

	public override string Issue => "Shell OnNavigated not called for route navigation";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellCurrentStateLocationCorrectAfterAbsoluteNavigation()
	{
		const string expectedCurrentState = "//DashboardPage/Page1/Page2";

		App.WaitForElement("LoginButton");
		App.Tap("LoginButton");
		App.WaitForTextToBePresentInElement("OnNavigatedCurrentStateLabel", expectedCurrentState, TimeSpan.FromSeconds(5));

		var currentStateText = App.WaitForElement("OnNavigatedCurrentStateLabel").GetText();
		Assert.That(currentStateText, Is.EqualTo(expectedCurrentState),
			"Shell.CurrentState.Location inside OnNavigated should be '//DashboardPage/Page1/Page2', not stale '//DashboardPage'");
	}
}
