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
#if WINDOWS
		App.Tap("navViewItem");
		App.WaitForElement("DogsPage");
		App.Tap("DogsPage");
#else
		App.WaitForElement("CatsPageUpdateQueryBtn");
		App.TapTab("DogsPage");
#endif
		App.WaitForElement("UpdateQueryButton");
		App.Tap("UpdateQueryButton");
		var searchHandlerString = App.GetShellSearchHandler().GetText();
		Assert.That(searchHandlerString, Is.EqualTo("Hound"));
	}
}