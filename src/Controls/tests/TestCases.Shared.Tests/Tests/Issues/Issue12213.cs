using Microsoft.Maui.Controls;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12213 : _IssuesUITest
{
	public Issue12213(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Windows] TapGestureRecognizer not working on Entry";

	[Test]
	[Category(UITestCategories.Entry)]
	[Category(UITestCategories.Gestures)]
	public void TapGestureRecognizerNotWorkingOnEntry()
	{
		// The button is placed on a stack layout. Button is tapped but the stack layout itself must NOT be tapped.
		App.WaitForElement("Button");
		App.Tap("Button");

		// The entry should be tapped.
		App.WaitForElement("Entry");
		App.Tap("Entry");
		App.WaitForElement("EntryTapped");

		ClassicAssert.Null(App.FindElement("StackLayoutTapped"));
	}
}