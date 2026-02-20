#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33239 : _IssuesUITest
{
    public override string Issue => "Unexpected high Image ByteCount when loading an image from resources using ImageSource";

    public Issue33239(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Image)]
    public void ImageFromResourceAndFileShouldHaveSimilarByteCount()
    {
        App.WaitForElement("MeasureButton");
        App.Tap("MeasureButton");
        var file = App.FindElement("FileImageLabel").GetText();
        var resource = App.FindElement("ResourceImageLabel").GetText();
        Assert.That(resource, Is.EqualTo(file));
    }
}
#endif
