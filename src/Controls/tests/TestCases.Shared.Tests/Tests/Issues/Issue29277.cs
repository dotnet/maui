using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue29277 : _IssuesUITest
{
	public Issue29277(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell SearchHandler.Unfocus() has no effect on iOS & Android";

    [Test]
	[Category(UITestCategories.Shell)]
	public void FocusCallShouldWorkOnSearchHandler()
	{
		App.WaitForElement("FoucsButton");
        App.Tap("FoucsButton");
		Thread.Sleep(2000);
        var text  = App.WaitForElement("MauiLabel").GetText();
		Assert.That(text,Is.EqualTo("Focused"));
	}

    [Test]
	[Category(UITestCategories.Shell)]
	public void UnfocusCallShouldWorkOnSearchHandler()
	{
		App.WaitForElement("UnfocusButton");
        App.Tap("UnfocusButton");
		Thread.Sleep(2000);
        var text  = App.WaitForElement("MauiLabel").GetText();
		Assert.That(text,Is.EqualTo("Unfocused"));
	}
}
