using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue19111 : _IssuesUITest
{
	public Issue19111(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Buttons with DragGestureRecogniser doesn't fire default Command on Android";

    [Test]
	[Category(UITestCategories.Button)]
	public void ButtonShouldBeClickableWithDragGesture()
	{
		App.WaitForElement("MauiButton");
        App.Tap("MauiButton");

        var text1 = App.WaitForElement("ClickedLabel").GetText();
        var text2 = App.WaitForElement("CommandLabel").GetText();

        Assert.That(text1,Is.EqualTo("Clicked"));
        Assert.That(text2,Is.EqualTo("Command"));
	}
}
