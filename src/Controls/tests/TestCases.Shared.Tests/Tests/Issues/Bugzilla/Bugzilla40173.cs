#if TEST_FAILS_ON_ANDROID //Issue reproduced on android and logged the issue: https://github.com/dotnet/maui/issues/26026
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40173 : _IssuesUITest
{
	public Bugzilla40173(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Android BoxView/Frame not clickthrough in ListView";

	[Test]
	[Category(UITestCategories.InputTransparent)]
	public void ButtonBlocked()
	{
		App.WaitForElement("CantTouchButtonId");
		App.Tap("CantTouchButtonId");

		Assert.That(App.WaitForElement("outputlabel")?.GetText(), Is.EqualTo(("Default")));

		App.Tap("CanTouchButtonId");

		Assert.That(App.WaitForElement("outputlabel")?.GetText(), Is.EqualTo(("ButtonTapped")));

		App.WaitForElement("Foo");
		App.Tap("Foo");
		Assert.That(App.WaitForElement("outputlabel")?.GetText(), Is.EqualTo(("ItemTapped")));
	}
}
#endif