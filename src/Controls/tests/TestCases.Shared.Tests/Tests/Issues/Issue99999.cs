using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue99999 : _IssuesUITest
{
    public override string Issue => "Picker IsOpen property can be programmatically opened and closed";

    public Issue99999(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Picker)]
    public void PickerCanBeOpenedProgrammatically()
    {
        App.WaitForElement("TestPicker");
        App.WaitForElement("OpenPickerButton");
        App.WaitForElement("IsOpenLabel");

        var initialLabel = App.FindElement("IsOpenLabel").GetText();
        Assert.That(initialLabel, Is.EqualTo("IsOpen: False"));

        App.Tap("OpenPickerButton");

#if ANDROID
		    // On Android, picker is not dismissed by a single tap coordinate, so using two taps to dismiss
		    App.TapCoordinates(250, 250);
            App.TapCoordinates(250, 250);
#elif IOS || MACCATALYST
        // On iOS, tap Done button
        App.Tap("Done");
#endif

        // Verify IsOpen changed back to false
        App.WaitForElement("IsOpenLabel");
        var closedLabel = App.FindElement("IsOpenLabel").GetText();
        Assert.That(closedLabel, Is.EqualTo("IsOpen: False"));
    }
}
