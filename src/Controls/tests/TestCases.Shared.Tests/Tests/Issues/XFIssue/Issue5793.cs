using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5793 : _IssuesUITest
{
	public Issue5793(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[CollectionView/ListView] Not listening for Reset command";

	// TODO: There is some old ControlGallery specific thing going on in the HostApp UI for this test. See how we should change that.
	//[Test]
	//[Category(UITestCategories.CollectionView)]
	//public void Reset()
	//{
	//	RunningApp.WaitForElement("Reset");

	//	// Verify the item is there
	//	RunningApp.WaitForElement("cover1.jpg, 0");

	//	// Clear the collection
	//	RunningApp.Tap("Reset");

	//	// Verify the item is gone
	//	RunningApp.WaitForNoElement("cover1.jpg, 0");
	//}
}