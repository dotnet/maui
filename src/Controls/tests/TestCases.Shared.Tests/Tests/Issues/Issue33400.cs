#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // https://github.com/dotnet/maui/issues/7767#issuecomment-1792186959
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33400 : _IssuesUITest
{
	public Issue33400(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Runtime Scrollbar visibility not updating correctly on Android platform";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void Issue33400ImmediateScrollbarVisibility()
	{
		App.WaitForElement("Issue33400HorizontalNeverButton");
		App.Tap("Issue33400HorizontalNeverButton");
		VerifyScreenshot("Issue33400_Never");
		App.WaitForElement("Issue33400HorizontalAlwaysButton");
		App.Tap("Issue33400HorizontalAlwaysButton");
		VerifyScreenshot("Issue33400_Always");
		App.WaitForElement("Issue33400HorizontalDefaultButton");
		App.Tap("Issue33400HorizontalDefaultButton");
		VerifyScreenshot("Issue33400_Default");
	}
}
#endif