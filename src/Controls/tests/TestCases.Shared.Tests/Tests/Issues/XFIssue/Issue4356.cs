using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4356 : _IssuesUITest
{
	public Issue4356(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] NSInternalInconsistencyException thrown when adding item to ListView after clearing bound ObservableCollection";

	//[Test]
	//[Category(UITestCategories.ListView)]
	//[FailsOnIOS]
	//public void Issue4356Test()
	//{
	//	App.WaitForElement(q => q.Marked("Will this repo work?"));
	//	App.WaitForElement(q => q.Marked("Remove item"));
	//	App.Tap(q => q.Marked("Remove item"));
	//	App.Tap(q => q.Marked("Remove item"));
	//	App.Tap(q => q.Marked("Add item"));
	//	App.WaitForElement(q => q.Marked("Added from Button Command"));
	//}
}