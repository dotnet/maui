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
	}
}