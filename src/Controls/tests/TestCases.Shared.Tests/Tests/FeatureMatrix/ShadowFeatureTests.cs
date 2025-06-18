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

		[Test, Order(1)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetColor()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("ColorEntry");
			App.EnterText("ColorEntry", "#00FF00");

			App.WaitForElement("LabelShadow").Tap();
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetColor_UpdateValue()
		{
			Exception? exception = null;

			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("ColorEntry");
			App.ClearText("ColorEntry");
			App.EnterText("ColorEntry", "#00FF00");

			App.WaitForElement("LabelShadow").Tap();
			VerifyScreenshotOrSetException(ref exception, "ShadowUpdateColor1");

			App.ClearText("ColorEntry");
			App.EnterText("ColorEntry", "#00FFFF");

			App.WaitForElement("LabelShadow").Tap();
			VerifyScreenshotOrSetException(ref exception, "ShadowUpdateColor2");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test, Order(3)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetOffset_PositiveValues()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("OffsetXEntry");
			App.ClearText("OffsetXEntry");
			App.EnterText("OffsetXEntry", "20");

			App.WaitForElement("OffsetYEntry");
			App.ClearText("OffsetYEntry");
			App.EnterText("OffsetYEntry", "20");

			Assert.That(App.FindElement("OffsetXEntry").GetText(), Is.EqualTo("20"));
			Assert.That(App.FindElement("OffsetYEntry").GetText(), Is.EqualTo("20"));

			App.WaitForElement("LabelShadow").Tap();
			VerifyScreenshot();
		}

		[Test, Order(4)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetOffset_Zero()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("OffsetXEntry");
			App.ClearText("OffsetXEntry");
			App.EnterText("OffsetXEntry", "0");

			App.ClearText("OffsetYEntry");
			App.EnterText("OffsetYEntry", "0");

			Assert.That(App.FindElement("OffsetXEntry").GetText(), Is.EqualTo("0"));
			Assert.That(App.FindElement("OffsetYEntry").GetText(), Is.EqualTo("0"));

			App.WaitForElement("LabelShadow").Tap();
			VerifyScreenshot();
		}

		[Test, Order(5)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetRadius()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("RadiusEntry");
			App.ClearText("RadiusEntry");
			App.EnterText("RadiusEntry", "20");

			Assert.That(App.FindElement("RadiusEntry").GetText(), Is.EqualTo("20"));

			App.WaitForElement("LabelShadow").Tap();
			VerifyScreenshot();
		}

		[Test, Order(6)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetRadius_Zero()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("RadiusEntry");
			App.ClearText("RadiusEntry");
			App.EnterText("RadiusEntry", "0");

			Assert.That(App.FindElement("RadiusEntry").GetText(), Is.EqualTo("0"));

			App.WaitForElement("LabelShadow").Tap();
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetOpacity()
		{
			App.WaitForElement("ResetButton").Tap();
			App.ClearText("OpacityEntry");
			App.WaitForElement("OpacityEntry");
			App.EnterText("OpacityEntry", "1");

			Assert.That(App.FindElement("OpacityEntry").GetText(), Is.EqualTo("1"));

			App.WaitForElement("LabelShadow").Tap();
			VerifyScreenshot();
		}

		[Test, Order(7)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetOpacity_Zero()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("OpacityEntry");
			App.ClearText("OpacityEntry");
			App.EnterText("OpacityEntry", "0");

			Assert.That(App.FindElement("OpacityEntry").GetText(), Is.EqualTo("0"));

			App.WaitForElement("LabelShadow").Tap();
			VerifyScreenshot();
		}

		[Test, Order(7)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetEnabledStateToFalse_VerifyScreenshot()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("IsEnabledFalseRadio");
			App.Tap("IsEnabledFalseRadio");
			VerifyScreenshot();
		}

		[Test, Order(8)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_ChangeFlowDirection_RTL_VerifyScreenshot()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			VerifyScreenshot();
		}

		[Test, Order(9)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_SetVisibilityToFalse_VerifyScreenshot()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("IsVisibleFalseRadio");
			App.Tap("IsVisibleFalseRadio");
			VerifyScreenshot();
		}

#if !WINDOWS // Shadow not updated when Clipping a View: https://github.com/dotnet/maui/issues/27730
		[Test, Order(10)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_AddClip_VerifyShadow()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("ClipButton");
			App.Tap("ClipButton");
			VerifyScreenshot();
		}

		[Test, Order(11)]
		[Category(UITestCategories.Shadow)]
		public void Shadow_AddRemoveClip_VerifyShadow()
		{
			Exception? exception = null;

			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("ClipButton");
			App.Tap("ClipButton");
			VerifyScreenshotOrSetException(ref exception, "ShadowAddClip");

			App.Tap("ClipButton");
			VerifyScreenshotOrSetException(ref exception, "ShadowRemoveClip");

			if (exception != null)
			{
				throw exception;
			}
		}
#endif

#if !IOS && !WINDOWS // Fails iOS: https://github.com/dotnet/maui/issues/27879 and Windows: https://github.com/dotnet/maui/issues/27732
		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_Remove_AtRuntime()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("ShadowButton");
			App.Tap("ShadowButton");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Shadow_RemoveAdd_AtRuntime()
		{
			Exception? exception = null;

			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("ShadowButton");
			App.Tap("ShadowButton");
			VerifyScreenshotOrSetException(ref exception, "RemoveShadowRuntime");

			App.Tap("ShadowButton");
			VerifyScreenshotOrSetException(ref exception, "AddShadowRuntime");

			if (exception != null)
			{
				throw exception;
			}
		}
#endif

		[Test]
		[Category(UITestCategories.Shadow)]
		[Ignore("Fails on all the platforms. Get lower fps especially on Android and Windows (below 24 fps at times).")]
		public void Shadow_Resize_Benchmark()
		{
			App.WaitForElement("ResetButton").Tap();
			App.WaitForElement("BenchmarkButton");
			App.Tap("BenchmarkButton");

			const double MinimumFps = 50;

			Thread.Sleep(500);
			double fps1 = Convert.ToDouble(App.FindElement("FpsLabel").GetText());
			Assert.That(fps1, Is.GreaterThanOrEqualTo(MinimumFps));

			Thread.Sleep(500);
			double fps2 = Convert.ToDouble(App.FindElement("FpsLabel").GetText());
			Assert.That(fps2, Is.GreaterThanOrEqualTo(MinimumFps));

			Thread.Sleep(500);
			double fps3 = Convert.ToDouble(App.FindElement("FpsLabel").GetText());
			Assert.That(fps3, Is.GreaterThanOrEqualTo(MinimumFps));

			App.Tap("BenchmarkButton");
		}
	}
}