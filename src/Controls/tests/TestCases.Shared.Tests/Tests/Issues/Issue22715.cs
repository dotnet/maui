#if IOS
using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue22715 : _IssuesUITest
{
    public Issue22715(TestDevice device) : base(device) { }

    public override string Issue => "Page should not scroll when focusing element above keyboard";

    [Test]
    [Category(UITestCategories.Entry)]
    public void PageShouldNotScroll ()
    {
        App.WaitForElement("EntNumber").GetRect();
        App.WaitForElement("TopLabel").GetRect();
        VerifyScreenshot();
    }
}
#endif
