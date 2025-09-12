using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
	public class TwoPaneViewFeatureTests : UITest
	{
		public const string TwoPaneViewFeatureMatrix = "TwoPaneView Feature Matrix";

		public TwoPaneViewFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(TwoPaneViewFeatureMatrix);
		}

		// The test cases below are divided based on platform-specific UI behaviors and layout logic.
		//
		// For MACCATALYST and WINDOWS:
		// - Increase WideModeStepper to switch to Tall Mode (Pane1 above Pane2)
		// - Use Y position to validate layout changes
		//
		// For ANDROID and IOS:
		// - Decrease WideModeStepper to switch to Wide Mode (Pane1 beside Pane2)
		// - Use X position to validate layout changes
		// - Extra checks for RTL and shadow are included
		//
		// Common tests:
		// - FlowDirection, visibility, shadow, and pane size changes are tested on all platforms
		//   with minor differences in interactions.

#if MACCATALYST || WINDOWS

        [Test]
        [Category(UITestCategories.Layout)]
        public void TwoPaneView_RTLFlowDirection()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.WaitForElement("FlowDirectionRTLCheckBox");
            App.Tap("FlowDirectionRTLCheckBox");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.Layout)]
        public void TwoPaneView_WideMode()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.Layout)]
        public void TwoPaneView_IsTall_UsingRect()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("WideModeStepper");
            App.IncreaseStepper("WideModeStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            var pane1Y = App.WaitForElement("Pane1Label").GetRect().Y;
            var pane2Y = App.WaitForElement("Pane2Label").GetRect().Y;

            Assert.That(pane2Y, Is.GreaterThan(pane1Y), "Pane2 should be below Pane1 in Tall mode");

            Assert.That(App.WaitForElement("CurrentModeLabel").GetText(), Is.EqualTo("Tall Mode"), "CurrentModeLabel should display 'Tall Mode'");
        }

        [Test]
        [Category(UITestCategories.Layout)]
        public void TwoPaneView_Wide_UsingRect()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            var pane1Y = App.WaitForElement("Pane1Label").GetRect().X;
            var pane2Y = App.WaitForElement("Pane2Label").GetRect().X;

            Assert.That(pane2Y, Is.GreaterThan(pane1Y), "Pane2 should be to the right of Pane1 in Wide mode");

            Assert.That(App.WaitForElement("CurrentModeLabel").GetText(), Is.EqualTo("Wide Mode"));
        }

        [Test]
        [Category(UITestCategories.Layout)]
        public void TwoPaneView_IsWideWithRTL_UsingRect()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.DecreaseStepper("WideModeStepper");
            App.DecreaseStepper("WideModeStepper");

            App.WaitForElement("FlowDirectionRTLCheckBox");
            App.Tap("FlowDirectionRTLCheckBox");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            var pane1X = App.WaitForElement("Pane1Label").GetRect().X;
            var pane2X = App.WaitForElement("Pane2Label").GetRect().X;

            Assert.That(pane1X, Is.GreaterThan(pane2X), "Pane1 should be to the right of Pane2 in RTL Wide mode");

            Assert.That(App.WaitForElement("CurrentModeLabel").GetText(), Is.EqualTo("Wide Mode"), "CurrentModeLabel should display 'Wide Mode'");
        }

        [Test]
        [Category(UITestCategories.Layout)]
        public void TwoPaneView_TallMode()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("WideModeStepper");
            App.IncreaseStepper("WideModeStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.Layout)]
        public void TwoPaneView_Pane1Priority()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.WaitForElement("TallModeSinglePaneRadio");
            App.Tap("TallModeSinglePaneRadio");

            App.WaitForElement("WideModeSinglePaneRadio");
            App.Tap("WideModeSinglePaneRadio");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.Layout)]
        public void TwoPaneView_Pane2Priority()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.WaitForElement("TallModeSinglePaneRadio");
            App.Tap("TallModeSinglePaneRadio");

            App.WaitForElement("WideModeSinglePaneRadio");
            App.Tap("WideModeSinglePaneRadio");

            App.WaitForElement("PanePriorityPane2Radio");
            App.Tap("PanePriorityPane2Radio");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.Layout)]
        public void TwoPaneView_Pane1SizeIncrease_WithTallMode()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("Pane1LengthStepper");
            App.IncreaseStepper("Pane1LengthStepper");

            App.IncreaseStepper("WideModeStepper");
            App.IncreaseStepper("WideModeStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.Layout)]
        public void TwoPaneView_Pane2SizeIncrease_WithTallMode()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("Pane2LengthStepper");
            App.IncreaseStepper("Pane2LengthStepper");

            App.IncreaseStepper("WideModeStepper");
            App.IncreaseStepper("WideModeStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

#endif
		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_IsVisible()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("VisibilityCheckBox");
			App.Tap("VisibilityCheckBox");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_ZIsShadowEnabled()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_Pane1SizeIncrease()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("Pane1LengthLabel");
			App.IncreaseStepper("Pane1LengthStepper");
			App.WaitForElement("Pane1LengthLabel");
			App.IncreaseStepper("Pane1LengthStepper");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_Pane2SizeIncrease()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("Pane2LengthLabel");
			App.IncreaseStepper("Pane2LengthStepper");
			App.WaitForElement("Pane2LengthLabel");
			App.IncreaseStepper("Pane2LengthStepper");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}

#if ANDROID || IOS

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_IsTall_UsingRect()
		{
			var pane1Y = App.WaitForElement("Pane1Label").GetRect().Y;
			var pane2Y = App.WaitForElement("Pane2Label").GetRect().Y;

			Assert.That(pane2Y, Is.GreaterThan(pane1Y), "Pane2 should be below Pane1 in Tall mode");

			Assert.That(App.WaitForElement("CurrentModeLabel").GetText(), Is.EqualTo("Tall Mode"), "CurrentModeLabel should display 'Tall Mode'");
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_IsWide_UsingRect()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");
			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			var pane1X = App.WaitForElement("Pane1Label").GetRect().X;
			var pane2X = App.WaitForElement("Pane2Label").GetRect().X;

			Assert.That(pane2X, Is.GreaterThan(pane1X), "Pane2 should be to the right of Pane1 in Wide mode");

			Assert.That(App.WaitForElement("CurrentModeLabel").GetText(), Is.EqualTo("Wide Mode"), "CurrentModeLabel should display 'Wide Mode'");

		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_IsWideWithRTL_UsingRect()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");
			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");

			App.WaitForElement("FlowDirectionRTLCheckBox");
			App.Tap("FlowDirectionRTLCheckBox");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			var pane1X = App.WaitForElement("Pane1Label").GetRect().X;
			var pane2X = App.WaitForElement("Pane2Label").GetRect().X;

			Assert.That(pane1X, Is.GreaterThan(pane2X), "Pane1 should be to the right of Pane2 in RTL Wide mode");

			Assert.That(App.WaitForElement("CurrentModeLabel").GetText(), Is.EqualTo("Wide Mode"), "CurrentModeLabel should display 'Wide Mode'");
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_TallMode()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_WideMode()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");
			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_Pane1SizeIncrease_WithWideMode()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("Pane1LengthStepper");
			App.IncreaseStepper("Pane1LengthStepper");
			App.WaitForElement("Pane1LengthStepper");
			App.IncreaseStepper("Pane1LengthStepper");

			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");
			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_Pane2SizeIncrease_WithWideMode()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("Pane2LengthStepper");
			App.IncreaseStepper("Pane2LengthStepper");
			App.WaitForElement("Pane2LengthStepper");
			App.IncreaseStepper("Pane2LengthStepper");

			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");
			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_ShadowWithWideMode()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("ShadowCheckBox");
			App.Tap("ShadowCheckBox");

			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");
			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_RTLFlowDirection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");
			App.WaitForElement("WideModeStepper");
			App.DecreaseStepper("WideModeStepper");

			App.WaitForElement("FlowDirectionRTLCheckBox");
			App.Tap("FlowDirectionRTLCheckBox");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}


		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_Pane1Priority()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("TallModeSinglePaneRadio");
			App.Tap("TallModeSinglePaneRadio");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void TwoPaneView_Pane2Priority()
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("TallModeSinglePaneRadio");
			App.Tap("TallModeSinglePaneRadio");

			App.WaitForElement("PanePriorityPane2Radio");
			App.Tap("PanePriorityPane2Radio");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			VerifyScreenshot();
		}
#endif
	}
}