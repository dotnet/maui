using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31363 : _IssuesUITest
{
    public Issue31363(TestDevice device) : base(device) { }

    public override string Issue => "The images under the I-CollectionView category are not showing up";

    [Test]
    [Category(UITestCategories.Image)]
    public void UriImageSourceShouldDisplayProperly()
    {
        App.WaitForElement("TestImage");
        VerifyScreenshot();
    }
}