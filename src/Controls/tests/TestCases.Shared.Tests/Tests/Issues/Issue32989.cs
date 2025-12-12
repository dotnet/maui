#if WINDOWS         //This is a Windows-specific issue
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue32989 : _IssuesUITest
{
    public override string Issue => "Exception thrown on .NET 10 Windows when calling Permissions.CheckStatusAsync<Permissions.Microphone>()";

    public Issue32989(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Button)]
    public void MicrophonePermissionCheckDoesNotCrash()
    {
        App.WaitForElement("CheckPermissionButton");
        App.Tap("CheckPermissionButton");
        // Verify that the status was updated (permission check completed)
        var statusText = App.FindElement("StatusLabel").GetText();
        Assert.That(statusText, Is.EqualTo("Test Passed"));
    }
}
#endif