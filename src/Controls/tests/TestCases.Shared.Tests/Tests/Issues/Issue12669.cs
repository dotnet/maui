using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue12669 : _IssuesUITest
{
    public Issue12669(TestDevice device) : base(device) { }

    public override string Issue => "Changing Content property of ShellContent doesn't change visual content";

    [Test]
    [Category(UITestCategories.Shell)]
    public void ShellContentShouldUpdateWhenContentPropertyChanges()
    {
        App.WaitForElement("OriginalContent");
        App.WaitForElement("ChangeContentButton");

        App.Tap("ChangeContentButton");

        // After clicking, the ShellContent.Content is changed to a new page.
        // The visual should update to show the new content.
        App.WaitForElement("NewContent");
        var labelText = App.FindElement("NewContent").GetText();
        Assert.That(labelText, Is.EqualTo("Content Changed"));
    }
}
