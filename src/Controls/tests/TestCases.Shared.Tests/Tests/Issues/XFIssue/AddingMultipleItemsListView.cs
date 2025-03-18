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
		App.FindElement("Smaller Job");
		App.FindElement("Add On Job");
		App.FindElement("Add One");
		App.FindElement("Add Two");
		App.FindElement("3672");
		App.FindElement("6289");
		App.FindElement("3672-41");
		App.FindElement("2");
		App.FindElement("2");
		App.FindElement("23");
	}

	[Test]
	public void AddingMultipleListViewTests2AddOneElementToList()
	{
		App.WaitForElement("Add One");
		App.Tap("Add One");

		App.WaitForElement("1234", timeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	public void AddingMultipleListViewTests3AddTwoElementToList()
	{
		App.WaitForElement("Add Two");
		App.Tap("Add Two");

		App.WaitForElement("9999", timeout: TimeSpan.FromSeconds(2));
		App.FindElement("8888");
	}
}