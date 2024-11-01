#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue10222 : _IssuesUITest
{
    public Issue10222(TestDevice device) : base(device)
    {
    }

    public override string Issue => "[CollectionView] ObjectDisposedException if the page is closed during scrolling";

    [Test]
    [Category(UITestCategories.CollectionView)]
    [FailsOnIOSWhenRunningOnXamarinUITest]
    public void Issue10222Test()
    {
        try
        {
            App.WaitForElement("goTo");
            App.Click("goTo");
            App.WaitForElement("collectionView");
            App.WaitForElement("goTo");
        }
        finally
        {
            App.Back();
        }
    }
}
#endif
