using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

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
		App.WaitForElement("FocusButton");
        App.Tap("FocusButton");
		App.WaitForTextToBePresentInElement("MauiLabel", "Focused");
	}

    [Test]
	[Category(UITestCategories.Shell)]
	public void UnfocusCallShouldWorkOnSearchHandler()
	{
		App.WaitForElement("UnfocusButton");
        App.Tap("UnfocusButton");
		App.WaitForTextToBePresentInElement("MauiLabel", "Unfocused");
	}
}
