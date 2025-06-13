using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
    public class ProgressBarFeatureTests : UITest
    {
        public ProgressBarFeatureTests(TestDevice device)
            : base(device)
        {
        }

        protected override void FixtureSetup()
        {
            base.FixtureSetup();
            App.NavigateToGallery("ProgressBar Feature Matrix");
        }

        [Test, Order(1)]
        [Category(UITestCategories.ProgressBar)]
        public void ProgressBar_ValidateDefaultProgress()
        {
            App.WaitForElement("ProgressValueLabel");
            Assert.That(App.FindElement("ProgressValueLabel").GetText(), Is.EqualTo("0.50"));
        }

        [Test, Order(2)]
        [Category(UITestCategories.ProgressBar)]
        public void ProgressBar_SetProgress_VerifyLabel()
        {
            App.WaitForElement("Options");
            App.Tap("Options");
            App.WaitForElement("ProgressEntry");
            App.ClearText("ProgressEntry");
            App.EnterText("ProgressEntry", "0.80");
            App.WaitForElement("Apply");
            App.Tap("Apply");
            App.WaitForElementTillPageNavigationSettled("ProgressBarControl");
            App.WaitForElement("ProgressValueLabel");
            Assert.That(App.FindElement("ProgressValueLabel").GetText(), Is.EqualTo("0.80"));
        }

        [Test, Order(3)]
        [Category(UITestCategories.ProgressBar)]
        public void ProgressBar_ProgressToMethod_VerifyVisualState()
        {
            App.WaitForElement("ProgressToButton");
            App.Tap("ProgressToButton");
            Task.Delay(1000).Wait();
            // VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.ProgressBar)]
        public void ProgressBar_SetProgressOutOfRange()
        {
            App.WaitForElement("Options");
            App.Tap("Options");
            App.WaitForElement("ProgressEntry");
            App.EnterText("ProgressEntry", "1.44");
            App.PressEnter();
            App.WaitForElement("Apply");
            App.Tap("Apply");
            App.WaitForElementTillPageNavigationSettled("ProgressBarControl");
            // VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.ProgressBar)]
        public void ProgressBar_SetProgressNegativeValue()
        {
            App.WaitForElement("Options");
            App.Tap("Options");
            App.WaitForElement("ProgressEntry");
            App.EnterText("ProgressEntry", "-0.44");
            App.PressEnter();
            App.WaitForElement("Apply");
            App.Tap("Apply");
            App.WaitForElementTillPageNavigationSettled("ProgressBarControl");
            // VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.ProgressBar)]
        public void ProgressBar_SetProgressColorAndBackgroundColor_VerifyVisualState()
        {
            App.WaitForElement("Options");
            App.Tap("Options");
            App.WaitForElement("ProgressEntry");
            App.ClearText("ProgressEntry");
            App.EnterText("ProgressEntry", "0.60");
            App.WaitForElement("ProgressColorRedButton");
            App.Tap("ProgressColorRedButton");
            App.WaitForElement("BackgroundColorLightBlueButton");
            App.Tap("BackgroundColorLightBlueButton");
            App.WaitForElement("Apply");
            App.Tap("Apply");
            App.WaitForElementTillPageNavigationSettled("ProgressBarControl");
            // VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.ProgressBar)]
        public void ProgressBar_SetIsVisibleFalse_VerifyLabel()
        {
            App.WaitForElement("Options");
            App.Tap("Options");
            App.WaitForElement("IsVisibleFalseRadio");
            App.Tap("IsVisibleFalseRadio");
            App.WaitForElement("Apply");
            App.Tap("Apply");
            App.WaitForNoElement("ProgressBarControl");
            // VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.ProgressBar)]
        public void ProgressBar_ChangeFlowDirection_RTL_VerifyLabel()
        {
            App.WaitForElement("Options");
            App.Tap("Options");
            App.WaitForElement("FlowDirectionRTL");
            App.Tap("FlowDirectionRTL");
            App.WaitForElement("Apply");
            App.Tap("Apply");
            App.WaitForElementTillPageNavigationSettled("ProgressBarControl");
            // VerifyScreenshot();
        }

        [Test]
        [Category(UITestCategories.ProgressBar)]
        public void ProgressBar_ToggleShadow_VerifyVisualState()
        {
            App.WaitForElement("Options");
            App.Tap("Options");
            App.WaitForElement("ShadowTrueRadio");
            App.Tap("ShadowTrueRadio");
            App.WaitForElement("Apply");
            App.Tap("Apply");
            App.WaitForElementTillPageNavigationSettled("ProgressBarControl");
            // VerifyScreenshot();
        }
    }
}