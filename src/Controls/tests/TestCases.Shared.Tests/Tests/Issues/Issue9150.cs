using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9150 : _IssuesUITest
{
	public Issue9150(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Picker SelectedIndex set before items should initialize selection";

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerSelectedIndexSetBeforeItemsInitializesSelection()
	{
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("Passed"));
	}
}
