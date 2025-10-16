using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29235 : _IssuesUITest
{
    public Issue29235(TestDevice device) : base(device) { }

    public override string Issue => "Incorrect logic of the Picker element when an item in a bind collection is deleted";

    [Test]
    [Category(UITestCategories.Picker)]
    public void VerifyPickerMaintainsSelectionAfterItemInsert()
    {
        App.WaitForElement("InsertButton");
        App.Tap("InsertButton");
        var text = App.FindElement("SelectedItemLabel").GetText();
        Assert.That(text, Is.EqualTo("Selected Item: Dog"));
    }

    [Test]
    [Category(UITestCategories.Picker)]
    public void VerifyPickerMaintainsSelectionAfterItemRemoval()
    {
        App.WaitForElement("RemoveButton");
        App.Tap("RemoveButton");
        var text = App.FindElement("SelectedItemLabel").GetText();
        Assert.That(text, Is.EqualTo("Selected Item: Rabbit"));
    }
}