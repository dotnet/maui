using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3524 : _IssuesUITest
{
	const string kText = "Click Me To Increment";

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
#if IOS
		Assert.That(App.WaitForElement(kText).GetText(), Is.EqualTo("Click Me To Increment: 1"));
#else
		App.WaitForElement($"{kText}: 1");
#endif
	}
}