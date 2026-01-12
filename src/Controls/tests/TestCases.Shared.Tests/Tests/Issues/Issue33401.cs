using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33401 : _IssuesUITest
{
	public Issue33401(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView's SelectionChanged isn't fired on iOS when it's inside a grid with TapGestureRecognizer";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewSelectionShouldFireWithParentTapGesture()
	{
		App.WaitForElement("TestCollectionView");
		
		// Tap the first item in the CollectionView
		var collectionView = App.WaitForElement("TestCollectionView");
		var rect = collectionView.GetRect();
		
		// Tap in the middle of the first item (approximately at y-offset 30 from top)
		App.TapCoordinates(rect.X + rect.Width / 2, rect.Y + 30);

		// Wait a moment for the event to fire
		Task.Delay(500).Wait();

		// Check the status label to verify SelectionChanged fired
		var statusText = App.FindElement("StatusLabel").GetText();
		
		Assert.That(statusText, Does.Contain("SelectionChanged: 1 times"));
		Assert.That(statusText, Does.Contain("Test 1"));
	}
}
