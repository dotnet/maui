using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue5749 : _IssuesUITest
{
	public Issue5749(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Disable horizontal scroll in the custom listview in android";

	[Test]
	[Category(UITestCategories.ListView)]
	public void DisableScrollingOnCustomHorizontalListView()
	{
		App.WaitForElement("Button");
		App.WaitForElement("True");
		App.Tap("Button");
		App.WaitForElement("False");
	}
}