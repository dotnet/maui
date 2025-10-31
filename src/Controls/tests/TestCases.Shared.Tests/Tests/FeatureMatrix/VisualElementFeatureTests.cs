using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Visual)]
public class VisualElementFeatureTests : UITest
{
	public const string VisualElementFeatureMatrix = "VisualElement Feature Matrix";

	public VisualElementFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(VisualElementFeatureMatrix);
	}

	[Test]
	public void VisualElement_IsVisible()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("VisibilityCheckBox");
		App.Tap("VisibilityCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualElement_IsShadow()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}
	[Test]
	public void VisualElement_Rotation()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("Preset45RotateButton");
		App.Tap("Preset45RotateButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}
	[Test]
	public void VisualElement_Scale()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("Preset2xScaleButton");
		App.Tap("Preset2xScaleButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualElement_ScaleX()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ScaleXEntry");
		App.ClearText("ScaleXEntry");
		App.EnterText("ScaleXEntry", "2");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualElement_Scaley()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ScaleYEntry");
		App.ClearText("ScaleYEntry");
		App.EnterText("ScaleYEntry", "2");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}
}