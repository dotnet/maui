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


    [Test, Order(1)]
    [ShardedTestCategory(UITestCategories.Shell, shard: 1)]
    public void Issue39055TabBarAutomationIdShouldBeSet()
    {
        App.WaitForElement("main_tab_bar");
        App.WaitForElement("tab_home");
        App.WaitForElement("tab_browse");
        App.WaitForElement("tab_settings");
        App.Tap("tab_browse");
        App.WaitForElement("page_browse");
    }

    [Test, Order(2)]
    [ShardedTestCategory(UITestCategories.Shell, shard: 1)]
    public void Issue39055OverflowTabAutomationIdShouldBeSet()
    {
#if WINDOWS || MACCATALYST  //Reduce window width for overflow tab testing
        App.Tap("ReduceWindowWidthButton");
#endif
        App.WaitForElement("More");
        App.Tap("More");
        App.WaitForElement("tab_account").Tap();
        App.WaitForElement("page_account");
    }
}
