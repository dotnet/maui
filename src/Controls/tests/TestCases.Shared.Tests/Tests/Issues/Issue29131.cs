using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29131 : _IssuesUITest
{
	public Issue29131(TestDevice device) : base(device) { }

	public override string Issue => "Android - KeepScrollOffset doesn't not works as expected when new items are added in CollectionView";
	const string AddNewItem = "AddNewItem";
	const string ScrollButton = "ScrollButton";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void KeepScrollOffSetShouldWork()
	{
		App.WaitForElement("CollectionView");
		App.Click("KeepScrollOffsetButton");
		App.Click(ScrollButton);
		App.Click(AddNewItem);
		App.WaitForElement("Item 30");
		App.Click(ScrollButton);
		App.Click(AddNewItem);
		App.WaitForElement("Item 32");
		App.Click(ScrollButton);
		App.Click(AddNewItem);
		App.WaitForElement("Item 30");
	}
}