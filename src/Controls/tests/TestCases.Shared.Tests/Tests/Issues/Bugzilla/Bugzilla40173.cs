#if TEST_FAILS_ON_ANDROID //Issue reproduced on android and logged the issue: https://github.com/dotnet/maui/issues/26026
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40173 : _IssuesUITest
{
	public Bugzilla40173(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Android BoxView/Frame not clickthrough in ListView";

	[Fact]
	[Category(UITestCategories.InputTransparent)]
	public void ButtonBlocked()
	{
		App.WaitForElement("CantTouchButtonId");
		App.Tap("CantTouchButtonId");

		Assert.Equal(("Default", App.WaitForElement("outputlabel")?.GetText()));

		App.Tap("CanTouchButtonId");

		Assert.Equal(("ButtonTapped", App.WaitForElement("outputlabel")?.GetText()));

		App.WaitForElement("Foo");
		App.Tap("Foo");
		Assert.Equal(("ItemTapped", App.WaitForElement("outputlabel")?.GetText()));
	}
}
#endif