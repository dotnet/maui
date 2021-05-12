using System.Reflection;
using Foundation;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	[Register(nameof(AppDelegate))]
	public partial class AppDelegate : Xunit.Runner.RunnerAppDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			// We need this to ensure the execution assembly is part of the app bundle
			AddExecutionAssembly(typeof(AppDelegate).Assembly);

			// tests can be inside the main assembly
			AddTestAssembly(typeof(AppDelegate).Assembly);
			AddTestAssembly(typeof(CompatTests).Assembly);
			AddTestAssembly(typeof(CoreTests).Assembly);

			return base.FinishedLaunching(app, options);
		}
	}
}