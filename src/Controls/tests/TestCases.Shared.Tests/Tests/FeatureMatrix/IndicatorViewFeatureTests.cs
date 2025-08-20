using Microsoft.Maui.Controls.Shapes;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class IndicatorViewFeatureTests : UITest
{
	private const string IndicatorViewFeatureMatrix = "IndicatorView Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string IndicatorColorBrownButton = "IndicatorColorBrownButton";
	public const string IndicatorColorGreenButton = "IndicatorColorGreenButton";
	public const string SelectedIndicatorColorGrayButton = "SelectedIndicatorColorGrayButton";
	public const string SelectedIndicatorColorOrangeButton = "SelectedIndicatorColorOrangeButton";
	public const string SelectedIndicatorColorPurpleButton = "SelectedIndicatorColorPurpleButton";
	public const string IndicatorShapeSquareRadioButton = "IndicatorShapeSquareRadioButton";
	public const string HideSingleTrueRadioButton = "HideSingleTrueRadioButton";
	public const string HideSingleFalseRadioButton = "HideSingleFalseRadioButton";
	public const string FlowDirectionRightToLeftRadioButton = "FlowDirectionRTLRadioButton";
	public const string IsEnabledFalseRadioButton = "IsEnabledFalseRadioButton";
	public const string IsVisibleFalseRadioButton = "IsVisibleFalseRadioButton";
	public const string PositionEntry = "PositionEntry";
	public const string AddItemButton = "AddItemButton";
	public const string RemoveItemButton = "RemoveItemButton";
	public const string MaximumVisibleStepper = "MaximumVisibleStepper";
	public const string ShadowTrueRadioButton = "ShadowTrueRadioButton";
	public const string CountLabel = "CountLabel";
	public const string PositionLabel = "PositionLabel";
	public const string IndicatorSizeStepper = "IndicatorSizeStepper";
	public const string PositionStepper = "PositionStepper";
	public const string IconTemplateButton = "IconTemplateButton";

	public IndicatorViewFeatureTests(TestDevice device)
	: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(IndicatorViewFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IndicatorColorGreenButton);
		App.Tap(IndicatorColorGreenButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifySelectedIndicatorColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectedIndicatorColorPurpleButton);
		App.Tap(SelectedIndicatorColorPurpleButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorView_Count()
	{
		App.WaitForElement(Options);
		Assert.That(App.FindElement(CountLabel).GetText(), Is.EqualTo("Count: 5"));
		App.WaitForElement(AddItemButton);
		App.Tap(AddItemButton);
		Assert.That(App.FindElement(CountLabel).GetText(), Is.EqualTo("Count: 6"));
		App.WaitForElement(RemoveItemButton);
		App.Tap(RemoveItemButton);
		Assert.That(App.FindElement(CountLabel).GetText(), Is.EqualTo("Count: 5"));
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/29312, https://github.com/dotnet/maui/issues/15443
	[Test, Order(4)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorView_Position()
	{
		App.WaitForElement(Options);
		Assert.That(App.FindElement(PositionLabel).GetText(), Is.EqualTo("Position: 0"));
		App.WaitForElement(Options);
		App.Tap(Options);
		IncreasePositionStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.That(App.FindElement(PositionLabel).GetText(), Is.EqualTo("Position: 1"));
	}

	[Test, Order(13)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifySelectedIndicatorColorWhenItemsChanged()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectedIndicatorColorOrangeButton);
		App.Tap(SelectedIndicatorColorOrangeButton);
		IncreasePositionStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(18)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifySelectedIndicatorColorWithPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectedIndicatorColorPurpleButton);
		App.Tap(SelectedIndicatorColorPurpleButton);
		IncreasePositionStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(29)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorShapeWithPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IndicatorShapeSquareRadioButton);
		App.Tap(IndicatorShapeSquareRadioButton);
		IncreasePositionStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/31064
	[Test, Order(5)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifySelectedIndicatorSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		IncreaseIndicatorSizeStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/31065
	[Test, Order(6)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorShape()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IndicatorShapeSquareRadioButton);
		App.Tap(IndicatorShapeSquareRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorColorWithIndicatorShape()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IndicatorColorBrownButton);
		App.Tap(IndicatorColorBrownButton);
		App.WaitForElement(IndicatorShapeSquareRadioButton);
		App.Tap(IndicatorShapeSquareRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(17)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifySelectedIndicatorColorWithIndicatorShape()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectedIndicatorColorOrangeButton);
		App.Tap(SelectedIndicatorColorOrangeButton);
		App.WaitForElement(IndicatorShapeSquareRadioButton);
		App.Tap(IndicatorShapeSquareRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(21)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorSizeWithIndicatorShape()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		IncreaseIndicatorSizeStepper();
		App.WaitForElement(IndicatorShapeSquareRadioButton);
		App.Tap(IndicatorShapeSquareRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(26)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorShapeWithFlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IndicatorShapeSquareRadioButton);
		App.Tap(IndicatorShapeSquareRadioButton);
		App.WaitForElement(FlowDirectionRightToLeftRadioButton);
		App.Tap(FlowDirectionRightToLeftRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(27)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorShapeWithHideSingle()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HideSingleTrueRadioButton);
		App.Tap(HideSingleTrueRadioButton);
		App.WaitForElement(IndicatorShapeSquareRadioButton);
		App.Tap(IndicatorShapeSquareRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		for (int i = 0; i < 4; i++)
		{
			App.WaitForElement(RemoveItemButton);
			App.Tap(RemoveItemButton);
		}
		VerifyScreenshot();
	}

	[Test, Order(28)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorShapeWithMaximumVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IndicatorShapeSquareRadioButton);
		App.Tap(IndicatorShapeSquareRadioButton);
		DecreaseMaximumVisibleStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/29812
	[Test, Order(30)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorShapeWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IndicatorShapeSquareRadioButton);
		App.Tap(IndicatorShapeSquareRadioButton);
		App.WaitForElement(ShadowTrueRadioButton);
		App.Tap(ShadowTrueRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif
#endif

	[Test, Order(7)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorView_HideSingle()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HideSingleTrueRadioButton);
		App.Tap(HideSingleTrueRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		for (int i = 0; i < 4; i++)
		{
			App.WaitForElement(RemoveItemButton);
			App.Tap(RemoveItemButton);
		}
		VerifyScreenshot();
	}

	[Test, Order(8)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorView_MaximumVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		DecreaseMaximumVisibleStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(9)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorView_FlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRightToLeftRadioButton);
		App.Tap(FlowDirectionRightToLeftRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(10)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorView_IsVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsVisibleFalseRadioButton);
		App.Tap(IsVisibleFalseRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // https://github.com/dotnet/maui/issues/29812
	[Test, Order(11)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorView_Shadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ShadowTrueRadioButton);
		App.Tap(ShadowTrueRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	[Test, Order(12)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorColorWhenItemsAdded()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IndicatorColorBrownButton);
		App.Tap(IndicatorColorBrownButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItemButton);
		App.Tap(AddItemButton);
		VerifyScreenshot();
	}

	[Test, Order(14)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorColorWithIndicatorSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IndicatorColorBrownButton);
		App.Tap(IndicatorColorBrownButton);
		IncreaseIndicatorSizeStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(15)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifySelectedIndicatorColorWithIndicatorSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectedIndicatorColorOrangeButton);
		App.Tap(SelectedIndicatorColorOrangeButton);
		IncreaseIndicatorSizeStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifySelectedIndicatorColorWithFlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectedIndicatorColorOrangeButton);
		App.Tap(SelectedIndicatorColorOrangeButton);
		App.WaitForElement(FlowDirectionRightToLeftRadioButton);
		App.Tap(FlowDirectionRightToLeftRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(20)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorColorWithFlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IndicatorColorGreenButton);
		App.Tap(IndicatorColorGreenButton);
		App.WaitForElement(FlowDirectionRightToLeftRadioButton);
		App.Tap(FlowDirectionRightToLeftRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(22)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorHideSingleWithIndicatorSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HideSingleTrueRadioButton);
		App.Tap(HideSingleTrueRadioButton);
		IncreaseIndicatorSizeStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		for (int i = 0; i < 4; i++)
		{
			App.WaitForElement(RemoveItemButton);
			App.Tap(RemoveItemButton);
		}
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //Issue Link: https://github.com/dotnet/maui/issues/31140 , https://github.com/dotnet/maui/issues/29812
	[Test, Order(23)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorSizeWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		IncreaseIndicatorSizeStepper();
		App.WaitForElement(ShadowTrueRadioButton);
		App.Tap(ShadowTrueRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	[Test, Order(24)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorSizeWithFlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		IncreaseIndicatorSizeStepper();
		App.WaitForElement(FlowDirectionRightToLeftRadioButton);
		App.Tap(FlowDirectionRightToLeftRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(25)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorSizeWithMaximumVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		IncreaseIndicatorSizeStepper();
		DecreaseMaximumVisibleStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(31)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifySelectedIndicatorColorWithIndicatorColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectedIndicatorColorOrangeButton);
		App.Tap(SelectedIndicatorColorOrangeButton);
		App.WaitForElement(IndicatorColorGreenButton);
		App.Tap(IndicatorColorGreenButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(32)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorHideSingleIsFalse()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HideSingleFalseRadioButton);
		App.Tap(HideSingleFalseRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		for (int i = 0; i < 4; i++)
		{
			App.WaitForElement(RemoveItemButton);
			App.Tap(RemoveItemButton);
		}
		VerifyScreenshot();
	}

	[Test, Order(33)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorHideSingleIsFalseWithSelectedIndicatorColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HideSingleFalseRadioButton);
		App.Tap(HideSingleFalseRadioButton);
		App.WaitForElement(SelectedIndicatorColorOrangeButton);
		App.Tap(SelectedIndicatorColorOrangeButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		for (int i = 0; i < 4; i++)
		{
			App.WaitForElement(RemoveItemButton);
			App.Tap(RemoveItemButton);
		}
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
// Issue Link: https://github.com/dotnet/maui/issues/31128
// Issue Link: https://github.com/dotnet/maui/issues/31141
// Issue Link: https://github.com/dotnet/maui/issues/31145
	[Test, Order(34)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IconTemplateButton);
		App.Tap(IconTemplateButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(35)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorTemplateWithFlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IconTemplateButton);
		App.Tap(IconTemplateButton);
		App.WaitForElement(FlowDirectionRightToLeftRadioButton);
		App.Tap(FlowDirectionRightToLeftRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(36)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorTemplateWithMaximumVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IconTemplateButton);
		App.Tap(IconTemplateButton);
		DecreaseMaximumVisibleStepper();
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(37)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorTemplateWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IconTemplateButton);
		App.Tap(IconTemplateButton);
		App.WaitForElement(ShadowTrueRadioButton);
		App.Tap(ShadowTrueRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(38)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorTemplateWithIsVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IconTemplateButton);
		App.Tap(IconTemplateButton);
		App.WaitForElement(IsVisibleFalseRadioButton);
		App.Tap(IsVisibleFalseRadioButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(39)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorTemplateWithSelectedIndicatorColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IconTemplateButton);
		App.Tap(IconTemplateButton);
		App.WaitForElement(SelectedIndicatorColorOrangeButton);
		App.Tap(SelectedIndicatorColorOrangeButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(40)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorTemplateWithIndicatorColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IconTemplateButton);
		App.Tap(IconTemplateButton);
		App.WaitForElement(IndicatorColorGreenButton);
		App.Tap(IndicatorColorGreenButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	public void IncreasePositionStepper()
	{
		ChangeStepper(PositionStepper, true);
	}
	public void DecreaseMaximumVisibleStepper()
	{
		ChangeStepper(MaximumVisibleStepper, false);
	}
	public void IncreaseIndicatorSizeStepper()
	{
		ChangeStepper(IndicatorSizeStepper, true);
	}
	private void ChangeStepper(string stepperAutomationId, bool increase)
	{
#if WINDOWS
		if (increase)
			App.IncreaseStepper(stepperAutomationId);
		else
			App.DecreaseStepper(stepperAutomationId);
#else
		App.WaitForElement(stepperAutomationId);
		if (increase)
			App.IncreaseStepper(stepperAutomationId);
		else
			App.DecreaseStepper(stepperAutomationId);
#endif
	}
}