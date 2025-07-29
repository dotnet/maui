using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30846 : _IssuesUITest
{
    public Issue30846(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Media Playback Customization in HybridWebView";

    [Test]
    [Category(UITestCategories.WebView)]
    public void HybridWebView_Autoplay_Respects_UserGestureSetting()
    {
        // Wait for the page to load
        App.WaitForElement("AutoPlaybackSwitch");

        // Test with user gesture required (default: switch is off)
        App.WaitForTextToBePresentInElement("VideoStatusLabel", "Video did not autoplay");
        App.WaitForTextToBePresentInElement("AudioStatusLabel", "Audio did not autoplay");

        // Toggle the switch to allow autoplay (user gesture NOT required)
        App.Tap("AutoPlaybackSwitch");

        // Wait for the WebView to reload and update status
        App.WaitForTextToBePresentInElement("VideoStatusLabel", "Video autoplayed!");
        App.WaitForTextToBePresentInElement("AudioStatusLabel", "Audio autoplayed!");

        // Toggle the switch to allow autoplay (user gesture IS required)
        App.Tap("AutoPlaybackSwitch");

        // Test with user gesture required
        App.WaitForTextToBePresentInElement("VideoStatusLabel", "Video did not autoplay");
        App.WaitForTextToBePresentInElement("AudioStatusLabel", "Audio did not autoplay");
    }
}
