using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue27992 : _IssuesUITest
{
	public Issue27992(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Entry Completed Event Triggered Twice";


	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryCompletedShouldOnlyFireOnce()
	{
		App.WaitForElement("MauiEntry");

		App.Tap("MauiEntry");

		App.PressEnter();

		var text = App.WaitForElement("MauiLabel").GetText();
		Assert.That(text, Is.EqualTo("Completed Invoked 1 times"));
	}
}
