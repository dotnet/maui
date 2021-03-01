using System.Reflection;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Essentials.DeviceTests.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : Xunit.Runner.RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // We need this to ensure the execution assembly is part of the app bundle
            AddExecutionAssembly(typeof(Battery_Tests).Assembly);

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            AddTestAssembly(typeof(Battery_Tests).Assembly);

            return base.FinishedLaunching(app, options);
        }
    }
}
