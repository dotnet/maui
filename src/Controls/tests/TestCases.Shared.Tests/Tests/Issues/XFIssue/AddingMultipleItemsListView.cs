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

	[Test]
	public void AddingMultipleListViewTests1AllElementsPresent()
	{
		App.WaitForElement("Big Job");
		App.Screenshot("All elements are present");
	}

	[Test]
	public void AddingMultipleListViewTests2AddOneElementToList()
	{
		App.Tap("Add One");

		App.WaitForElement("1234");
		App.Screenshot("One more element exists");
	}

	[Test]
	public void AddingMultipleListViewTests3AddTwoElementToList()
	{
		App.Screenshot("Click 'Add Two'");
		App.Tap("Add Two");

		App.WaitForElement("9999");
		App.WaitForElement("8888");
		App.Screenshot("Two more element exist");
	}
}