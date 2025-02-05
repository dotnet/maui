#if TEST_FAILS_ON_CATALYST // TouchAndHold not supported on Mac, Also using LongPress is not applicable on this case, while LongPress does not open the context menu for Entry/Editor control.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8291 : _IssuesUITest
{
	public Issue8291(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Editor - Text selection menu does not appear when selecting text on an editor placed within a ScrollView";

	[Test]
	[Category(UITestCategories.Editor)]
	public void ContextMenuShowsUpWhenPressAndHoldTextOnEditorAndEntryField()
	{
		App.WaitForElement("PressEditor");
		App.TouchAndHold("PressEditor");

		TestForPopup();
		App.Tap("PressEntry");

		App.TouchAndHold("PressEditor");
		TestForPopup();
	}

	void TestForPopup()
	{
		var result = App.QueryUntilPresent(() =>
		{
			return "Paste"
					.Union("Share")
					.Union("Copy")
					.Union("Cut")
					.Union("Select All")
					.ToArray();
		});

		Assert.That(result, Is.Not.Null);
		Assert.That(result.Length, Is.GreaterThan(0));
	}
}
#endif