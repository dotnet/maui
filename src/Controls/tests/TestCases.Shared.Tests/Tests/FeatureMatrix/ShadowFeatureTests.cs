using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class ShadowFeatureTests : UITest
	{
		public const string ShadowFeatureMatrix = "Shadow Feature Matrix";

		public ShadowFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(ShadowFeatureMatrix);
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetColor()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("ColorEntry");
			App.EnterText("ColorEntry", "#00FF00");
			App.PressEnter();
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetColor_UpdateValue()
		{
			Exception? exception = null;

			App.WaitForElement("ShadowContainer");
			App.WaitForElement("ColorEntry");
			App.EnterText("ColorEntry", "#00FF00");
			App.PressEnter();
			VerifyScreenshotOrSetException(ref exception, "ShadowUpdateColor1");

			App.EnterText("ColorEntry", "#00FFFF");
			App.PressEnter();
			VerifyScreenshotOrSetException(ref exception, "ShadowUpdateColor2");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetOffset_PositiveValues()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("OffsetXEntry");
			App.EnterText("OffsetXEntry", "20");
			App.EnterText("OffsetYEntry", "20");

			Assert.That(App.FindElement("OffsetXEntry").GetText(), Is.EqualTo("OffsetX: 20"));
			Assert.That(App.FindElement("OffsetYEntry").GetText(), Is.EqualTo("OffsetY: 20"));
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetOffset_NegativeValues()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("OffsetXEntry");
			App.EnterText("OffsetXEntry", "-20");
			App.EnterText("OffsetYEntry", "-20");

			Assert.That(App.FindElement("OffsetXEntry").GetText(), Is.EqualTo("OffsetX: -20"));
			Assert.That(App.FindElement("OffsetYEntry").GetText(), Is.EqualTo("OffsetY: -20"));
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetRadius()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("RadiusEntry");
			App.EnterText("RadiusEntry", "20");

			Assert.That(App.FindElement("RadiusEntry").GetText(), Is.EqualTo("Radius: 20"));
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetRadius_Zero()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("RadiusEntry");
			App.EnterText("RadiusEntry", "0");

			Assert.That(App.FindElement("RadiusEntry").GetText(), Is.EqualTo("Radius: 0"));
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetOpacity()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("OpacityEntry");
			App.EnterText("OpacityEntry", "0.5");

			Assert.That(App.FindElement("OpacityEntry").GetText(), Is.EqualTo("Opacity: 0.5"));
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetOpacity_Zero()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("OpacityEntry");
			App.EnterText("OpacityEntry", "0");

			Assert.That(App.FindElement("OpacityEntry").GetText(), Is.EqualTo("Opacity: 0"));
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetEnabledStateToFalse_VerifyScreenshot()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("IsEnabledFalseRadio");
			App.Tap("IsEnabledFalseRadio");
			App.WaitForElement("ShadowContainer");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_ChangeFlowDirection_RTL_VerifyScreenshot()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("ShadowContainer");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetVisibilityToFalse_VerifyScreenshot()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("IsVisibleFalseRadio");
			App.Tap("IsVisibleFalseRadio");
			App.WaitForElement("ShadowContainer");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_AddClip_VerifyShadow()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("ClipButton");
			App.Tap("ClipButton");
			App.WaitForElement("ShadowContainer");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_AddRemoveClip_VerifyShadow()
		{
			Exception? exception = null;

			App.WaitForElement("ShadowContainer");
			App.WaitForElement("ClipButton");
			App.Tap("ClipButton");
			App.WaitForElement("ShadowContainer");
			VerifyScreenshotOrSetException(ref exception, "ShadowAddClip");
			
			App.Tap("ClipButton");
			VerifyScreenshotOrSetException(ref exception, "ShadowRemoveClip");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_Remove_AtRuntime()
		{
			App.WaitForElement("ShadowContainer");
			App.WaitForElement("ShadowButton");
			App.Tap("ShadowButton");
			App.WaitForElement("ShadowContainer");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_RemoveAdd_AtRuntime()
		{
			Exception? exception = null;

			App.WaitForElement("ShadowContainer");
			App.WaitForElement("ShadowButton");
			App.Tap("ShadowButton");
			App.WaitForElement("ShadowContainer");
			VerifyScreenshotOrSetException(ref exception, "RemoveShadowRuntime");

			App.Tap("ShadowButton");
			VerifyScreenshotOrSetException(ref exception, "AddShadowRuntime");

			if (exception != null)
			{
				throw exception;
			}
		}
	}
}