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

	[Test]

	public void ListViewViewCellBindingTestsAllElementsPresent()
	{
		App.WaitForElement("Remove");
		App.WaitForElement("Add");
		App.WaitForElement("1");
		App.WaitForElement("100.0");
		App.WaitForElement("2");
		App.WaitForElement("200.0");
		App.WaitForElement("3");
		App.WaitForElement("300.0");
	}

	[Test]

	public void ListViewViewCellBindingTestsAddListItem()
	{
		App.Tap("Add");
		App.WaitForElement("4");
		App.WaitForElement("400.0");
	}

	[Test]

	public void ListViewViewCellBindingTestsRemoveListItem()
	{
		App.Tap("Remove");
		App.WaitForNoElement("1");
		App.WaitForNoElement("100.0");
	}
}