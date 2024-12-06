using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7167 : _IssuesUITest
{
	public Issue7167(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] improved observablecollection. a lot of collectionchanges. a reset is sent and listview scrolls to the top";

	//[Test]
	//[Category(UITestCategories.ListView)]
	//const string ListViewId = "ListViewId";
	//const string AddCommandID = "AddCommandID";
	//const string ClearListCommandId = "ClearListCommandId";
	//const string AddRangeCommandId = "AddRangeCommandId";
	//const string AddRangeWithCleanCommandId = "AddRangeWithCleanCommandId";

	//public void Issue7167Test()
	//{
	//	// arrange
	//	// add items to the list and scroll down till item "25"
	//	App.Screenshot("Empty ListView");
	//	App.Tap(q => q.Button(AddRangeCommandId));
	//	App.Tap(q => q.Button(AddRangeCommandId));
	//	App.WaitForElement(c => c.Index(25).Property("Enabled", true));
	//	App.Print.Tree();
	//	App.ScrollDownTo(a => a.Marked("25").Property("text").Contains("25"),
	//		b => b.Marked(ListViewId), ScrollStrategy.Auto);
	//	App.WaitForElement(x => x.Marked("25"));

	//	// act
	//	// when adding additional items via a addrange and a CollectionChangedEventArgs.Action.Reset is sent
	//	// then the listview shouldnt reset or it should not scroll to the top
	//	App.Tap(q => q.Marked(AddRangeCommandId));

	//	// assert
	//	// assert that item "25" is still visible
	//	var result = App.Query(x => x.Marked(ListViewId).Child().Marked("25"));
	//	Assert.That(result?.Length <= 0);
	//}
}