#if TEST_FAILS_ON_WINDOWS //Related issue for windows: https://github.com/dotnet/maui/issues/35035
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34898 : _IssuesUITest
{
    public Issue34898(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Shell.Items.Clear does not disconnect handlers correctly";

    [Test]

    [Category(UITestCategories.Shell)]
    public void ShellItemsClearShouldDisconnectChildHandlers()
    {
        App.WaitForElement("ClearAndNavigateButton");
        App.Tap("ClearAndNavigateButton");

        App.WaitForElement("CheckHandlersButton");
        App.Tap("CheckHandlersButton");

        Assert.That(App.WaitForElement("PageHandlerStatus").GetText(), Is.EqualTo("Disconnected"),
            "Page handler should be disconnected after Shell.Items.Clear()");

        Assert.That(App.WaitForElement("LabelHandlerStatus").GetText(), Is.EqualTo("Disconnected"),
            "Label handler should be disconnected after Shell.Items.Clear()");

        Assert.That(App.WaitForElement("EntryHandlerStatus").GetText(), Is.EqualTo("Disconnected"),
            "Entry handler should be disconnected after Shell.Items.Clear()");

        Assert.That(App.WaitForElement("ButtonHandlerStatus").GetText(), Is.EqualTo("Disconnected"),
            "Button handler should be disconnected after Shell.Items.Clear()");
    }
}
#endif
