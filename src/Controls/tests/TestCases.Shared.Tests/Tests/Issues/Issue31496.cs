using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31496 : _IssuesUITest
{
    public Issue31496(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "BoxView in AbsoluteLayout does not return to default AutoSize";

    [Test]
    [Category(UITestCategories.Layout)]
    public void BoxViewInAbsoluteLayoutReturnsToDefaultAutoSize()
    {
        App.WaitForElement("Issue31496ChangeBoundsButton");
        App.Tap("Issue31496ChangeBoundsButton");
        App.WaitForElement("Issue31496ResetButton");
        App.Tap("Issue31496ResetButton");
        VerifyScreenshot();
    }
}