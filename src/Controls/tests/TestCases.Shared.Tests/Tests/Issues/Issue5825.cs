using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5825 : _IssuesUITest
{
	public override string Issue => "[Android] TapGestureRecognizer doesn't fire inside CollectionView/ListView";

	public Issue5825(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Gestures)]
	public void DoubleTapInCollectionViewShouldFireCommand()
	{
		App.WaitForElement("CollectionViewItem");

		// Double-tap on a CollectionView item label
		App.DoubleTap("CollectionViewItem");

		// Verify the command fired
		App.WaitForTextToBePresentInElement("ResultLabel", "Success");
	}
}
