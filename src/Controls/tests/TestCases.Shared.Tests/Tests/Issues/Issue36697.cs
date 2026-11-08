using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36697 : _IssuesUITest
{
    public Issue36697(TestDevice device) : base(device) { }
    public override string Issue => "Button with CharacterSpacing does not restore TextColor to platform default when reset to null";

    [Test]
    [Category(UITestCategories.Button)]
    public void ButtonWithCharacterSpacingRestoresDefaultTextColorAfterNull()
    {
        Exception? exception = null;

        App.WaitForElement("SetCharacterSpacingButton");
        App.Tap("SetCharacterSpacingButton");
        App.Tap("SetTextColorButton");
        VerifyScreenshotOrSetException(ref exception, "ButtonWithCharacterSpacingAfterTextColorSet");
        App.Tap("ResetTextColorButton");
        VerifyScreenshotOrSetException(ref exception, "ButtonWithCharacterSpacingAfterTextColorResetToNull");

        if (exception is not null)
        {
            throw exception;
        }
    }
}
