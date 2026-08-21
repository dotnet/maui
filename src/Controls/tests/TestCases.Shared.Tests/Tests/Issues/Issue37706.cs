#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37706 : _IssuesUITest
{
	public Issue37706(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Page.OnBackButtonPressed is not invoked on a root page";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void RootPageBackButtonOverrideIsInvokedOnce()
	{
		App.WaitForElement("RootPageLabel");

		App.Back();

		Assert.That(
			App.WaitForTextToBePresentInElement("BackButtonPressedStatus", "OnBackButtonPressed called 1 time"),
			Is.True,
			"OnBackButtonPressed should be called exactly once.");
		App.WaitForElement("RootPageLabel");
	}
}
#endif
