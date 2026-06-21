using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24734 : _IssuesUITest
{
	public Issue24734(TestDevice device) : base(device)
	{
	}

	public override string Issue => "TapGestureRecognizer ButtonMask always return 0";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void ButtonMaskShouldNotReturnZero()
	{
		App.WaitForElement("LabelwithGesture");
		App.Tap("LabelwithGesture");
		App.WaitForElement("Success");
	}
}
