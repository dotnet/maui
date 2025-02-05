using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3524 : _IssuesUITest
{
	const string kText = "ClickMeToIncrement";
	public Issue3524(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ICommand binding from a TapGestureRecognizer on a Span doesn't work";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void SpanGestureCommand()
	{
		App.WaitForElement(kText);
		App.Tap(kText);
		Assert.That(App.WaitForElement(kText).GetText(), Is.EqualTo("ClickMeToIncrement: 1"));
	}
}