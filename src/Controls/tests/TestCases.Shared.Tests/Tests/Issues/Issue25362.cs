using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25362 : _IssuesUITest
{
	public override string Issue => "[iOS] CollectionView Items display issue when Header is resized";

	public Issue25362(TestDevice device) : base(device)
	{ }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void HeaderShouldNotCollapseWithItems()
	{
		App.WaitForElement("button");
		App.Click("button");
		App.Click("button");
		App.Click("button");

		//The test passes of header has 4 elements that don't overlap with the collection view's items
		VerifyScreenshot();
	}
}
