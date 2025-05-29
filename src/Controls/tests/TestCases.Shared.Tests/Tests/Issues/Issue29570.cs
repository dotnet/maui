#if TEST_FAILS_ON_WINDOWS // A fix for this issue is already available for Windows platform in an open PR (https://github.com/dotnet/maui/pull/29441), so the test is restricted on Windows for now.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29570 : _IssuesUITest
{
	public Issue29570(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Unable to Programmatically Update SearchHandler Query Value on Second Tab within ShellSection";

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifySearchQueryUpdateOnSecondShellTab()
	{
		App.WaitForElement("CatsPageUpdateQueryBtn");
		App.TapTab("DogsPage");
		App.WaitForElement("UpdateQueryButton");
		App.Tap("UpdateQueryButton");
		var searchHandlerString = App.GetShellSearchHandler().GetText();
		Assert.That(searchHandlerString, Is.EqualTo("Hound"));
	}
}
#endif