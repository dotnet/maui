using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue19289 : _IssuesUITest
{
	public Issue19289(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Android] Adding a PointerGestureRecognizer to a button on Android stops the button from working";

    [Test]
	[Category(UITestCategories.Button)]
	public void ButtonWithPointGestureShouldWork()
	{
		App.WaitForElement("MauiButton");
		App.Tap("MauiButton");

		var text1 = App.WaitForElement("MauiLabel").GetText();

        Assert.That(text1,Is.EqualTo("Button Works"));
	}
}
