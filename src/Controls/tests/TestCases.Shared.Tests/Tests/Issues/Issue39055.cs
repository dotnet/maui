using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue39055 : _IssuesUITest
{
    public Issue39055(TestDevice device) : base(device)
    {
    }

    public override string Issue => "TabBar AutomationId should be set";


    [Test]
    [Category(UITestCategories.Shell)]
    public void Issue39055TabBarAutomationIdShouldBeSet()
    {
        App.WaitForElement("main_tab_bar");
        App.WaitForElement("tab_home");
        App.WaitForElement("tab_browse");
        App.WaitForElement("tab_settings");
    }
}
