using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.ListView)]
public class AddingMultipleItemsListView : _IssuesUITest
{
	public AddingMultipleItemsListView(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Adding Multiple Items to a ListView";

	// [Test]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void AddingMultipleListViewTests1AllElementsPresent()
	// {
	//     App.WaitForElement("Big Job");
	//     App.WaitForElement("Smaller Job");
	//     App.WaitForElement("Add On Job");
	//     App.WaitForElement("Add One");
	//     App.WaitForElement("Add Two");
	//     App.WaitForElement("3672");
	//     App.WaitForElement("6289");
	//     App.WaitForElement("3672-41");
	//     App.WaitForElement("2");
	//     App.WaitForElement("2");
	//     App.WaitForElement("23");
	//     App.Screenshot("All elements are present");
	// }

	// [Test]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void AddingMultipleListViewTests2AddOneElementToList()
	// {
	//     App.Tap("Add One");

	//     App.WaitForElement("1234", timeout: TimeSpan.FromSeconds(2));
	//     App.Screenshot("One more element exists");
	// }

	// [Test]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void AddingMultipleListViewTests3AddTwoElementToList()
	// {
	//     App.Screenshot("Click 'Add Two'");
	//     App.Tap("Add Two");

	//     App.WaitForElement("9999", timeout: TimeSpan.FromSeconds(2));
	//     App.WaitForElement("8888", timeout: TimeSpan.FromSeconds(2));
	//     App.Screenshot("Two more element exist");
	// }
}