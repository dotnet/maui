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

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewResetTest()
	{
		App.WaitForElement("Reset");

		// Verify the item is there
		App.WaitForElement("cover1.jpg, 0");

		// Clear the collection
		App.Tap("Reset");

		// Verify the item is gone
		App.WaitForNoElement("cover1.jpg, 0");
	}
}