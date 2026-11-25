#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Windows requires touch interaction; App.ScrollUp does not work on MacCatalyst.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37361 : _IssuesUITest
{
	public Issue37361(TestDevice device) : base(device)
	{
	}

	public override string Issue => "RefreshView pull-to-refresh does nothing when CollectionView is empty";

	[Test]
	[Category(UITestCategories.RefreshView)]
	public void PullToRefreshWorksWhenCollectionViewIsEmpty()
	{
		App.WaitForElement("EmptyViewLabel");

		App.ScrollUp("RefreshView");
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("Refreshes: 1"));

		App.Tap("AddItemButton");
		App.Tap("ClearItemsButton");
		App.WaitForElement("EmptyViewLabel");

		App.ScrollUp("RefreshView");
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("Refreshes: 2"));
	}
}
#endif