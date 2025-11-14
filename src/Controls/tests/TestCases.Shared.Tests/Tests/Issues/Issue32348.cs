using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32348 : _IssuesUITest
{
	public Issue32348(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "CollectionView VSM Background and BackgroundColor work similarly";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void TableViewTextCellCommand()
	{
		App.WaitForElement("Item 2");
		App.Tap("Item 2");
		App.WaitForElement("Item 5");
		App.Tap("Item 5");
		VerifyScreenshot();
	}
}