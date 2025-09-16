using System;
using Microsoft.Maui.Platform;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

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

	[Test]
	public void VerifySolidColorBrushWithBackgroundColor()
	{
		App.WaitForElement("Brushes Control Test");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundSolidColor");
		App.Tap("BackgroundSolidColor");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
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
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
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
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
	}

	[Test]
	public void VerifySolidColorBrushWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowSolidColor");
		App.Tap("ShadowSolidColor");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowLinearGradient");
		App.Tap("ShadowLinearGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowRadialGradient");
		App.Tap("ShadowRadialGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
	}

	[Test]
	public void VerifySolidColorBrushWithStroke()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("StrokeSolidColor");
		App.Tap("StrokeSolidColor");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithStroke()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("StrokeLinearGradient");
		App.Tap("StrokeLinearGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithStroke()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("StrokeRadialGradient");
		App.Tap("StrokeRadialGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
	}

	[Test]
	public void VerifySolidColorBrushWithOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacitySolidColor");
		App.Tap("OpacitySolidColor");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
	}

	[Test]
	public void VerifyLinearGradientBrushWithOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityLinearGradient");
		App.Tap("OpacityLinearGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
	}

	[Test]
	public void VerifyRadialGradientBrushWithOpacity()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityRadialGradient");
		App.Tap("OpacityRadialGradient");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Brushes Control Test");
		VerifyScreenshot();
	}

	public void VerifySolidColorBrushWithBackgroundColorAndShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundSolidColor");
		App.Tap("BackgroundSolidColor");
		App.WaitForElement("ShadowSolidColor");
		App.Tap("ShadowSolidColor");
		VerifyScreenshot();
	}
}