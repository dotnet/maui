using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class CheckBoxFeatureTests : _GalleryUITest
{
	public const string CheckBoxFeatureMatrix = "CheckBox Feature Matrix";

	public override string GalleryPageName => CheckBoxFeatureMatrix;

	public CheckBoxFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_ValidateDefaultValues_VerifyLabels()
	{
		Assert.That(App.FindElement("IsCheckedLabel").GetText(), Is.EqualTo("True"));
		App.WaitForElement("CheckBoxControl");
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_SetIsCheckedState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("IsCheckedSwitch");
		App.Tap("IsCheckedSwitch");

		Assert.That(App.FindElement("IsCheckedLabel").GetText(), Is.EqualTo("False"));

		App.WaitForElement("IsCheckedSwitch");
		App.Tap("IsCheckedSwitch");

		Assert.That(App.FindElement("IsCheckedLabel").GetText(), Is.EqualTo("True"));
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_VerifyCheckedChangedEvent()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("IsEnabledSwitch");
		App.Tap("IsEnabledSwitch");

		App.Tap("CheckBoxControl");

		App.WaitForNoElement("CheckedChanged Triggered");

		App.WaitForElement("IsEnabledSwitch");
		App.Tap("IsEnabledSwitch");

		App.Tap("CheckBoxControl");

		Assert.That(App.FindElement("CheckedChangedStatusLabel").GetText(), Is.EqualTo("CheckedChanged Triggered"));
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_SetVisibilityToFalse_VerifyVisualState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("IsVisibleSwitch");
		App.Tap("IsVisibleSwitch");

		Assert.That(App.FindElements("CheckBoxControl"), Is.Empty);

		App.WaitForElement("IsVisibleSwitch");
		App.Tap("IsVisibleSwitch");

		Assert.That(App.FindElements("CheckBoxControl"), Is.Not.Empty);
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_ChangeColor_VerifyVisualState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("GreenColorButton");
		App.Tap("GreenColorButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_SetIsCheckedAndColor_VerifyVisualState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("IsCheckedSwitch");
		App.Tap("IsCheckedSwitch");

		App.WaitForElement("BlueColorButton");
		App.Tap("BlueColorButton");

		Assert.That(App.FindElement("IsCheckedLabel").GetText(), Is.EqualTo("False"));

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_SetAllProperties_VerifyVisualState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("BlueColorButton");
		App.Tap("BlueColorButton");

		App.WaitForElement("IsEnabledSwitch");
		App.Tap("IsEnabledSwitch");

		App.Tap("CheckBoxControl");

		Assert.That(App.FindElement("IsCheckedLabel").GetText(), Is.EqualTo("True"));
	}
}
