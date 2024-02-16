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

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void PointerGestureTest()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS },
				"PointerGestureRecognizer doesn't work with mouse in Android or iOS");

			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "PointerGestureRecognizerEvents");
			App.Click("GoButton");

			App.WaitForElement("primaryLabel");
			// using Tap in place of moving mouse for now
			App.Click("primaryLabel");
			App.Click("secondaryLabel");

			var secondaryLabelText = App.FindElement("secondaryLabel").GetText();
			Assert.IsNotEmpty(secondaryLabelText);
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void DoubleTap()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DoubleTapGallery");
			App.Click("GoButton");

			App.WaitForElement("DoubleTapSurface");
			App.DoubleClick("DoubleTapSurface");

			var result = App.FindElement("DoubleTapResults").GetText();
			Assert.AreEqual("Success", result);
		}
	}
}

