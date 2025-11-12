using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
public class Issue32278 : _IssuesUITest
{
public Issue32278(TestDevice device) : base(device) { }

public override string Issue => "Shell content page title position incorrect/clipped";

[Test]
[Category(UITestCategories.Shell)]
public void ShellNavigationPageTitleNotClipped()
{
// Wait for the main page to load
App.WaitForElement("NavigateButton");
App.WaitForElement("TopLabelPage1");

// Get the position of the top label on Page 1
var topLabelPage1 = App.FindElement("TopLabelPage1");
var rectPage1 = topLabelPage1.GetRect();

// Navigate to the new page
App.Tap("NavigateButton");

// Wait for the second page to appear
App.WaitForElement("TopLabelPage2");

// Get the position of the top label on Page 2
var topLabelPage2 = App.FindElement("TopLabelPage2");
var rectPage2 = topLabelPage2.GetRect();

// The top labels should be at the same Y position on both pages
// If SafeAreaEdges is broken on the second page, the label will be clipped under the toolbar
// and positioned differently than on the first page
Assert.That(rectPage2.Y, Is.EqualTo(rectPage1.Y).Within(5), 
$"Top label on Page 2 (Y={rectPage2.Y}) should be at the same position as Page 1 (Y={rectPage1.Y})");

// Both labels should be below the toolbar (not at Y=0 or negative)
Assert.That(rectPage1.Y, Is.GreaterThan(0), 
$"Top label on Page 1 should be below the toolbar (Y={rectPage1.Y})");
Assert.That(rectPage2.Y, Is.GreaterThan(0), 
$"Top label on Page 2 should be below the toolbar (Y={rectPage2.Y})");
}
}
}
