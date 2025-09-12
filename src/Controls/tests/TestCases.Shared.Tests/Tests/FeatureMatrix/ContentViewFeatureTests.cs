using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class ContentViewFeatureTests : UITest
{
	public const string ContentViewFeatureMatrix = "ContentView Feature Matrix";

	public ContentViewFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ContentViewFeatureMatrix);
	}

	[Test]
	public void ContentViewWithDefaultLayoutContent()
	{
		App.WaitForElement("This is Default Page");
		App.WaitForElement("Change Text");
		App.Tap("Change Text");
		App.WaitForElement("Text Changed after Button Click!");
	}

	[Test]
	public void ContentViewWithFirstCustomPage()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("First ContentView Page");
	}

	[Test]
	public void ContentViewWithFirstCustomPageAndControlTemplate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("ControlTemplateYesRadioButton");
		App.Tap("ControlTemplateYesRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("First ContentView Page");
		App.WaitForElement("First Control Template Applied");
	}

	[Test]
	public void ContentViewWithSecondCustomPage()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SecondPageRadioButton");
		App.Tap("SecondPageRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Second Custom Title");
	}

	[Test]
	public void ContentViewWithSecondCustomPageAndControlTemplate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SecondPageRadioButton");
		App.Tap("SecondPageRadioButton");
		App.WaitForElement("ControlTemplateYesRadioButton");
		App.Tap("ControlTemplateYesRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Second Custom Title");
		App.WaitForElement("Second Control Template Applied");
	}

	[Test]
	public void DefaultContentWithIsEnabled()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("EnabledFalseRadioButton");
		App.Tap("EnabledFalseRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("This is Default Page");
		App.WaitForElement("Change Text");
		App.Tap("Change Text");
		App.WaitForNoElement("Text Changed after Button Click!");
	}

	[Test]
	public void DefaultContentWithBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BgSkyBlueRadioButton");
		App.Tap("BgSkyBlueRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("This is Default Page");
		VerifyScreenshot();
	}

	[Test]
	public void DefaultContentWithHeightRequest()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Height600RadioButton");
		App.Tap("Height600RadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("This is Default Page");
		VerifyScreenshot();
	}

	[Test]
	public void DefaultContentWithWidthRequest()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Width50RadioButton");
		App.Tap("Width50RadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("This is Default Page");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // related issue link: https://github.com/dotnet/maui/issues/29812
	[Test]
	public void DefaultContentWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("This is Default Page");
		VerifyScreenshot();
	}
#endif

	[Test]
	public void DefaultContentWithIsVisible()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VisibleFalseRadioButton");
		App.Tap("VisibleFalseRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("This is Default Page");
	}

	[Test]
	public void DefaultContentWithFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("This is Default Page");
		VerifyScreenshot();
	}

	[Test]
	public void FirstCustomPageWithIsEnable()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("EnabledTrueRadioButton");
		App.Tap("EnabledTrueRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("First ContentView Page");
		App.WaitForElement("Change Text");
		App.Tap("Change Text");
		App.WaitForElement("Success");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("EnabledFalseRadioButton");
		App.Tap("EnabledFalseRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("First ContentView Page");
		App.WaitForElement("Change Text");
		App.Tap("Change Text");
		App.WaitForElement("Failed");
	}

	[Test]
	public void FirstCustomPageWithBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("BgSkyBlueRadioButton");
		App.Tap("BgSkyBlueRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("First ContentView Page");
		VerifyScreenshot();
	}

	[Test]
	public void FirstCustomPageWithIsVisible()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("VisibleFalseRadioButton");
		App.Tap("VisibleFalseRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("First ContentView Page");
	}

	[Test]
	public void FirstCustomPageWithFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("First ContentView Page");
		VerifyScreenshot();
	}

	[Test]
	public void FirstCustomPageWithCardTitleSetValues()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("CardTitleRadioButtonSecond");
		App.Tap("CardTitleRadioButtonSecond");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Test Content View");
	}

	[Test]
	public void FirstCustomPageWithIconImageChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("IconImageSourceRadioButtonSecond");
		App.Tap("IconImageSourceRadioButtonSecond");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("First ContentView Page");
		VerifyScreenshot();
	}

	[Test]
	public void FirstCustomPageWithCardColorChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("CardColorRadioButtonRed");
		App.Tap("CardColorRadioButtonRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("First ContentView Page");
		VerifyScreenshot();
	}

	[Test]
	public void SecondCustomPageWithBackgroundColorChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SecondPageRadioButton");
		App.Tap("SecondPageRadioButton");
		App.WaitForElement("BgLightGreenRadioButton");
		App.Tap("BgLightGreenRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Second Custom Title");
		VerifyScreenshot();
	}

	[Test]
	public void SecondCustomPageWithIsVisibleChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SecondPageRadioButton");
		App.Tap("SecondPageRadioButton");
		App.WaitForElement("VisibleFalseRadioButton");
		App.Tap("VisibleFalseRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("Second Custom Title");
	}

	[Test]
	public void SecondCustomPageWithFlowDirectionChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SecondPageRadioButton");
		App.Tap("SecondPageRadioButton");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Second Custom Title");
		VerifyScreenshot();
	}
}