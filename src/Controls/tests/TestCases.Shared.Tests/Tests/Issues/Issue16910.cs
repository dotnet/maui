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
		App.WaitForElement("RunTest", timeout: TimeSpan.FromSeconds(45));
		App.Tap("RunTest");
		var result = App.WaitForElement("TestResult", timeout: TimeSpan.FromSeconds(45)).GetText();
		Assert.That(result, Is.EqualTo("SUCCESS"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Failing on Mac and Windows. Flaky Test. More information: https://github.com/dotnet/maui/issues/28368
	[Test]
	public void BindingUpdatesFromInteractiveRefresh()
	{
		var scrollViewRect = App.WaitForElement("RefreshScrollView", timeout: TimeSpan.FromSeconds(45)).GetRect();
		//In CI, using App.ScrollDown sometimes fails to trigger the refresh command, so here use DragCoordinates instead of the ScrollDown action in Appium.
		App.DragCoordinates(scrollViewRect.CenterX(), scrollViewRect.Y + 50, scrollViewRect.CenterX(), scrollViewRect.Y + scrollViewRect.Height - 50);
		App.WaitForElement("IsRefreshing", timeout: TimeSpan.FromSeconds(45));
		App.Tap("StopRefreshing");
		App.WaitForElement("IsNotRefreshing", timeout: TimeSpan.FromSeconds(45));
	}
#endif
}