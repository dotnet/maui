using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue21328 : _IssuesUITest
{
	public Issue21328(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Link in Label with TextType HTML is not clickable";

    [Test]
    [Category(UITestCategories.Label)]
    public void HtmlTextLinkShouldBeClickable()
    {
        App.WaitForElement("HtmlLinkLabel");
        App.Tap("HtmlLinkLabel");
        Thread.Sleep(3000);
        VerifyScreenshot();
    }
}
