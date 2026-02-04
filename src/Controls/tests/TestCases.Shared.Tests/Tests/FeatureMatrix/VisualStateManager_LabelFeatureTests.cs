using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManager_LabelFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerLabelFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerLabelFeatureTests;

	public VisualStateManager_LabelFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void Label_Selected_UpdatesStateLabel()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("SelectableLabelContainer");
		App.WaitForElement("LabelState");
		App.Tap("SelectableLabelContainer");
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Selected"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	public void Label_Disable_BlocksSelectionAndUpdatesStateLabel()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelDisable");
		App.Tap("LabelDisable");
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Disabled"));
		// Tap container should not change state while disabled
		App.Tap("SelectableLabelContainer");
		labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	public void Label_Reset_ReturnsToNormal()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelReset");
		App.Tap("LabelReset");
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Normal"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}
}