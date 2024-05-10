using Maui.Controls.Sample;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
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
		public void PointerGestureTest()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS },
				"PointerGestureRecognizer doesn't work with mouse in Android or iOS");

			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "PointerGestureRecognizerEvents");
			App.Tap("GoButton");

			App.WaitForElement("primaryLabel");
			// using Tap in place of moving mouse for now
			App.Tap("primaryLabel");
			App.Tap("secondaryLabel");

			var secondaryLabelText = App.FindElement("secondaryLabel").GetText();
			Assert.IsNotEmpty(secondaryLabelText);
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
			Assert.AreEqual("Success", result);
		}
	}
}

