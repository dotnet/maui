#if ANDROID || IOS  // SafeAreaEdges not supported on Catalyst and Windows
 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
 
namespace Microsoft.Maui.TestCases.Tests.Issues;
 
public class Issue33038 : _IssuesUITest
{
    public Issue33038(TestDevice testDevice) : base(testDevice) { }
 
    public override string Issue => "Layout breaks on first navigation until soft keyboard appears/disappears";
 
    [Test]
    [Category(UITestCategories.SafeAreaEdges)]
    public void LayoutShouldBeCorrectOnFirstNavigation()
    {
        App.WaitForElement("StartPageLabel");
        App.Tap("GoToSignInButton");
 
        // Capture the label position immediately after the first navigation.
        // This is the state that regresses: the layout ignores the safe area until
        // the soft keyboard is shown/hidden, which forces a relayout.
        var positionOnFirstNavigation = App.WaitForElement("SignInLabel").GetRect();
        Assert.That(positionOnFirstNavigation.Y, Is.GreaterThan(0),
            $"SignInLabel should be positioned below the safe area on first navigation (Y={positionOnFirstNavigation.Y}).");
 
        // Toggle the soft keyboard, which is what previously "fixed" the broken layout.
        App.Tap("EmailEntry");
        App.WaitForKeyboardToShow();
        App.DismissKeyboard();
        App.WaitForKeyboardToHide();
 
        // After the keyboard toggle the layout is always correct. If the position changed,
        // it means the first-navigation layout was broken and only settled after the toggle.
        var positionAfterKeyboardToggle = App.WaitForElement("SignInLabel").GetRect();
        Assert.That(positionOnFirstNavigation.Y, Is.EqualTo(positionAfterKeyboardToggle.Y).Within(1),
            $"SignInLabel Y on first navigation ({positionOnFirstNavigation.Y}) should match its Y after a keyboard toggle ({positionAfterKeyboardToggle.Y}); a difference indicates the layout was broken until the keyboard appeared.");
    }
}
#endif