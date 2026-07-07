// Material3 CheckBox tests reuse the existing CheckBox Feature Matrix HostApp page.
// The native Android checkbox tint differs (Material3 buttonTint vs Material2 accent color),
// so these tests produce separate screenshot baselines under the Material3 category.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3CheckBoxFeatureTests : _GalleryUITest
{
	const string CheckBoxControl = "CheckBoxControl";
	const string ResetButton = "ResetButton";
	const string IsCheckedSwitch = "IsCheckedSwitch";
	const string BlueColorButton = "BlueColorButton";
	const string GreenColorButton = "GreenColorButton";
	const string DefaultColorButton = "DefaultColorButton";

	public override string GalleryPageName => "CheckBox Feature Matrix";

	public Material3CheckBoxFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.Material3)]
	public void Material3CheckBox_InitialState_VerifyVisualState()
	{
		App.WaitForElement(CheckBoxControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	[Category(UITestCategories.Material3)]
	public void Material3CheckBox_Click_VerifyVisualState()
	{
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3CheckBox_SetColorBlue_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(BlueColorButton);
		App.Tap(BlueColorButton);
		App.WaitForElement(CheckBoxControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3CheckBox_SetColorGreen_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(GreenColorButton);
		App.Tap(GreenColorButton);
		App.WaitForElement(CheckBoxControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3CheckBox_SetColorAndUncheck_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(BlueColorButton);
		App.Tap(BlueColorButton);
		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);
		App.WaitForElement(CheckBoxControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3CheckBox_ResetToDefaultColor_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(BlueColorButton);
		App.Tap(BlueColorButton);
		App.WaitForElement(DefaultColorButton);
		App.Tap(DefaultColorButton);
		App.WaitForElement(CheckBoxControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
