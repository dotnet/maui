#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // In iOS and Catalyst, WaitForNoElement throws a timeout exception eventhough the text is not visible on the screen by scrolling.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3840 : _IssuesUITest
{
	const string FailedText = "Test Failed if Visible";
	const string FirstButton = "FirstClick";
	const string SecondButton = "SecondClick";
	public Issue3840(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Translation change causes ScrollView to reset to initial position (0, 0)";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void TranslatingViewKeepsScrollViewPosition()
	{
		App.WaitForElement(FailedText);
		App.Tap(FirstButton);
		App.WaitForElement(SecondButton);
		App.Tap(SecondButton);
		App.WaitForNoElement(FailedText);
	}
}
#endif