#if WINDOWS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38949 : _IssuesUITest
{
	public Issue38949(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "Updating Window.TitleBar while pushing a modal page crashes on Windows";

	[Test]
	[Category(UITestCategories.Window)]
	public void TitleBarCanMoveToModalNavigationContext()
	{
		var issuePageLabel = App.WaitForElement("Issue38949PageLabel");
		Assert.That(issuePageLabel.GetText(), Is.EqualTo("TitleBar modal regression test"));

		App.WaitForElement("PushModalPageButton");
		App.Tap("PushModalPageButton");

		var modalPageLabel = App.WaitForElement("ModalPageLabel");
		Assert.That(modalPageLabel.GetText(), Is.EqualTo("Modal page displayed"));

		App.WaitForElement("PopModalPageButton");
		App.Tap("PopModalPageButton");

		App.WaitForElement("Issue38949PageLabel");
		App.WaitForElement("PushModalPageButton");
	}
}
#endif