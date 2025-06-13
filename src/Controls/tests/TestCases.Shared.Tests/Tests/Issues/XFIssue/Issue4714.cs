using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4714 : _IssuesUITest
{
	const string InitialText = "ClickMeToIncrement";

	public Issue4714(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "SingleTapGesture called once on DoubleTap";

	[Fact]
	[Category(UITestCategories.Gestures)]
	public void Issue4714Test()
	{
		App.WaitForElement(InitialText);
		App.DoubleTap(InitialText);
		App.Tap(InitialText);
		App.Tap(InitialText);
		Assert.Equal("ClickMeToIncrement: 4", App.FindElement(InitialText).GetText());
	}
}