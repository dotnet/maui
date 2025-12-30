using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32885 : _IssuesUITest
{
	public Issue32885(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] Text Color of the Entry, TimePicker and SearchBar is not properly worked on dynamic Scenarios";

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryTextColorTogglesBetweenGreenAndNull()
	{
		App.WaitForElement("TestButton");
		//Change text color to green
		App.Tap("TestButton");
		//Change text color to null, system default color
		App.Tap("TestButton");
		VerifyScreenshot();
	}
}
