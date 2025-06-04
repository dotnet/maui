using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22565 : _IssuesUITest
{
	public Issue22565(TestDevice device) : base(device)
	{
	}

	public override string Issue => "A disabled Picker prevents the parent container's GestureRecognizer from being triggered";

	[Test]
	[Category(UITestCategories.Picker)]
	public void VerifyGesturePropagationWithDisabledPicker()
	{
		App.WaitForElement("22565DescriptionLabel");
		App.Tap("DisabledPicker");
		Assert.That(App.FindElement("22565DescriptionLabel").GetText(), Is.EqualTo("Parent Gesture recognizer triggered"));
	}
}