using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Visual)]
public class VisualTransformFeatureTests : UITest
{
	public const string VisualTransformFeatureMatrix = "VisualTransform Feature Matrix";

	public VisualTransformFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(VisualTransformFeatureMatrix);
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_ANDROID //For iOS & macOS, see issue: https://github.com/dotnet/maui/issues/32767 and for Android, see issue: https://github.com/dotnet/maui/issues/32731

	[Test]
	public void VisualTransform_ScaleXWithShadow()
	{
	 App.WaitForElement("Reset");
	 App.Tap("Reset");

	 App.WaitForElement("Options");
	 App.Tap("Options");

	 App.WaitForElement("ScaleXEntry");
	 App.ClearText("ScaleXEntry");
	 App.EnterText("ScaleXEntry", "2");

	 App.WaitForElement("ShadowCheckBox");
	 App.Tap("ShadowCheckBox");

	 App.WaitForElement("Apply");
	 App.Tap("Apply");

	 VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_AnchorYWithShadow()
	{
	 App.WaitForElement("Reset");
	 App.Tap("Reset");

	 App.WaitForElement("Options");
	 App.Tap("Options");

	 App.WaitForElement("RotationEntry");
	 App.ClearText("RotationEntry");
	 App.EnterText("RotationEntry", "45");

	 App.WaitForElement("AnchorYEntry");
	 App.ClearText("AnchorYEntry");
	 App.EnterText("AnchorYEntry", "1");

	 App.WaitForElement("ShadowCheckBox");
	 App.Tap("ShadowCheckBox");

	 App.WaitForElement("Apply");
	 App.Tap("Apply");

	 VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_IsShadow()
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
	public void VisualTransform_RotationYWithShadow()
	{
	 App.WaitForElement("Reset");
	 App.Tap("Reset");

	 App.WaitForElement("Options");
	 App.Tap("Options");

	 App.WaitForElement("RotationYEntry");
	 App.ClearText("RotationYEntry");
	 App.EnterText("RotationYEntry", "60");

	 App.WaitForElement("ShadowCheckBox");
	 App.Tap("ShadowCheckBox");

	 App.WaitForElement("Apply");
	 App.Tap("Apply");

	 VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_ScaleWithShadow()
	{
	 App.WaitForElement("Reset");
	 App.Tap("Reset");

	 App.WaitForElement("Options");
	 App.Tap("Options");

	 App.WaitForElement("ScaleEntry");
	 App.ClearText("ScaleEntry");
	 App.EnterText("ScaleEntry", "2");

	 App.WaitForElement("ShadowCheckBox");
	 App.Tap("ShadowCheckBox");

	 App.WaitForElement("Apply");
	 App.Tap("Apply");

	 VerifyScreenshot();
	}


	[Test]
	public void VisualTransform_RotationXWithShadow()
	{
	 App.WaitForElement("Reset");
	 App.Tap("Reset");

	 App.WaitForElement("Options");
	 App.Tap("Options");

	 App.WaitForElement("RotationXEntry");
	 App.ClearText("RotationXEntry");
	 App.EnterText("RotationXEntry", "50");

	 App.WaitForElement("ShadowCheckBox");
	 App.Tap("ShadowCheckBox");

	 App.WaitForElement("Apply");
	 App.Tap("Apply");

	 VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_ScaleYWithShadow()
	{
	 App.WaitForElement("Reset");
	 App.Tap("Reset");

	 App.WaitForElement("Options");
	 App.Tap("Options");

	 App.WaitForElement("ScaleYEntry");
	 App.ClearText("ScaleYEntry");
	 App.EnterText("ScaleYEntry", "2");

	 App.WaitForElement("ShadowCheckBox");
	 App.Tap("ShadowCheckBox");

	 App.WaitForElement("Apply");
	 App.Tap("Apply");

	 VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_AnchorXWithShadow()
	{
	 App.WaitForElement("Reset");
	 App.Tap("Reset");

	 App.WaitForElement("Options");
	 App.Tap("Options");

	 App.WaitForElement("RotationEntry");
	 App.ClearText("RotationEntry");
	 App.EnterText("RotationEntry", "45");

	 App.WaitForElement("AnchorXEntry");
	 App.ClearText("AnchorXEntry");
	 App.EnterText("AnchorXEntry", "1");

	 App.WaitForElement("ShadowCheckBox");
	 App.Tap("ShadowCheckBox");

	 App.WaitForElement("Apply");
	 App.Tap("Apply");

	 VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_RotationWithShadow()
	{
	 App.WaitForElement("Reset");
	 App.Tap("Reset");

	 App.WaitForElement("Options");
	 App.Tap("Options");

	 App.WaitForElement("RotationEntry");
	 App.ClearText("RotationEntry");
	 App.EnterText("RotationEntry", "45");

	 App.WaitForElement("ShadowCheckBox");
	 App.Tap("ShadowCheckBox");

	 App.WaitForElement("Apply");
	 App.Tap("Apply");

	 VerifyScreenshot();
	}
#endif

	[Test]
	public void VisualTransform_IsVisible()
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
	public void VisualTransform_Rotation()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "45");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_RotationWithRotationX()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "45");

		App.WaitForElement("RotationXEntry");
		App.ClearText("RotationXEntry");
		App.EnterText("RotationXEntry", "30");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_RotationWithScale()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "55");

		App.WaitForElement("ScaleEntry");
		App.ClearText("ScaleEntry");
		App.EnterText("ScaleEntry", "0.5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_RotationXWithScaleX()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationXEntry");
		App.ClearText("RotationXEntry");
		App.EnterText("RotationXEntry", "55");

		App.WaitForElement("ScaleXEntry");
		App.ClearText("ScaleXEntry");
		App.EnterText("ScaleXEntry", "1.5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_RotationXWithRotationY()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationXEntry");
		App.ClearText("RotationXEntry");
		App.EnterText("RotationXEntry", "55");

		App.WaitForElement("RotationYEntry");
		App.ClearText("RotationYEntry");
		App.EnterText("RotationYEntry", "25");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_RotationYWithScaleY()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationYEntry");
		App.ClearText("RotationYEntry");
		App.EnterText("RotationYEntry", "55");

		App.WaitForElement("ScaleYEntry");
		App.ClearText("ScaleYEntry");
		App.EnterText("ScaleYEntry", "1.5");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_RotationX()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationXEntry");
		App.ClearText("RotationXEntry");
		App.EnterText("RotationXEntry", "45");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_RotationY()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationYEntry");
		App.ClearText("RotationYEntry");
		App.EnterText("RotationYEntry", "45");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_Scale()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ScaleEntry");
		App.ClearText("ScaleEntry");
		App.EnterText("ScaleEntry", "2");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_ScaleWithAnchorX()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ScaleEntry");
		App.ClearText("ScaleEntry");
		App.EnterText("ScaleEntry", "2");

		App.WaitForElement("RotationYEntry");
		App.ClearText("RotationYEntry");
		App.EnterText("RotationYEntry", "45");

		App.WaitForElement("AnchorXEntry");
		App.ClearText("AnchorXEntry");
		App.EnterText("AnchorXEntry", "1");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_ScaleXWithAnchorY()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ScaleXEntry");
		App.ClearText("ScaleXEntry");
		App.EnterText("ScaleXEntry", "2");

		App.WaitForElement("RotationYEntry");
		App.ClearText("RotationYEntry");
		App.EnterText("RotationYEntry", "45");

		App.WaitForElement("AnchorYEntry");
		App.ClearText("AnchorYEntry");
		App.EnterText("AnchorYEntry", "1");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_ScaleYWithAnchorX()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ScaleYEntry");
		App.ClearText("ScaleYEntry");
		App.EnterText("ScaleYEntry", "1.5");

		App.WaitForElement("RotationYEntry");
		App.ClearText("RotationYEntry");
		App.EnterText("RotationYEntry", "45");

		App.WaitForElement("AnchorXEntry");
		App.ClearText("AnchorXEntry");
		App.EnterText("AnchorXEntry", "1");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_ScaleX()
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
	public void VisualTransform_ScaleY()
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

	[Test]
	public void VisualTransform_AnchorX_ScaleYWithRotation()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "45");

		App.WaitForElement("ScaleYEntry");
		App.ClearText("ScaleYEntry");
		App.EnterText("ScaleYEntry", "1.5");

		App.WaitForElement("AnchorXEntry");
		App.ClearText("AnchorXEntry");
		App.EnterText("AnchorXEntry", "0.9");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_AnchorY_ScaleWithRotationY()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationYEntry");
		App.ClearText("RotationYEntry");
		App.EnterText("RotationYEntry", "45");

		App.WaitForElement("ScaleEntry");
		App.ClearText("ScaleEntry");
		App.EnterText("ScaleEntry", "1.5");

		App.WaitForElement("AnchorYEntry");
		App.ClearText("AnchorYEntry");
		App.EnterText("AnchorYEntry", "0.7");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_AnchorY_ScaleXWithRotationX()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationXEntry");
		App.ClearText("RotationXEntry");
		App.EnterText("RotationXEntry", "45");

		App.WaitForElement("ScaleXEntry");
		App.ClearText("ScaleXEntry");
		App.EnterText("ScaleXEntry", "1.5");

		App.WaitForElement("AnchorYEntry");
		App.ClearText("AnchorYEntry");
		App.EnterText("AnchorYEntry", "0.7");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VisualTransform_AnchorXWithAnchorY()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RotationEntry");
		App.ClearText("RotationEntry");
		App.EnterText("RotationEntry", "45");

		App.WaitForElement("AnchorXEntry");
		App.ClearText("AnchorXEntry");
		App.EnterText("AnchorXEntry", "1");

		App.WaitForElement("AnchorYEntry");
		App.ClearText("AnchorYEntry");
		App.EnterText("AnchorYEntry", "1");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}
}