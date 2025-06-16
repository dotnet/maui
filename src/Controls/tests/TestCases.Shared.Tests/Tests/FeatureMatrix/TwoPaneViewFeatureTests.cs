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

        [Test, Order(1)]
        [Category("TwoPaneView")]
        public void TwoPaneView()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category("TwoPaneView")]
        public void TwoPaneView_Pane1Size()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("Pane1LengthStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category("TwoPaneView")]
        public void TwoPaneView_Pane2Size()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("Pane2LengthStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }
        
        [Test]
        [Category("TwoPaneView")]
        public void TwoPaneView_Pane1SmallerThanPane2()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("Pane2LengthStepper");
            App.DecreaseStepper("Pane1LengthStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category("TwoPaneView")]
        public void TwoPaneView_Pane2SmallerThanPane1()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("Pane1LengthStepper");
            App.DecreaseStepper("Pane2LengthStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category("TwoPaneView")]
        public void TwoPaneView_IsVisible()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.WaitForElement("VisibilityCheckBox");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

#if ANDROID || IOS
        [Test]
        [Category("TwoPaneView")]
        public void TwoPaneView_Height()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("TallModeStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }
#endif

#if MACCATALYST || WINDOWS

        [Test]
        [Category("TwoPaneView")]
        public void TwoPaneView_HeightAndWidth()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("TallModeStepper");

            App.IncreaseStepper("WideModeStepper");
            App.IncreaseStepper("WideModeStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category("TwoPaneView")]
        public void TwoPaneView_RTLFlowDirection()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.WaitForElement("FlowDirectionRTLCheckBox");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }

        [Test]
        [Category("TwoPaneView")]
        public void TwoPaneView_Width()
        {
            App.WaitForElement("Options");
            App.Tap("Options");

            App.IncreaseStepper("WideModeStepper");
            App.IncreaseStepper("WideModeStepper");

            App.WaitForElement("Apply");
            App.Tap("Apply");

            VerifyScreenshot();
        }
#endif
    }
}