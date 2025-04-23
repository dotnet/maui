using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla32801 : _IssuesUITest
{
	const string Tab1 = "Tab 1";
	const string FirstTabAddButton = "FirstTabAddButton";
	const string Level2AddButton = "Level2AddButton";
	const string Level3StackButton = "Level3StackButton";
	const string SecondTabStackButton = "SecondTabStackButton";


	public Bugzilla32801(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Memory Leak in TabbedPage + NavigationPage";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void Bugzilla32801Test()
	{
		App.WaitForElement(FirstTabAddButton);
		App.Tap(FirstTabAddButton);
		App.WaitForElementTillPageNavigationSettled(Level2AddButton);
		App.Tap(Level2AddButton);
		App.WaitForElementTillPageNavigationSettled(Level3StackButton);
		App.Tap(Level3StackButton);
		App.WaitForElement("Stack 3");
		App.TapTab(Tab1);
		App.WaitForElementTillPageNavigationSettled(SecondTabStackButton);
		App.Tap(SecondTabStackButton);
		App.WaitForElement("Stack 1");
	}
}