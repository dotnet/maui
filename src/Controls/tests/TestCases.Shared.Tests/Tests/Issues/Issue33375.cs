using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue33375 : _IssuesUITest
	{
		public Issue33375(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SwipeGestureRecognizer triggers while scrolling CollectionView horizontally on iOS";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void Issue33375Test()
		{
			App.WaitForElement("TestCollectionView");
		var statusLabel = App.WaitForElement("StatusLabel");
		Assert.That(statusLabel.GetText(), Is.EqualTo("No swipe detected"));
		var collectionView = App.FindElement("TestCollectionView");
		
		// Scroll right in the CollectionView
		App.DragCoordinates(
			collectionView.GetRect().CenterX(), 
			collectionView.GetRect().CenterY(),
			collectionView.GetRect().X + 50, 
			collectionView.GetRect().CenterY()
		);
		
		Task.Delay(1000).Wait();
		statusLabel = App.FindElement("StatusLabel");
		Assert.That(statusLabel.GetText(), Is.EqualTo("No swipe detected"), 
			"SwipeGestureRecognizer should not trigger when scrolling CollectionView");
		
		// Scroll back left to original position
		App.DragCoordinates(
			collectionView.GetRect().X + 50,
			collectionView.GetRect().CenterY(),
			collectionView.GetRect().CenterX(),
			collectionView.GetRect().CenterY()
		);
		
		Task.Delay(1000).Wait();
		statusLabel = App.FindElement("StatusLabel");
		Assert.That(statusLabel.GetText(), Is.EqualTo("No swipe detected"), 
			"SwipeGestureRecognizer should not trigger when scrolling back");
		}
	}
}
