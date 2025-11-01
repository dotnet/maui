using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28716 : _IssuesUITest
{
	public Issue28716(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Support for KeepLastItemInView for CV2";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void KeepLastItemInViewShouldWork()
	{
		App.WaitForElement("AddItemButton");
		App.Click("AddItemButton");
		App.WaitForElement("Item20cv1");
		App.WaitForElement("Item20cv2");
		App.WaitForElement("Item20cv3");
	}
}