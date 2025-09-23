using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31465 : _IssuesUITest
{
	public Issue31465(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "CollectionView with null ItemsSource and Header can be dragged down creating extra space";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewWithNullBindingContextShouldDisplayHeaderAndEmptyViewCorrectly()
	{
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("HeaderLabel");
		App.WaitForElement("EmptyViewLabel");

		var headerLabel = App.FindElement("HeaderLabel");
		Assert.That(headerLabel.GetText(), Is.EqualTo("Header: This should show for all cases."));

		var emptyViewLabel = App.FindElement("EmptyViewLabel");
		Assert.That(emptyViewLabel.GetText(), Is.EqualTo("EmptyView: This should show when no data."));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewShouldNotBeDraggableWhenEmpty()
	{
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("HeaderLabel");
		App.WaitForElement("EmptyViewLabel");

		var collectionView = App.FindElement("TestCollectionView");
		var headerLabel = App.FindElement("HeaderLabel");
		var emptyViewLabel = App.FindElement("EmptyViewLabel");

		var initialHeaderRect = headerLabel.GetRect();
		var initialEmptyViewRect = emptyViewLabel.GetRect();

		Assert.That(headerLabel.GetText(), Is.EqualTo("Header: This should show for all cases."));
		Assert.That(emptyViewLabel.GetText(), Is.EqualTo("EmptyView: This should show when no data."));

		var collectionViewRect = collectionView.GetRect();
		var startX = collectionViewRect.X + collectionViewRect.Width / 2;
		var startY = collectionViewRect.Y + 50;
		var endY = startY + 200;

		App.DragCoordinates(startX, startY, startX, endY);

		Thread.Sleep(500);

		App.WaitForElement("HeaderLabel");
		App.WaitForElement("EmptyViewLabel");

		var afterDragHeaderLabel = App.FindElement("HeaderLabel");
		var afterDragEmptyViewLabel = App.FindElement("EmptyViewLabel");

		Assert.That(afterDragHeaderLabel.GetText(), Is.EqualTo("Header: This should show for all cases."));
		Assert.That(afterDragEmptyViewLabel.GetText(), Is.EqualTo("EmptyView: This should show when no data."));

		var afterDragHeaderRect = afterDragHeaderLabel.GetRect();
		var afterDragEmptyViewRect = afterDragEmptyViewLabel.GetRect();

		var headerYDifference = Math.Abs(afterDragHeaderRect.Y - initialHeaderRect.Y);
		var emptyViewYDifference = Math.Abs(afterDragEmptyViewRect.Y - initialEmptyViewRect.Y);

		Assert.That(headerYDifference, Is.LessThan(5),
			$"Header moved too much: initial Y={initialHeaderRect.Y}, after drag Y={afterDragHeaderRect.Y}");
		Assert.That(emptyViewYDifference, Is.LessThan(5),
			$"EmptyView moved too much: initial Y={initialEmptyViewRect.Y}, after drag Y={afterDragEmptyViewRect.Y}");

		var spaceBetween = afterDragEmptyViewRect.Y - (afterDragHeaderRect.Y + afterDragHeaderRect.Height);
		Assert.That(spaceBetween, Is.LessThan(50),
			$"Too much space between Header and EmptyView: {spaceBetween}px");
	}
}