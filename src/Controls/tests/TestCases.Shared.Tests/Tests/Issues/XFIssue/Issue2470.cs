using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2470 : _IssuesUITest
{
	const string Generate = "Generate";
	const string Results = "Results";

	public Issue2470(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ObservableCollection changes do not update ListView";

	[Test]
	[Category(UITestCategories.ListView)]
	public void OnservableCollectionChangeListView()
	{
		App.WaitForElement("Switch");
		// Tab 1
		App.Tap("Switch");
		App.TapTab(Results);

		// Tab 2
		App.WaitForElement("Entry 0 of 5");
		App.WaitForElement("Entry 1 of 5");
		App.WaitForElement("Entry 2 of 5");
		App.WaitForElement("Entry 3 of 5");
		App.WaitForElement("Entry 4 of 5");

		App.TapTab(Generate);

		// Tab 1
		App.Tap("Switch");
		App.TapTab(Results);

		// Tab 2
		App.WaitForElement("Entry 0 of 2");
		App.WaitForElement("Entry 1 of 2");


		// Tab 1
		App.TapTab(Generate);
		App.Tap("Switch");
		App.TapTab(Results);

		// Tab 2
		App.WaitForElement("Entry 0 of 5");
		App.WaitForElement("Entry 1 of 5");
		App.WaitForElement("Entry 2 of 5");
		App.WaitForElement("Entry 3 of 5");
		App.WaitForElement("Entry 4 of 5");
	}
}