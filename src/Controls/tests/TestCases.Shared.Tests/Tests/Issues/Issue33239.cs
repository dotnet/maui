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
    public void ImageFromResourceAndStreamShouldHaveSimilarByteCount()
    {
        App.WaitForElement("MeasureButton");
        App.Tap("MeasureButton");
        var resultText = App.FindElement("ResultLabel").GetText();
        Assert.That(resultText, Is.EqualTo("Image ByteCount: 786432"));
    }
}
#endif