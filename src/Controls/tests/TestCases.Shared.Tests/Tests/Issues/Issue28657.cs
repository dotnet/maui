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
        // Use retryTimeout to wait for orientation change to complete
        VerifyScreenshot("Issue28657_Landscape", retryTimeout: TimeSpan.FromSeconds(2));
        App.SetOrientationPortrait();
        VerifyScreenshot("Issue28657_Portrait", retryTimeout: TimeSpan.FromSeconds(2));
    }
}
#endif