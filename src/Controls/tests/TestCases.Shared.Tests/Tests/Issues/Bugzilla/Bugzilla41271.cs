using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla41271 : _IssuesUITest
{
	public Bugzilla41271(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[UWP] Memory Leak from ListView in TabbedPage";

	string _cell = string.Empty;

	// [Test]
	// [Category(UITestCategories.ListView)]
	// public void MemoryLeakInListViewTabbedPageUWP()
	// {
	// 	_cell = "California #60";
	// 	for (int i = 1; i <= 10; i++)
	// 	{
	// 		ScrollListInPage($"List {i}");
	// 	}
	// }

	void ScrollListInPage(string tabName)
	{
		App.WaitForElement(tabName);
		App.Tap(tabName);
		App.ScrollDown(_cell, ScrollStrategy.Programmatically, 0.7);
		App.ScrollUp("California #1", ScrollStrategy.Programmatically, 0.7);
	}
}