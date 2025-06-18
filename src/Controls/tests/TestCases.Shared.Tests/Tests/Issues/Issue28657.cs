#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28657 : _IssuesUITest
{
    public override string Issue => "iOS - Rotating the simulator would cause clipping on the description text";
    public Issue28657(TestDevice device) : base(device)
    {
    }

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void CellLayoutUpdatesCorrectlyAfterDeviceOrientationChanges()
    {
        App.WaitForElement("StubLabel");
        App.SetOrientationLandscape();
        Thread.Sleep(400);
        VerifyScreenshot("Issue28657_Landscape");
        App.SetOrientationPortrait();
        Thread.Sleep(400);
        VerifyScreenshot("Issue28657_Portrait");
    }
}
#endif