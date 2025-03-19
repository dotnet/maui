using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue27831 : _IssuesUITest
{
	public Issue27831(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Adding GestureRecognizer To Editor prevents focus on Android & iOS";

    [Test]
	[Category(UITestCategories.Editor)]
	public void EditorShouldGainFocusWithGesture()
	{
		App.WaitForElement("MauiEditor");
        App.Tap("MauiEditor");

        var text = App.WaitForElement("MauiLabel").GetText();

		Thread.Sleep(1000);
        Assert.That(App.IsKeyboardShown(),Is.EqualTo(true));
        Assert.That(text,Is.EqualTo("Tapped"));
        
	}
}
