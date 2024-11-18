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

#if !MACCATALYST
	[Test]
	[FailsOnMacWhenRunningOnXamarinUITest("When the refreshview appears on catalyst. Appium starts to have a really hard time finding elements")]
	public void BindingUpdatesFromProgrammaticRefresh()
	{
		_ = App.WaitForElement("StartRefreshing");
		App.Tap("StartRefreshing");
		App.WaitForElement("IsRefreshing");
		App.Click("StopRefreshing");
		App.WaitForElement("IsNotRefreshing");
	}
#endif

	// Windows only works with touch inputs which we don't have running on the test server
#if !WINDOWS && !MACCATALYST
	[Test]
	public void BindingUpdatesFromInteractiveRefresh()
	{
		_ = App.WaitForElement("CollectionView");
		App.ScrollUp("CollectionView");
		App.WaitForElement("IsRefreshing");
		App.Tap("StopRefreshing");
		App.WaitForElement("IsNotRefreshing");
	}
#endif

}
