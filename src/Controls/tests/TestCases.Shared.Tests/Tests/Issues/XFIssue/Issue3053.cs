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

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// [FailsOnAndroid]
	// public void MovingItemInObservableCollectionBreaksListView()
	// {
	// 	App.WaitForElement(_instructions);
	// 	App.Tap(_instructions);
	// 	App.WaitForElement("Item 2");
	// }
}