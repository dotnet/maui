using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
    public abstract class _GalleryUITest : UITest
    {
        public _GalleryUITest(TestDevice device) : base(device) { }

        public override IConfig GetTestConfig()
        {
            var config = base.GetTestConfig();

#if MACCATALYST
            // For Catalyst, pass the test name as a startup argument
            // If the UITestContext is not null we can directly pass the Issue via LaunchAppWithTest
            if (UITestContext is null)
            {
                config.SetTestConfigurationArg("test", GalleryPageName);
            }
#endif

            return config;
        }

        public override void LaunchAppWithTest()
        {
            App.LaunchApp(GalleryPageName, ResetAfterEachTest);
        }

        protected override void FixtureSetup()
        {
            base.FixtureSetup();
            if (Device is not TestDevice.Mac)
                App.NavigateToGallery(GalleryPageName);
        }

        public abstract string GalleryPageName { get; }
    }
}
