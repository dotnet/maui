using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22320 : _IssuesUITest
{
	public Issue22320(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView does not highlight selected grouped items correctly";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void SelectionShouldNotMovedToTopWithGroupedCollection()
	{
		App.WaitForElement("CollectionView");
		App.Tap("Grapes");
		App.Tap("Potato");
		VerifyScreenshot();
	}
}