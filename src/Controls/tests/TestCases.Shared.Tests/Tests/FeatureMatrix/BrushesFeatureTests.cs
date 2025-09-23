using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Brush)]
public class BrushesFeatureTests : UITest
{
	public const string BrushesFeatureMatrix = "Brushes Feature Matrix";
	public BrushesFeatureTests(TestDevice testDevice) : base(testDevice)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(BrushesFeatureMatrix);
	}

	public void VerifyBrushesScreenshot()
	{
#if WINDOWS
		VerifyScreenshot(cropTop: 100, tolerance: 0.10);
#else
		VerifyScreenshot(tolerance: 0.10);
#endif
	}

	[Test]
	public void VerifySolidColorBrushComparison()
	{
		App.WaitForElement("BrushesLabel");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Color1Green");
		App.Tap("Color1Green");
		App.WaitForElement("Color2Green");
		App.Tap("Color2Green");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("CompareButton");
		App.Tap("CompareButton");
		Assert.That(App.FindElement("CompareResultLabel").GetText(), Is.EqualTo("True"));
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Color1Blue");
		App.Tap("Color1Blue");
		App.WaitForElement("Color2Red");
		App.Tap("Color2Red");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("CompareButton");
		App.Tap("CompareButton");
		Assert.That(App.FindElement("CompareResultLabel").GetText(), Is.EqualTo("False"));
	}

	[Test]
	public void VerifySolidColorBrushWithBackgroundColor()
	{
		App.WaitForElement("BrushesLabel");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundSolidColor");
		App.Tap("BackgroundSolidColor");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundLinearGradient");
		App.Tap("BackgroundLinearGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifySolidColorBrushWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundSolidColor");
		App.Tap("BackgroundSolidColor");
		App.WaitForElement("ShadowSolidColor");
		App.Tap("ShadowSolidColor");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundLinearGradient");
		App.Tap("BackgroundLinearGradient");
		App.WaitForElement("ShadowLinearGradient");
		App.Tap("ShadowLinearGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("ShadowRadialGradient");
		App.Tap("ShadowRadialGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifySolidColorBrushWithStroke()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundSolidColor");
		App.Tap("BackgroundSolidColor");
		App.WaitForElement("StrokeSolidColor");
		App.Tap("StrokeSolidColor");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithStroke()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundLinearGradient");
		App.Tap("BackgroundLinearGradient");
		App.WaitForElement("StrokeLinearGradient");
		App.Tap("StrokeLinearGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithStroke()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("StrokeRadialGradient");
		App.Tap("StrokeRadialGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifySolidColorBrushWithOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundSolidColor");
		App.Tap("BackgroundSolidColor");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundLinearGradient");
		App.Tap("BackgroundLinearGradient");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
public void VerifySolidBrushColorWithStrokeAndOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundSolidColor");
		App.Tap("BackgroundSolidColor");
		App.WaitForElement("StrokeSolidColor");
		App.Tap("StrokeSolidColor");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithStrokeAndOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundLinearGradient");
		App.Tap("BackgroundLinearGradient");
		App.WaitForElement("StrokeLinearGradient");
		App.Tap("StrokeLinearGradient");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithStrokeAndOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("StrokeRadialGradient");
		App.Tap("StrokeRadialGradient");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifySolidBrushColorWithShadowAndOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundSolidColor");
		App.Tap("BackgroundSolidColor");
		App.WaitForElement("ShadowSolidColor");
		App.Tap("ShadowSolidColor");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithShadowAndOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundLinearGradient");
		App.Tap("BackgroundLinearGradient");
		App.WaitForElement("ShadowLinearGradient");
		App.Tap("ShadowLinearGradient");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithShadowAndOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("ShadowRadialGradient");
		App.Tap("ShadowRadialGradient");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifySolidColorBrushWithBackgroundColorAndShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundSolidColor");
		App.Tap("BackgroundSolidColor");
		App.WaitForElement("ShadowSolidColor");
		App.Tap("ShadowSolidColor");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithBackgroundAndStartPoint()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundLinearGradient");
		App.Tap("BackgroundLinearGradient");
		App.WaitForElement("LinearStartXEntry");
		App.ClearText("LinearStartXEntry");
		App.EnterText("LinearStartXEntry", "1");
		App.WaitForElement("LinearStartYEntry");
		App.ClearText("LinearStartYEntry");
		App.EnterText("LinearStartYEntry", "2");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithBackgroundAndCenter()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("RadialCenterXEntry");
		App.ClearText("RadialCenterXEntry");
		App.EnterText("RadialCenterXEntry", "0.2");
		App.WaitForElement("RadialCenterYEntry");
		App.ClearText("RadialCenterYEntry");
		App.EnterText("RadialCenterYEntry", "0.8");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithBackgroundAndRadius()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("RadialRadiusEntry");
		App.ClearText("RadialRadiusEntry");
		App.EnterText("RadialRadiusEntry", "0.3");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithStrokeAndStartPoint()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundLinearGradient");
		App.Tap("BackgroundLinearGradient");
		App.WaitForElement("StrokeLinearGradient");
		App.Tap("StrokeLinearGradient");
		App.WaitForElement("LinearStartXEntry");
		App.ClearText("LinearStartXEntry");
		App.EnterText("LinearStartXEntry", "1");
		App.WaitForElement("LinearStartYEntry");
		App.ClearText("LinearStartYEntry");
		App.EnterText("LinearStartYEntry", "2");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithStrokeAndEndPoint()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundLinearGradient");
		App.Tap("BackgroundLinearGradient");
		App.WaitForElement("StrokeLinearGradient");
		App.Tap("StrokeLinearGradient");
		App.WaitForElement("LinearEndXEntry");
		App.ClearText("LinearEndXEntry");
		App.EnterText("LinearEndXEntry", "0.5");
		App.WaitForElement("LinearEndYEntry");
		App.ClearText("LinearEndYEntry");
		App.EnterText("LinearEndYEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithStrokeAndRadius()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("StrokeRadialGradient");
		App.Tap("StrokeRadialGradient");
		App.WaitForElement("RadialRadiusEntry");
		App.ClearText("RadialRadiusEntry");
		App.EnterText("RadialRadiusEntry", "0.3");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithStrokeAndCenter()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("StrokeRadialGradient");
		App.Tap("StrokeRadialGradient");
		App.WaitForElement("RadialCenterXEntry");
		App.ClearText("RadialCenterXEntry");
		App.EnterText("RadialCenterXEntry", "0.2");
		App.WaitForElement("RadialCenterYEntry");
		App.ClearText("RadialCenterYEntry");
		App.EnterText("RadialCenterYEntry", "0.8");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifySolidColorBrushWithBackgroundColorAndShadowAndStroke()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundSolidColor");
		App.Tap("BackgroundSolidColor");
		App.WaitForElement("ShadowSolidColor");
		App.Tap("ShadowSolidColor");
		App.WaitForElement("StrokeSolidColor");
		App.Tap("StrokeSolidColor");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithBackgroundAndShadowAndStroke()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundLinearGradient");
		App.Tap("BackgroundLinearGradient");
		App.WaitForElement("ShadowLinearGradient");
		App.Tap("ShadowLinearGradient");
		App.WaitForElement("StrokeLinearGradient");
		App.Tap("StrokeLinearGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithBackgroundAndShadowAndStroke()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundRadialGradient");
		App.Tap("BackgroundRadialGradient");
		App.WaitForElement("ShadowRadialGradient");
		App.Tap("ShadowRadialGradient");
		App.WaitForElement("StrokeRadialGradient");
		App.Tap("StrokeRadialGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyBackgroundWithAlterSolidColorBrush()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("AltSolidButton");
		App.Tap("AltSolidButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyBackgroundWithAlterLinearGradientBrush()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("AltLinearButton");
		App.Tap("AltLinearButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}

	[Test]
	public void VerifyBackgroundWithAlterRadialGradientBrush()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("AltRadialButton");
		App.Tap("AltRadialButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("BrushesLabel");
		VerifyBrushesScreenshot();
	}
}