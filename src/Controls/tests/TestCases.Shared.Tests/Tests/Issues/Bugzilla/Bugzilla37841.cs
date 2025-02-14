using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla37841 : _IssuesUITest
{
	const string Generate = "Generate";
	const string entrycell = "entrycell";
	const string textcell = "textcell";
	public Bugzilla37841(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TableView EntryCells and TextCells cease to update after focus change";

	[Test]
	[Category(UITestCategories.TableView)]
	public void TextAndEntryCellsDataBindInTableView()
	{
		App.WaitForElement(Generate);
		App.Tap(Generate);

		App.WaitForTextToBePresentInElement(entrycell, "12345");
		App.WaitForTextToBePresentInElement(textcell, "6789");
		App.Tap(Generate);

		App.WaitForTextToBePresentInElement(entrycell, "112358");
		App.WaitForTextToBePresentInElement(textcell, "48151623");
	}
}