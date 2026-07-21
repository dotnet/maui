using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19099 : _IssuesUITest
{
	public Issue19099(TestDevice device) : base(device)
	{
	}

	public override string Issue => "TapGestureRecognizer no longer works on Button";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapGestureRecognizerOnButtonInvokesCommand()
	{
		App.WaitForElement("GestureButton");
		App.Tap("GestureButton");

		var resultText = App.WaitForElement("GestureResultLabel").GetText();
		Assert.That(resultText, Is.EqualTo("Tapped"), "The TapGestureRecognizer command on the Button did not run.");
	}
}
