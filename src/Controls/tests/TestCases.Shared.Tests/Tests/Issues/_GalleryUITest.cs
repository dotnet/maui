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
            const int maxRetries = 3;
            const int delayBetweenRetries = 2000; // 2 seconds
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    App.LaunchApp(GalleryPageName, ResetAfterEachTest);
                    return; // Success, exit retry loop
                }
                catch (Exception)
                {
                    if (attempt == maxRetries)
                    {
                        TestContext.WriteLine($"Failed to launch app after {maxRetries} attempts.");
                        throw; // Final attempt failed, rethrow the original exception
                    }
                    
                    // Wait before retrying
                    System.Threading.Thread.Sleep(delayBetweenRetries);
                }
            }
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