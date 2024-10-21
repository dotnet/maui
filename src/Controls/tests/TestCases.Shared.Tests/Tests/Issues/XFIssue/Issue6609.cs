using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6609 : _IssuesUITest
{
	public Issue6609(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug, CollectionView] SelectionChangedCommand invoked before SelectedItem is set";

	// TODO: There is some old ControlGallery specific thing going on in the HostApp UI for this test. See how we should change that.
	//[Test]
	//[Category(UITestCategories.CollectionView)]
	//public void SelectionChangedCommandParameterBoundToSelectedItemShouldMatchSelectedItem()
	//{
	//	RunningApp.WaitForElement("Item 2");
	//	RunningApp.Tap("Item 2");

	//	RunningApp.WaitForElement("Success");
	//}
}