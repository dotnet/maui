using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.VisualStateManager)]
public class VisualStateManager_LabelFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerLabelFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerLabelFeatureTests;

	public VisualStateManager_LabelFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_Label_InitialState()
	{
		App.WaitForElement("VSMLabelButton");
		App.Tap("VSMLabelButton");
		App.WaitForElement("LabelState");
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
	}

	[Test, Order(2)]
	public void VerifyVSM_Label_Selected()
	{
		App.WaitForElement("SelectableLabelContainer");
		App.Tap("SelectableLabelContainer");
		App.WaitForElement("LabelState");
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Selected"));
		VerifyScreenshot();
	}

	[Test, Order(3)]
	public void VerifyVSM_Label_Disabled()
	{
		App.WaitForElement("LabelDisable");
		App.Tap("LabelDisable");
		App.WaitForElement("LabelState");
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
	}

	[Test, Order(4)]
	public void VerifyVSM_Label_Reset()
	{
		App.WaitForElement("LabelReset");
		App.Tap("LabelReset");
		App.WaitForElement("LabelState");
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
	}

}

