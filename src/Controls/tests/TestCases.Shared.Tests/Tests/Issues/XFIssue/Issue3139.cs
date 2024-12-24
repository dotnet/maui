using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3139 : _IssuesUITest
{
	public Issue3139(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "DisplayActionSheet is hiding behind Dialogs";

	 [Test]
	[Category(UITestCategories.ActionSheet)]
	public void Issue3139Test()
	{
		// Can't able to access items inside the action sheet on catalyst platform using the text.
#if MACCATALYST
		App.WaitForElement(AppiumQuery.ById("action-button--998"));
		App.Tap(AppiumQuery.ById("action-button--997"));
		App.WaitForElement(AppiumQuery.ById("action-button--998"));
		App.Tap(AppiumQuery.ById("action-button--997"));
		Assert.That(App.WaitForElement(AppiumQuery.ById("StatusLabel"))?.GetText(), Is.EqualTo("Test passed"));
#else
		App.WaitForElement("Click Yes");
		App.Tap("Yes");
		App.WaitForElement("Again Yes");
		App.Tap("Yes");
		Assert.That(App.WaitForElement("StatusLabel")?.GetText(), Is.EqualTo("Test passed"));
#endif
	}
}
