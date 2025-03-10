using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.RefreshView)]
public class Issue16910 : _IssuesUITest
{
	public override string Issue => "IsRefreshing binding works";

	protected override bool ResetAfterEachTest => true;

	public Issue16910(TestDevice device)
		: base(device)
	{

	}

	[Test]
	public void BindingUpdatesFromProgrammaticRefresh()
	{
		_ = App.WaitForElement("StartRefreshing");
		App.Tap("StartRefreshing");
		App.WaitForElement("IsRefreshing");
		App.Tap("StopRefreshing");
		App.WaitForElement("IsNotRefreshing");
	}

	[Test]
	public void BindingUpdatesFromInteractiveRefresh()
	{
		const int offset = 50;

		var collectionViewRect = App.WaitForElement("CollectionView").GetRect();
		//In CI, using App.ScrollDown sometimes fails to trigger the refresh command, so here use DragCoordinates instead of the ScrollDown action in Appium.
		App.DragCoordinates(collectionViewRect.CenterX(), collectionViewRect.Y + offset, collectionViewRect.CenterX(), collectionViewRect.Y + collectionViewRect.Height - offset);
		App.WaitForElement("IsRefreshing");
		App.Tap("StopRefreshing");
		App.WaitForElement("IsNotRefreshing");
	}
}