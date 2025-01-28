using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla33870 : _IssuesUITest
{
	public Bugzilla33870(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[W] Crash when the ListView Selection is set to null";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla33870Test()
	{
		App.WaitForElement("ListViewAutomatedId");
		App.Tap("CLEAR SELECTION");

		App.WaitForElement("Cleared");
	}
}