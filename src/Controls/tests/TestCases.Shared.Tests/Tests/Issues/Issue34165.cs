#if ANDROID || IOS // ScrollRight with ScrollStrategy.Gesture is not supported on Windows and MacCatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.RefreshView)]
public class Issue34165 : _IssuesUITest
{
	public Issue34165(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "CollectionView is scrolling left/right when the collection is empty and inside a RefreshView";

	[Test]
	public void EmptyCollectionViewInsideRefreshViewShouldNotScrollHorizontally()
	{
		App.WaitForElement("CollectionView");

		var rectBefore = App.WaitForElement("EmptyViewLabel").GetRect();

		App.ScrollRight("CollectionView", ScrollStrategy.Gesture, swipePercentage: 0.8, swipeSpeed: 300);

		var rectAfter = App.WaitForElement("EmptyViewLabel").GetRect();

		Assert.That(rectAfter.X, Is.EqualTo(rectBefore.X).Within(1),
			$"EmptyViewLabel X position changed from {rectBefore.X} to {rectAfter.X}. " +
			"CollectionView must NOT scroll horizontally when empty inside a RefreshView.");
	}
}
#endif
