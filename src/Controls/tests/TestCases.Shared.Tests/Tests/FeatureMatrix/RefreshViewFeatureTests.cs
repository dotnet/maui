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

	[Test]
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
		var rect = App.WaitForElement("RefreshView").GetRect();
		App.DragCoordinates(rect.CenterX(), rect.CenterY(), rect.CenterX(), rect.CenterY() + 200);
		VerifyScreenshot();
	}

	[Test]
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
		var rect = App.WaitForElement("RefreshView").GetRect();
		App.DragCoordinates(rect.CenterX(), rect.CenterY(), rect.CenterX(), rect.CenterY() + 200);
		Assert.That(App.FindElement("RefreshStatusLabel").GetText(), Is.EqualTo("None"));
		VerifyScreenshot();
	}

	[Test]
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetFlowDirectionRightToLeft_VerifyFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		var rect = App.WaitForElement("RefreshView").GetRect();
		App.DragCoordinates(rect.CenterX(), rect.CenterY(), rect.CenterX(), rect.CenterY() + 200);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetIsRefreshing_VerifyStatusChanges()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsRefreshingTrueRadioButton");
		App.Tap("IsRefreshingTrueRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("RefreshView");
		VerifyScreenshot();
	}

	[Test]
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.RefreshView)]
	public void RefreshView_SetRefreshColorBlue_VerifyColorChange()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("RefreshColorBlueRadio");
		App.Tap("RefreshColorBlueRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		var rect = App.WaitForElement("RefreshView").GetRect();
		App.DragCoordinates(rect.CenterX(), rect.CenterY(), rect.CenterX(), rect.CenterY() + 200);
		VerifyScreenshot();
	}
	[Test]
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
}