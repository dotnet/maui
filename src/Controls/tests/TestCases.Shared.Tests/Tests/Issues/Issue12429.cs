using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12429 : _IssuesUITest
{
	public override string Issue => "[iOS] CollectionView Items display issue when Header is resized";

	public Issue12429(TestDevice device) : base(device)
	{ }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public async Task HeaderShouldNotCollapseWithItems()
	{
		App.WaitForElement("button");
		App.Click("button");
		App.Click("button");
		App.Click("button");

		await Task.Delay(500);

		//The test passes of header has 4 elements that don't overlap with the collection view's items
		VerifyScreenshot();
	}
}
