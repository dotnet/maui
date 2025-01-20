using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3053 : _IssuesUITest
{
	public Issue3053(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Moving items around on an Observable Collection causes the last item to disappear";

	[Test]
	[Category(UITestCategories.ListView)]
	public void MovingItemInObservableCollectionBreaksListView()
	{
		App.WaitForElement("InstructionButton");
		App.Tap("InstructionButton");
		App.WaitForElement("Item 2");
	}
}