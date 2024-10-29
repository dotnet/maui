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
	//	App.WaitForElement(q => q.Marked("Remove"));
	//	App.WaitForElement(q => q.Marked("Add"));
	//	App.WaitForElement(q => q.Marked("1"));
	//	App.WaitForElement(q => q.Marked("$100.00"));
	//	App.WaitForElement(q => q.Marked("2"));
	//	App.WaitForElement(q => q.Marked("$200.00"));
	//	App.WaitForElement(q => q.Marked("3"));
	//	App.WaitForElement(q => q.Marked("$300.00"));

	//	App.Screenshot("All elements exist");
	//}

	//[Test]
	//[FailsOnIOS]
	//public void ListViewViewCellBindingTestsAddListItem()
	//{
	//	App.Tap(q => q.Button("Add"));
	//	App.WaitForElement(q => q.Marked("4"));
	//	App.WaitForElement(q => q.Marked("$400.00"));
	//	App.Screenshot("List item added");
	//}

	//[Test]
	//[FailsOnIOS]
	//public void ListViewViewCellBindingTestsRemoveListItem()
	//{
	//	App.Tap(q => q.Button("Remove"));
	//	App.WaitForNoElement(q => q.Marked("1"));
	//	App.WaitForNoElement(q => q.Marked("$100.00"));
	//	App.Screenshot("List item removed");
	//}
}