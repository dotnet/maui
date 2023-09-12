using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using TestUtils.Appium.UITests;
using Xamarin.UITest;

namespace Microsoft.Maui.AppiumTests
{
	public class GestureRecognizerUITests : UITestBase
	{
		const string GestureRecognizerGallery = "* marked:'Gesture Recognizer Gallery'";
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
			App.NavigateBack();
		}

		[Test]
		public void PointerGestureTest()
		{
			UITestContext.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS },
				"PointerGestureRecognizer doesn't work with mouse in Android or iOS");

			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "PointerGestureRecognizerEvents");
			App.Tap("GoButton");

			App.WaitForElement("primaryLabel");
			// using Tap in place of moving mouse for now
			App.Tap("primaryLabel");
			App.Tap("secondaryLabel");

			var secondaryLabelText = App.Query("secondaryLabel").First().Text;
			Assert.IsNotEmpty(secondaryLabelText);
		}

		[Test]
		public void DoubleTap()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DoubleTapGallery");
			App.Tap("GoButton");

			App.WaitForElement("DoubleTapSurface");
			App.DoubleTap("DoubleTapSurface");

			var result = App.Query("DoubleTapResults").First().Text;
			Assert.AreEqual("Success", result);
		}
	}
}

