using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5831 : _IssuesUITest
{
	const string flyoutMainTitle = "Main";
	const string flyoutOtherTitle = "Other Page";
	public Issue5831(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Navigating away from CollectionView and coming back leaves weird old items";

	[Test]
	[Category(UITestCategories.Shell)]
	public void CollectionViewRenderingWhenLeavingAndReturningViaFlyout()
	{
		App.TapInShellFlyout(flyoutOtherTitle);
		App.WaitForElementTillPageNavigationSettled("Label");
		App.TapInShellFlyout(flyoutMainTitle);
		App.WaitForElement("Baboon");
		App.WaitForElement("Capuchin Monkey");
		App.WaitForElement("Blue Monkey");
		App.WaitForElement("Squirrel Monkey");
		App.WaitForElement("Golden Lion Tamarin");
		App.WaitForElement("Howler Monkey");
		App.WaitForElement("Japanese Macaque");
	}
}
