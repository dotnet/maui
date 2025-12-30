using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33264 : _IssuesUITest
{
	public override string Issue => "RadioButtonGroup not working with CollectionView";

	public Issue33264(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButtonGroupBindingWorksInsideCollectionView()
	{
		// Wait for all elements to load
		App.WaitForElement("ChoicesCollectionView");
		App.WaitForElement("Choice 2");
		App.WaitForElement("SelectedValueLabel");
		
		// Initially, SelectedValue should show "None"
		var initialValue = App.FindElement("SelectedValueLabel").GetText();
		Console.WriteLine($"Initial SelectedValue: '{initialValue}'");
		Assert.That(initialValue, Is.EqualTo("None"), "Initial value should be 'None'");

		// Tap "Choice 2" radio button
		App.Tap("Choice 2");

		// Wait for binding update by polling the label text
		App.WaitForTextToBePresentInElement("SelectedValueLabel", "Choice 2");

		// Verify SelectedValue is updated
		var selectedValue = App.FindElement("SelectedValueLabel").GetText();
		Console.WriteLine($"After tapping 'Choice 2', SelectedValue: '{selectedValue}'");
		Assert.That(selectedValue, Is.EqualTo("Choice 2"), "SelectedValue should be updated via binding when RadioButton is checked");

		// Tap "Choice 3" radio button
		App.Tap("Choice 3");

		// Wait for binding update by polling the label text
		App.WaitForTextToBePresentInElement("SelectedValueLabel", "Choice 3");

		// Verify SelectedValue is updated again
		selectedValue = App.FindElement("SelectedValueLabel").GetText();
		Console.WriteLine($"After tapping 'Choice 3', SelectedValue: '{selectedValue}'");
		Assert.That(selectedValue, Is.EqualTo("Choice 3"), "SelectedValue should be updated when different RadioButton is checked");
	}
}
