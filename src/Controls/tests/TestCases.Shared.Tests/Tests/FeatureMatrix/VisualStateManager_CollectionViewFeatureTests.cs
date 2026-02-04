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
	public void CollectionView_Disabled_UpdatesStateLabel()
	{
		try { App.WaitForElement("VSMCollectionViewButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMCollectionViewButton"); } catch { }
		App.WaitForElement("CVDisable");
		App.Tap("CVDisable");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	public void CollectionView_Reset_ReturnsToNormal()
	{
		try { App.WaitForElement("VSMCollectionViewButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMCollectionViewButton"); } catch { }
		App.WaitForElement("CVReset");
		App.Tap("CVReset");
		var stateText = App.FindElement("CVState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}
}