using System;
using System.Globalization;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34396 : _IssuesUITest
{
	public Issue34396(TestDevice device) : base(device) { }

	public override string Issue => "[iOS, MacCatalyst] UI freezes when adding a large number of Editors to a layout";

	[Test]
	[Category(UITestCategories.Editor)]
	public void AddingManyEditorsDoesNotFreezeUI()
	{
		App.WaitForElement("AddEditorsButton");

		App.Tap("AddEditorsButton");

		App.WaitForTextToBePresentInElement("StatusLabel34396", "Completed:", TimeSpan.FromSeconds(60));

		var status = App.FindElement("StatusLabel34396").GetText() ?? string.Empty;
		var elapsedMs = long.Parse(status.Replace("Completed:", string.Empty, StringComparison.Ordinal).Trim(), CultureInfo.InvariantCulture);

		Assert.That(elapsedMs, Is.LessThan(10000),
			$"Adding 200 editors took {elapsedMs} ms on the UI thread. The freeze from #34396 has regressed (expected well under 10000 ms).");

		App.Tap("ClickedButton");
		Assert.That(App.FindElement("ClickedButton").GetText(), Is.EqualTo("Clicked 1"),
			"The button should respond after adding 200 editors, proving the UI is not frozen.");
	}
}
