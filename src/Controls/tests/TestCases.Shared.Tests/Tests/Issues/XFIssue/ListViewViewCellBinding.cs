using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.ListView)]
public class ListViewViewCellBinding : _IssuesUITest
{
	public ListViewViewCellBinding(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView ViewCell binding";

	//[Test]
	//[FailsOnIOS]
	//public void ListViewViewCellBindingTestsAllElementsPresent()
	//{
	//	RunningApp.WaitForElement(q => q.Marked("Remove"));
	//	RunningApp.WaitForElement(q => q.Marked("Add"));
	//	RunningApp.WaitForElement(q => q.Marked("1"));
	//	RunningApp.WaitForElement(q => q.Marked("$100.00"));
	//	RunningApp.WaitForElement(q => q.Marked("2"));
	//	RunningApp.WaitForElement(q => q.Marked("$200.00"));
	//	RunningApp.WaitForElement(q => q.Marked("3"));
	//	RunningApp.WaitForElement(q => q.Marked("$300.00"));

	//	RunningApp.Screenshot("All elements exist");
	//}

	//[Test]
	//[FailsOnIOS]
	//public void ListViewViewCellBindingTestsAddListItem()
	//{
	//	RunningApp.Tap(q => q.Button("Add"));
	//	RunningApp.WaitForElement(q => q.Marked("4"));
	//	RunningApp.WaitForElement(q => q.Marked("$400.00"));
	//	RunningApp.Screenshot("List item added");
	//}

	//[Test]
	//[FailsOnIOS]
	//public void ListViewViewCellBindingTestsRemoveListItem()
	//{
	//	RunningApp.Tap(q => q.Button("Remove"));
	//	RunningApp.WaitForNoElement(q => q.Marked("1"));
	//	RunningApp.WaitForNoElement(q => q.Marked("$100.00"));
	//	RunningApp.Screenshot("List item removed");
	//}
}