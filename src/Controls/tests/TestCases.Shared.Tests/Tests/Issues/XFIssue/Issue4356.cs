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

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue4356Test()
	{
		App.WaitForElement("Will this repo work?");
		App.Tap("RemoveItem");
		App.Tap("RemoveItem");
		App.Tap("AddItem");
		App.WaitForElement("Added from Button Command");
	}
}