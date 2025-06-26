using Microsoft.Maui.Controls;
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12213 : _IssuesUITest
{
	public Issue12213(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Windows] TapGestureRecognizer not working on Entry";

	[Fact]
	[Trait("Category", UITestCategories.Entry)]
	[Trait("Category", UITestCategories.Gestures)]
	public void TapGestureRecognizerNotWorkingOnEntry()
	{
		// The button is placed on a stack layout. Button is tapped but the stack layout itself must NOT be tapped.
		App.WaitForElement("Button");
		App.Tap("Button");

		// The entry should be tapped.
		App.WaitForElement("Entry");
		App.Tap("Entry");
		App.WaitForElement("EntryTapped");

		Assert.Null(App.FindElement("StackLayoutTapped"));
	}
}