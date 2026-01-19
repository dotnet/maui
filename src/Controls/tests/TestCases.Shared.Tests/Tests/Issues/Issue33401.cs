#if TEST_FAILS_ON_WINDOWS // CollectionView SelectionChanged event and parent TapGestureRecognizer conflict on windows. This needs to fix it.
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
	[Category(UITestCategories.Gestures)]
	public void CollectionViewSelectionShouldFireWithParentTapGesture()
	{
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("Item 1");
		App.Tap("Item 1");
		var statusText = App.FindElement("StatusLabel").GetText();
		Assert.That(statusText, Does.Contain("Status: Selected Item 1"));
	}
}
#endif