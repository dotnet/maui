using System.Threading.Tasks;
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
#if TEST_FAILS_ON_CATALYST //Scroll actions cannot be performed on the macOS test server
	[Test]
	public void BindingUpdatesFromInteractiveRefresh()
	{
		_ = App.WaitForElement("CollectionView");
		App.ScrollUp("CollectionView", ScrollStrategy.Gesture, 0.7, 500);
		App.WaitForElement("IsRefreshing");
		App.Tap("StopRefreshing");
		App.WaitForElement("IsNotRefreshing");
	}
#endif
}