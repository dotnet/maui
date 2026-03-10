using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.VisualStateManager)]
public class VisualStateManager_CollectionViewFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerCollectionViewFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerCollectionViewFeatureTests;

	public VisualStateManager_CollectionViewFeatureTests(TestDevice device)
		: base(device)
	{
	}
// PointerOver states cannot currently be reliably covered in CI environments, as hover/pointer interactions are not consistently supported in automated runs. Therefore, these states are validated manually on Mac and Windows, and PointerOver-related tests have not been included in the automated test cases.
	[Test, Order(1)]
	public void VerifyVSM_CollectionView_InitialState()
	{
		App.WaitForElement("VSMCollectionViewButton");
		App.Tap("VSMCollectionViewButton");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
	}

	[Test, Order(2)]
	public void VerifyVSM_CollectionView_Selected()
	{
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (1)"));
		VerifyScreenshot();
	}

	[Test, Order(3)]
	public void VerifyVSM_CollectionView_Normal()
	{
		App.WaitForElement("CVNormal");
		App.Tap("CVNormal");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unselected"));
		VerifyScreenshot();
	}

	[Test, Order(4)]
	public void VerifyVSM_CollectionView_Disabled()
	{
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
	}

	[Test, Order(5)]
	public void VerifyVSM_CollectionView_Reset()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST //related issue: https://github.com/dotnet/maui/issues/18028
	[Test, Order(6)]
	public void VerifyVSM_CollectionView_Selected_Multiple()
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
		VerifyScreenshot();
	}
#endif

	[Test, Order(7)]
	public void VerifyVSM_CollectionView_DisableWhileNormal()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVNormal");
		App.Tap("CVNormal");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unselected"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(8)]
	public void VerifyVSM_CollectionView_DisableWhileSelected()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVSelectItem");
		App.Tap("CVSelectItem");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (1)"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(9)]
	public void VerifyVSM_CollectionView_ResetWhileDisabled()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(10)]
	public void VerifyVSM_CollectionView_ResetWhileNormal()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVNormal");
		App.Tap("CVNormal");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unselected"));
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(11)]
	public void VerifyVSM_CollectionView_ResetWhileSelected()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVSelectItem");
		App.Tap("CVSelectItem");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (1)"));
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

#if TEST_FAILS_ON_CATALYST //related issue: https://github.com/dotnet/maui/issues/18028
	[Test, Order(12)]
	public void VerifyVSM_CollectionView_ResetWhileMultipleSelected()
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
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(13)]
	public void VerifyVSM_CollectionView_SelectAndUnselectItem_UsingTap()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (1)"));
		App.Tap("Banana");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}
#endif

	[Test, Order(14)]
	public void VerifyVSM_CollectionView_SelectItem_UsingButton()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVSelectItem");
		App.Tap("CVSelectItem");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (1)"));
		App.WaitForElement("CVNormal");
		App.Tap("CVNormal");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unselected"));
		}

#if TEST_FAILS_ON_CATALYST //related issue: https://github.com/dotnet/maui/issues/18028
	[Test, Order(15)]
	public void VerifyVSM_CollectionView_SelectMultipleItems_UsingTap()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("Cherry");
		App.Tap("Cherry");
		App.WaitForElement("Grape");
		App.Tap("Grape");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (3)"));
		App.Tap("Cherry");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (2)"));
	}
#endif

	[Test, Order(16)]
	[Ignore("Fails on all platforms. Related issue: https://github.com/dotnet/maui/issues/20615")]
	public void VerifyVSM_CollectionView_DisableAndEnableWhileSelected()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (1)"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText,Is.EqualTo("State: Selected (1)"));
	}

	[Test, Order(17)]
	public void VerifyVSM_CollectionView_DisableAndEnableWhileUnselected()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVNormal");
		App.Tap("CVNormal");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unselected"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

#if TEST_FAILS_ON_CATALYST //related issue: https://github.com/dotnet/maui/issues/18028
	[Test, Order(18)]
	public void VerifyVSM_CollectionView_DisableEnableWhileSelectMultipleItems()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("Honeydew");
		App.Tap("Honeydew");
		App.WaitForElement("Grape");
		App.Tap("Grape");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (3)"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Selected (3)"));
	}
#endif

	[Test, Order(19)]
	[Ignore("Fails on all platforms. Related issue: https://github.com/dotnet/maui/issues/20615")]
	public void VerifyVSM_CollectionView_SelectedWhileDisabled()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(20)]
	[Ignore("Fails on all platforms. Related issue: https://github.com/dotnet/maui/issues/20615")]
	public void VerifyVSM_CollectionView_SelectedMultipleWhileDisabled()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("Cherry");
		App.Tap("Cherry");
		App.WaitForElement("Fig");
		App.Tap("Fig");
		App.WaitForElement("CVState");	
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(21)]
	[Ignore("Fails on all platforms. Related issue: https://github.com/dotnet/maui/issues/20615")]
	public void VerifyVSM_CollectionView_UnselectWhileDisabled()
	{
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("CVState");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Does.Contain("State: Selected (1)"));
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("CVState");
		stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}
}

