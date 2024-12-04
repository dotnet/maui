using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2470 : _IssuesUITest
{
#if ANDROID
	const string Generate = "GENERATE";
	const string Results = "RESULTS";
#else
	const string Generate = "Generate";
	const string Results = "Results";
#endif
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
		App.Tap(Results);

		// Tab 2
		App.WaitForElement("Entry 0 of 5");
		App.WaitForElement("Entry 1 of 5");
		App.WaitForElement("Entry 2 of 5");
		App.WaitForElement("Entry 3 of 5");
		App.WaitForElement("Entry 4 of 5");

		App.Tap(Generate);

		// Tab 1
		App.Tap("Switch");
		App.Tap(Results);

		// Tab 2
		App.WaitForElement("Entry 0 of 2");
		App.WaitForElement("Entry 1 of 2");


		// Tab 1
		App.Tap(Generate);
		App.Tap("Switch");
		App.Tap(Results);

		// Tab 2
		App.WaitForElement("Entry 0 of 5");
		App.WaitForElement("Entry 1 of 5");
		App.WaitForElement("Entry 2 of 5");
		App.WaitForElement("Entry 3 of 5");
		App.WaitForElement("Entry 4 of 5");
	}
}