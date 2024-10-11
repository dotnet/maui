using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla21177 : _IssuesUITest
{
    public Bugzilla21177(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Using a UICollectionView in a ViewRenderer results in issues with selection";

    // TODO: From Xamarin.UITest migration
    // This test seems wrong? There is no element #1?
    // [Test]
    // [Category(UITestCategories.CollectionView)]
    // [FailsOnIOS]
    // public void Bugzilla21177Test()
    // {
    //     App.WaitForElement("#1");
    //     App.Tap("#1");
    //     App.WaitForElement("Success");
    //     App.Tap("Cancel");
    // }
}