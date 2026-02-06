using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManager_CollectionViewFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerCollectionViewFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerCollectionViewFeatureTests;

	public VisualStateManager_CollectionViewFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_CollectionView_Disabled_UpdatesStateLabel()
	{
		App.WaitForElement("VSMCollectionViewButton");
		App.Tap("VSMCollectionViewButton");
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("CollectionView_Disabled_State");
	}

	[Test, Order(2)]
	public void VerifyVSM_CollectionView_Reset_ReturnsToNormal()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot("CollectionView_Normal_State");
	}

	[Test, Order(3)]
	public void VerifyVSM_CollectionView_SelectItem_UpdatesToSelectedState()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (1)"));
		VerifyScreenshot("CollectionView_Selected_State");
	}

	[Test, Order(4)]
	public void VerifyVSM_CollectionView_SelectMultipleItems_UpdatesSelectionCount()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("Cherry");
		App.Tap("Cherry");
		App.WaitForElement("Date");
		App.Tap("Date");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (3)"));
        VerifyScreenshot("CollectionView_Selected_Multiple_State");
	}

	[Test, Order(5)]
	public void VerifyVSM_CollectionView_DisableWhileSelected_MaintainsDisabledState()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("CollectionView_Disabled_Selected_State");
	}

	[Test, Order(6)]
	public void VerifyVSM_CollectionView_DeselectItems_ReturnsToNormal()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("Cherry");
		App.Tap("Cherry");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (2)"));
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("Cherry");
		App.Tap("Cherry");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(7)]
	public void VerifyVSM_CollectionView_ToggleDisable_RestoresCorrectState()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("CVState");
		var selectedText = App.FindElement("CVState").GetText();
		Assert.That(selectedText, Does.Contain("State: Selected (1)"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		var disabledText = App.FindElement("CVState").GetText();
		Assert.That(disabledText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		var enabledText = App.FindElement("CVState").GetText();
		Assert.That(enabledText, Does.Contain("State: Selected (1)"));
	}
}