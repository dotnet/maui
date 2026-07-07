// Material3 ProgressBar tests reuse the existing ProgressBar Feature Matrix HostApp page.
// The native Android view differs (LinearProgressIndicator vs ProgressBar), so these tests
// produce separate screenshot baselines under the Material3 category.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3ProgressBarFeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "ProgressBar Feature Matrix";

	public Material3ProgressBarFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.Material3)]
	public void Material3ProgressBar_ProgressToMethod_VerifyVisualState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("ProgressToEntry");
		App.ClearText("ProgressToEntry");
		App.EnterText("ProgressToEntry", "0.90");
		App.PressEnter();
		App.WaitForElement("ProgressToButton");
		App.Tap("ProgressToButton");
		Task.Delay(1000).Wait();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3ProgressBar_SetProgressOutOfRange()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("ProgressEntry");
		App.ClearText("ProgressEntry");
		App.EnterText("ProgressEntry", "1.44");
		App.PressEnter();
		App.WaitForElement("ProgressBarControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3ProgressBar_SetProgressNegativeValue()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("ProgressEntry");
		App.ClearText("ProgressEntry");
		App.EnterText("ProgressEntry", "-0.44");
		App.PressEnter();
		App.WaitForElement("ProgressBarControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3ProgressBar_SetProgressColorAndBackgroundColor_VerifyVisualState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("ProgressEntry");
		App.ClearText("ProgressEntry");
		App.EnterText("ProgressEntry", "0.60");
		App.PressEnter();
		App.WaitForElement("ProgressColorRedButton");
		App.Tap("ProgressColorRedButton");
		App.WaitForElement("BackgroundColorLightBlueButton");
		App.Tap("BackgroundColorLightBlueButton");
		App.WaitForElement("ProgressBarControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3ProgressBar_ChangeFlowDirection_RTL_VerifyLabel()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("ProgressBarControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3ProgressBar_ToggleShadow_VerifyVisualState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("ShadowTrueRadio");
		App.Tap("ShadowTrueRadio");
		App.WaitForElement("ProgressBarControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
