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
	//	RunningApp.WaitForElement(q => q.Marked("Will this repo work?"));
	//	RunningApp.WaitForElement(q => q.Marked("Remove item"));
	//	RunningApp.Tap(q => q.Marked("Remove item"));
	//	RunningApp.Tap(q => q.Marked("Remove item"));
	//	RunningApp.Tap(q => q.Marked("Add item"));
	//	RunningApp.WaitForElement(q => q.Marked("Added from Button Command"));
	//}
}