using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12345 : _IssuesUITest
{
    public Issue12345(TestDevice device) : base(device) { }
    public override string Issue => "Button with CharacterSpacing does not restore TextColor to platform default when reset to null";

    [Test]
    [Category(UITestCategories.Button)]
    public void ButtonWithCharacterSpacingRestoresDefaultTextColorAfterNull()
    {
        App.WaitForElement("SetCharacterSpacingButton");
        App.Tap("SetCharacterSpacingButton");
        VerifyScreenshot("ButtonWithCharacterSpacingAfterCharacterSpacingSet");
        App.Tap("ResetTextColorButton");
        VerifyScreenshot("ButtonWithCharacterSpacingAfterTextColorResetToNull");
    }
}
