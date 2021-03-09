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
			AddExecutionAssembly(typeof(SliderHandlerTests).Assembly);

			// tests can be inside the main assembly
			AddTestAssembly(Assembly.GetExecutingAssembly());
			AddTestAssembly(typeof(SliderHandlerTests).Assembly);

			return base.FinishedLaunching(app, options);
		}
	}
}