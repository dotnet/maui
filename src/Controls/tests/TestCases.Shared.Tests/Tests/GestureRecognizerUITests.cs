using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class GestureRecognizerUITests : UITest
	{
		const string GestureRecognizerGallery = "Gesture Recognizer Gallery";
		public GestureRecognizerUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(GestureRecognizerGallery);
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		[FailsOnAndroidWhenRunningOnXamarinUITest("PointerGestureRecognizer doesn't work with mouse in Android")]
		[FailsOnIOSWhenRunningOnXamarinUITest("PointerGestureRecognizer doesn't work with mouse in iOS")]
		public void PointerGestureTest()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "PointerGestureRecognizerEvents");
			App.Tap("GoButton");

			App.WaitForElement("primaryLabel");
			// using Tap in place of moving mouse for now
			App.Tap("primaryLabel");
			App.Tap("secondaryLabel");

			var secondaryLabelText = App.FindElement("secondaryLabel").GetText();
			ClassicAssert.IsNotEmpty(secondaryLabelText);
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void DoubleTap()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DoubleTapGallery");
			App.Tap("GoButton");

			App.WaitForElement("DoubleTapSurface");
			App.DoubleTap("DoubleTapSurface");

			var result = App.FindElement("DoubleTapResults").GetText();
			ClassicAssert.AreEqual("Success", result);
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void SingleTap()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "SingleTapGallery");
			App.Tap("GoButton");

			App.WaitForElement("SingleTapSurface");
			App.Tap("SingleTapSurface");

			var result = App.FindElement("SingleTapGestureResults").GetText();
			ClassicAssert.AreEqual("Success", result);
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void DisabledSingleTap()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "SingleTapGallery");
			App.Tap("GoButton");

			App.WaitForElement("DisabledTapSurface");
			App.Tap("DisabledTapSurface");

			var result = App.FindElement("DisabledTapGestureResults").GetText();
			ClassicAssert.AreNotEqual("Failed", result);
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void DynamicallyAddedTapGesturesDontCauseMultipleTapEvents()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DynamicTapGestureGallery");
			App.Tap("GoButton");

			App.WaitForElement("DynamicTapSurface");
			App.Tap("DynamicTapSurface");
			App.Tap("DynamicTapSurface");
			App.Tap("DynamicTapSurface");

			var result = App.FindElement("DynamicTapGestureResults").GetText();

			if (int.TryParse(result, out var resultInt))
			{
				ClassicAssert.AreEqual(3, resultInt);
			}
			else
			{
				ClassicAssert.Fail("Failed to parse result as int");
			}
		}
	}
}