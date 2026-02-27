#if TEST_FAILS_ON_WINDOWS //The AutomationId for SwipeView items does not function as expected on the Windows platform. Additionally, programmatic swiping is currently not working. For reference:  https://github.com/dotnet/maui/issues/14777.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29086 : _IssuesUITest
{

	public Issue29086(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "SwipeView Closes when Content Changes even with SwipeBehaviorOnInvoked is set to RemainOpen";

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeViewShouldNotClose()
	{
		App.WaitForElement("SwipeItem1");
		App.SwipeLeftToRight("SwipeItem1");
		App.SwipeRightToLeft("SwipeItem2");
		App.WaitForElement("AddButton1");
		App.WaitForElement("AddButtonRight2");
		App.Click("AddButton1");
		App.Click("AddButtonRight2");
		App.WaitForNoElement("AddButton1");
		App.WaitForNoElement("AddButtonRight2");
	}
}
#endif