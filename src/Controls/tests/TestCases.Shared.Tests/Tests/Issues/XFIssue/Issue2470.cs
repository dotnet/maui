using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2470 : _IssuesUITest
{
	public Issue2470(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ObservableCollection changes do not update ListView";

	//[Test]
	//[Category(UITestCategories.ListView)]
	//public void OnservableCollectionChangeListView()
	//{
	//	// Tab 1
	//	RunningApp.Tap(q => q.Marked("Switch"));
	//	RunningApp.Screenshot("Switch On");
	//	RunningApp.Tap(q => q.Marked("Results"));

	//	// Tab 2
	//	RunningApp.WaitForElement(q => q.Marked("Entry 0 of 5"));
	//	RunningApp.WaitForElement(q => q.Marked("Entry 1 of 5"));
	//	RunningApp.WaitForElement(q => q.Marked("Entry 2 of 5"));
	//	RunningApp.WaitForElement(q => q.Marked("Entry 3 of 5"));
	//	RunningApp.WaitForElement(q => q.Marked("Entry 4 of 5"));
	//	RunningApp.Screenshot("Should be 5 elements");
	//	RunningApp.Tap(q => q.Marked("Generate"));

	//	// Tab 1
	//	RunningApp.Tap(q => q.Marked("Switch"));
	//	RunningApp.Screenshot("Switch Off");
	//	RunningApp.Tap(q => q.Marked("Results"));

	//	// Tab 2
	//	RunningApp.WaitForElement(q => q.Marked("Entry 0 of 2"));
	//	RunningApp.WaitForElement(q => q.Marked("Entry 1 of 2"));
	//	RunningApp.Screenshot("Should be 2 elements");

	//	// Tab 1
	//	RunningApp.Tap(q => q.Marked("Generate"));
	//	RunningApp.Tap(q => q.Marked("Switch"));
	//	RunningApp.Screenshot("Switch On");
	//	RunningApp.Tap(q => q.Marked("Results"));

	//	// Tab 2
	//	RunningApp.WaitForElement(q => q.Marked("Entry 0 of 5"));
	//	RunningApp.WaitForElement(q => q.Marked("Entry 1 of 5"));
	//	RunningApp.WaitForElement(q => q.Marked("Entry 2 of 5"));
	//	RunningApp.WaitForElement(q => q.Marked("Entry 3 of 5"));
	//	RunningApp.WaitForElement(q => q.Marked("Entry 4 of 5"));
	//	RunningApp.Screenshot("Should be 5 elements");
	//}
}