using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class RefreshViewFeatureTests : UITest
{
	public const string RefreshViewFeatureMatrix = "RefreshView Feature Matrix";

	public RefreshViewFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(RefreshViewFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_ValidateDefaultValues_VerifyLabels()
	{
		App.WaitForElement("Options");
		Assert.That(App.FindElement("IsRefreshingValueLabel").GetText(), Is.EqualTo("False"));
		Assert.That(App.FindElement("IsEnabledValueLabel").GetText(), Is.EqualTo("True"));
		Assert.That(App.FindElement("IsVisibleValueLabel").GetText(), Is.EqualTo("True"));
		Assert.That(App.FindElement("RefreshStatusLabel").GetText(), Is.EqualTo("None"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // In Appium PullToRefresh is not supported on Catalyst and Windows

	[Test, Order(2)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_InsideScrollView_VerifyScrollAndRefresh()
	{
		App.WaitForElement("RefreshView");
		App.WaitForElement("ScrollViewContentButton");
		App.Tap("ScrollViewContentButton");
		App.WaitForElement("RefreshView");
		App.ScrollUp("RefreshView");
		Assert.That(App.FindElement("RefreshStatusLabel").GetText(), Is.Not.EqualTo("None"));
	}

	[Test, Order(3)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_InsideCollectionView_VerifyRefresh()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		App.WaitForElement("CollectionViewContentButton");
		App.Tap("CollectionViewContentButton");
		App.WaitForElement("RefreshView");
		App.ScrollUp("RefreshView");
		Assert.That(App.FindElement("RefreshStatusLabel").GetText(), Is.Not.EqualTo("None"));
	}

	[Test, Order(4)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetCommandParameterTrue_VerifyCommandParameter()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CommandRedButton");
		App.Tap("CommandRedButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		App.ScrollUp("RefreshView");
		Assert.That(App.FindElement("RefreshStatusLabel").GetText(), Is.Not.EqualTo("None"));
	}

	[Test, Order(5)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetIsEnabled_VerifyEnabledState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseButton");
		App.Tap("IsEnabledFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		App.ScrollUp("RefreshView");
		Assert.That(App.FindElement("RefreshStatusLabel").GetText(), Is.EqualTo("None"));
	}

	[Test, Order(6)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetRefreshColorBlue_VerifyColorChange()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("RefreshColorBlueRadio");
		App.Tap("RefreshColorBlueRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		App.WaitForElement("CollectionViewContentButton");
		App.Tap("CollectionViewContentButton");
		App.WaitForElement("RefreshView");
		App.ScrollUp("RefreshView");
		Assert.That(App.FindElement("RefreshStatusLabel").GetText(), Is.Not.EqualTo("None"));
	}

	[Test, Order(7)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetFlowDirectionRightToLeft_VerifyFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		App.ScrollUp("RefreshView");
		Assert.That(App.FindElement("RefreshStatusLabel").GetText(), Is.Not.EqualTo("None"));
	}
#endif

	[Test, Order(8)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetIsVisible_VerifyVisibilityState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseButton");
		App.Tap("IsVisibleFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("RefreshView");
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30535

	[Test, Order(9)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetIsRefreshingAndScrollView_VerifyStatusChanges()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsRefreshingTrueRadioButton");
		App.Tap("IsRefreshingTrueRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		App.WaitForElement("ScrollViewContentButton");
		App.Tap("ScrollViewContentButton");
		Assert.That(App.FindElement("IsRefreshingValueLabel").GetText(), Is.EqualTo("True"));
	}

	[Test, Order(10)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetIsRefreshingAndCollectionView_VerifyStatusChanges()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsRefreshingTrueRadioButton");
		App.Tap("IsRefreshingTrueRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		App.WaitForElement("CollectionViewContentButton");
		App.Tap("CollectionViewContentButton");
		Assert.That(App.FindElement("IsRefreshingValueLabel").GetText(), Is.EqualTo("True"));
	}

	[Test, Order(11)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetRefreshColorRedAndIsRefreshing_VerifyColorChange()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsRefreshingTrueRadioButton");
		App.Tap("IsRefreshingTrueRadioButton");
		App.WaitForElement("RefreshColorRedRadio");
		App.Tap("RefreshColorRedRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		Assert.That(App.FindElement("IsRefreshingValueLabel").GetText(), Is.EqualTo("True"));
	}
#endif

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/29812

	[Test, Order(12)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetShadow_VerifyShadowApplied()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowTrueButton");
		App.Tap("ShadowTrueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		VerifyScreenshot();
	}

	[Test, Order(13)]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetShadowWithCollectionView_VerifyShadowApplied()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowTrueButton");
		App.Tap("ShadowTrueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		App.WaitForElement("CollectionViewContentButton");
		App.Tap("CollectionViewContentButton");
		VerifyScreenshot();
	}
#endif
}